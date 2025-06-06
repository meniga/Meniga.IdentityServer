﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityModel;
using Meniga.IdentityServer.Validation;
using Microsoft.AspNetCore.Authentication;

namespace Meniga.IdentityServer.Test;

/// <summary>
/// Resource owner password validator for test users
/// </summary>
/// <seealso cref="IdentityServer4.Validation.IResourceOwnerPasswordValidator" />
public class TestUserResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly TestUserStore _users;
    private readonly TimeProvider _clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestUserResourceOwnerPasswordValidator"/> class.
    /// </summary>
    /// <param name="users">The users.</param>
    /// <param name="clock">The clock.</param>
    public TestUserResourceOwnerPasswordValidator(TestUserStore users, TimeProvider clock)
    {
        _users = users;
        _clock = clock;
    }

    /// <summary>
    /// Validates the resource owner password credential
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        if (_users.ValidateCredentials(context.UserName, context.Password))
        {
            var user = _users.FindByUsername(context.UserName);
            ArgumentNullException.ThrowIfNull("Subject ID not set", nameof(user.SubjectId));
            
            context.Result = new GrantValidationResult(
                user.SubjectId, 
                OidcConstants.AuthenticationMethods.Password, _clock.GetUtcNow().UtcDateTime, 
                user.Claims);
        }

        return Task.CompletedTask;
    }
}