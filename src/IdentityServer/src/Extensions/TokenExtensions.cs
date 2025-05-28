// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Meniga.IdentityModel;
using Meniga.IdentityServer.Models;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Meniga.IdentityServer.Configuration;
using System.Text.Json;

namespace Meniga.IdentityServer.Extensions;

/// <summary>
/// Extensions for <see cref="Token"/>.
/// </summary>
public static class TokenExtensions
{
    /// <summary>
    /// Creates the default JWT <see cref="JwtPayload"/>.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="options">IdentityServer options.</param>
    /// <param name="logger">Logger.</param>
    /// <returns>The populated payload.</returns>
    public static JwtPayload CreateJwtPayload(
        this Token token,
        TimeProvider clock,
        IdentityServerOptions options,
        ILogger logger)
    {
        var payload = new JwtPayload(
            token.Issuer,
            null,
            null,
            clock.GetUtcNow().UtcDateTime,
            clock.GetUtcNow().UtcDateTime.AddSeconds(token.Lifetime));

        // audiences
        foreach (var aud in token.Audiences)
        {
            payload.AddClaim(new Claim(JwtClaimTypes.Audience, aud));
        }

        // segregate claim types
        var amrClaims = token.Claims.Where(c => c.Type == JwtClaimTypes.AuthenticationMethod).ToArray();
        var scopeClaims = token.Claims.Where(c => c.Type == JwtClaimTypes.Scope).ToArray();
        var jsonClaims = token.Claims
            .Where(c => c.ValueType == IdentityServerConstants.ClaimValueTypes.Json)
            .ToList();

        if (token.Confirmation.IsPresent())
        {
            jsonClaims.Add(new Claim(
                JwtClaimTypes.Confirmation,
                token.Confirmation,
                IdentityServerConstants.ClaimValueTypes.Json));
        }

        var normalClaims = token.Claims
            .Except(amrClaims)
            .Except(jsonClaims)
            .Except(scopeClaims);

        payload.AddClaims(normalClaims);

        // scopes
        if (!scopeClaims.IsNullOrEmpty())
        {
            var scopes = scopeClaims.Select(c => c.Value).ToArray();
            payload.Add(
                JwtClaimTypes.Scope,
                options.EmitScopesAsSpaceDelimitedStringInJwt
                    ? string.Join(' ', scopes)
                    : scopes);
        }

        // authentication methods
        if (!amrClaims.IsNullOrEmpty())
        {
            payload.Add(
                JwtClaimTypes.AuthenticationMethod,
                amrClaims.Select(c => c.Value).Distinct().ToArray());
        }

        // JSON-valued claims
        try
        {
            var jsonTokens = jsonClaims
                .Select(c =>
                {
                    try
                    {
                        // Parse with strict RFC-8259 rules; throws on invalid JSON.
                        using var doc = JsonDocument.Parse(c.Value);
                        return new { c.Type, Element = doc.RootElement.Clone() };
                    }
                    catch (JsonException jex)
                    {
                        logger.LogCritical(jex, "Invalid JSON in claim '{claimType}'", c.Type);
                        throw;
                    }
                })
                .ToArray();

            // ---- JSON objects --------------------------------------------------
            var jsonObjects = jsonTokens
                .Where(x => x.Element.ValueKind == JsonValueKind.Object)
                .ToArray();

            foreach (var grp in jsonObjects.GroupBy(x => x.Type))
            {
                if (payload.ContainsKey(grp.Key))
                    throw new InvalidOperationException(
                        $"Can't add two claims where one is a JSON object and the other is not ({grp.Key}).");

                if (grp.Skip(1).Any())
                {
                    // turn the IEnumerable<JsonElement> into a single JsonElement[array]
                    var arr = grp.Select(x => x.Element).ToArray();
                    var merged = JsonDocument
                        .Parse(JsonSerializer.Serialize(arr))
                        .RootElement.Clone();

                    payload.Add(grp.Key, merged);
                }
                else
                {
                    payload.Add(grp.Key, grp.First().Element);
                }
            }

            // ---- JSON arrays ---------------------------------------------------
            var jsonArrays = jsonTokens
                .Where(x => x.Element.ValueKind == JsonValueKind.Array)
                .ToArray();

            foreach (var grp in jsonArrays.GroupBy(x => x.Type))
            {
                if (payload.ContainsKey(grp.Key))
                    throw new InvalidOperationException(
                        $"Can't add two claims where one is a JSON array and the other is not ({grp.Key}).");

                // flatten all arrays for this claim-type
                var flat = grp
                    .SelectMany(x => x.Element.EnumerateArray().Select(e => e.Clone()))
                    .ToArray();

                var merged = JsonDocument
                    .Parse(JsonSerializer.Serialize(flat))
                    .RootElement.Clone();

                payload.Add(grp.Key, merged);
            }

            // ---- unsupported JSON kinds ---------------------------------------
            var unsupported = jsonTokens
                .Where(x => x.Element.ValueKind != JsonValueKind.Object
                            && x.Element.ValueKind != JsonValueKind.Array)
                .Select(x => x.Type)
                .Distinct()
                .ToArray();

            if (unsupported.Any())
            {
                throw new InvalidOperationException(
                    $"Unsupported JSON type for claim types: {string.Join(", ", unsupported)}");
            }

            return payload;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error creating a JSON-valued claim");
            throw;
        }
    }
}