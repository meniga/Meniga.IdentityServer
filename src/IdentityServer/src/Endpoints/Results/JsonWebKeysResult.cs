// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Extensions;
using Meniga.IdentityServer.Hosting;
using Meniga.IdentityServer.Models;
using Microsoft.AspNetCore.Http;

namespace Meniga.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for the jwks document
/// </summary>
/// <seealso cref="IdentityServer4.Hosting.IEndpointResult" />
public class JsonWebKeysResult : IEndpointResult
{
    /// <summary>
    /// Gets the web keys.
    /// </summary>
    /// <value>
    /// The web keys.
    /// </value>
    public IEnumerable<JsonWebKey> WebKeys { get; }

    /// <summary>
    /// Gets the maximum age.
    /// </summary>
    /// <value>
    /// The maximum age.
    /// </value>
    public int? MaxAge { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWebKeysResult" /> class.
    /// </summary>
    /// <param name="webKeys">The web keys.</param>
    /// <param name="maxAge">The maximum age.</param>
    public JsonWebKeysResult(IEnumerable<JsonWebKey> webKeys, int? maxAge)
    {
        WebKeys = webKeys ?? throw new ArgumentNullException(nameof(webKeys));
        MaxAge = maxAge;
    }

    /// <summary>
    /// Executes the result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns></returns>
    public Task ExecuteAsync(HttpContext context)
    {
        if (MaxAge.HasValue && MaxAge.Value >= 0)
        {
            context.Response.SetCache(MaxAge.Value, "Origin");
        }

        return context.Response.WriteJsonAsync(new { keys = WebKeys }, "application/json; charset=UTF-8");
    }
}