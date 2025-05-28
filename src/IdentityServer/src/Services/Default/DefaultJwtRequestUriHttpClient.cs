// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Net;
using Meniga.IdentityModel;
using Meniga.IdentityServer.Configuration;
using Meniga.IdentityServer.Models;
using Microsoft.Extensions.Logging;

namespace Meniga.IdentityServer.Services;

/// <summary>
///     Default JwtRequest client
/// </summary>
public class DefaultJwtRequestUriHttpClient : IJwtRequestUriHttpClient
{
    private readonly HttpClient _client;
    private readonly ILogger<DefaultJwtRequestUriHttpClient> _logger;
    private readonly IdentityServerOptions _options;

    /// <summary>
    ///     ctor
    /// </summary>
    /// <param name="client">An HTTP client</param>
    /// <param name="options">The options.</param>
    /// <param name="loggerFactory">The logger factory</param>
    public DefaultJwtRequestUriHttpClient(HttpClient client, IdentityServerOptions options,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _options = options;
        _logger = loggerFactory.CreateLogger<DefaultJwtRequestUriHttpClient>();
    }


    /// <inheritdoc />
    public async Task<string> GetJwtAsync(string url, Client client)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Options.Set(new HttpRequestOptionsKey<Client>(IdentityServerConstants.JwtRequestClientKey), client);

        var response = await _client.SendAsync(req);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            if (_options.StrictJarValidation)
                if (!string.Equals(response.Content.Headers.ContentType.MediaType,
                        $"application/{JwtClaimTypes.JwtTypes.AuthorizationRequest}", StringComparison.Ordinal))
                {
                    _logger.LogError("Invalid content type {type} from jwt url {url}",
                        response.Content.Headers.ContentType.MediaType, url);
                    return null;
                }

            _logger.LogDebug("Success http response from jwt url {url}", url);

            var json = await response.Content.ReadAsStringAsync();
            return json;
        }

        _logger.LogError("Invalid http status code {status} from jwt url {url}", response.StatusCode, url);
        return null;
    }
}