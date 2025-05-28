// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Meniga.IdentityServer.Configuration;

#pragma warning disable 1591

namespace Meniga.IdentityServer.Hosting;

public class BaseUrlMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IdentityServerOptions _options;

    public BaseUrlMiddleware(RequestDelegate next, IdentityServerOptions options)
    {
        _next = next;
        _options = options;
    }

    public Task Invoke(HttpContext context)
    {
        var request = context.Request;
            
        context.SetIdentityServerBasePath(request.PathBase.Value.RemoveTrailingSlash());

        return _next(context);
    }
}