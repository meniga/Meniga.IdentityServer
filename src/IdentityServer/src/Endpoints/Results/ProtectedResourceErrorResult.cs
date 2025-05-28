// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Extensions;
using Microsoft.Extensions.Primitives;
using Meniga.IdentityServer.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Meniga.IdentityModel;

namespace Meniga.IdentityServer.Endpoints.Results;

internal class ProtectedResourceErrorResult : IEndpointResult
{
    public string Error;
    public string ErrorDescription;

    public ProtectedResourceErrorResult(string error, string errorDescription = null)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }

    public Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.SetNoCache();

        if (Constants.ProtectedResourceErrorStatusCodes.TryGetValue(Error, out var code))
        {
            context.Response.StatusCode = code;
        }

        if (Error == OidcConstants.ProtectedResourceErrors.ExpiredToken)
        {
            Error = OidcConstants.ProtectedResourceErrors.InvalidToken;
            ErrorDescription = "The access token expired";
        }

        var errorString = string.Format($"error=\"{Error}\"");
        if (ErrorDescription.IsMissing())
        {
            context.Response.Headers.Append(HeaderNames.WWWAuthenticate, new StringValues([
                "Bearer realm=\"IdentityServer\"", errorString
            ]).ToString());
        }
        else
        {
            var errorDescriptionString = string.Format($"error_description=\"{ErrorDescription}\"");
            context.Response.Headers.Append(HeaderNames.WWWAuthenticate, new StringValues([
                "Bearer realm=\"IdentityServer\"", errorString, errorDescriptionString
            ]).ToString());
        }

        return Task.CompletedTask;
    }
}