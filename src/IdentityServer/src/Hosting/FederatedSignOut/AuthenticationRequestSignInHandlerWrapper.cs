﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Meniga.IdentityServer.Hosting.FederatedSignOut;

internal class AuthenticationRequestSignInHandlerWrapper : AuthenticationRequestSignOutHandlerWrapper, IAuthenticationSignInHandler
{
    private readonly IAuthenticationSignInHandler _inner;

    public AuthenticationRequestSignInHandlerWrapper(IAuthenticationSignInHandler inner, IHttpContextAccessor httpContextAccessor)
        : base(inner, httpContextAccessor)
    {
        _inner = inner;
    }

    public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
    {
        return _inner.SignInAsync(user, properties);
    }
}