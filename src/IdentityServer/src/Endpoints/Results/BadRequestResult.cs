﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Hosting;
using Microsoft.AspNetCore.Http;
using Meniga.IdentityServer.Extensions;

namespace Meniga.IdentityServer.Endpoints.Results;

internal class BadRequestResult : IEndpointResult
{
    public string Error { get; set; }
    public string ErrorDescription { get; set; }

    public BadRequestResult(string error = null, string errorDescription = null)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }

    public Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = 400;
        context.Response.SetNoCache();

        if (Error.IsPresent())
        {
            var dto = new ResultDto
            {
                error = Error,
                error_description = ErrorDescription
            };

            return context.Response.WriteJsonAsync(dto);
        }

        return Task.CompletedTask;
    }

    internal class ResultDto
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }    
}