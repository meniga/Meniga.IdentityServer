// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Services;
using Meniga.IdentityServer.Stores;
using Microsoft.Extensions.Logging;

namespace Meniga.IdentityServer.Validation;

/// <summary>
/// Default custom token validator
/// </summary>
public class DefaultCustomTokenValidator : ICustomTokenValidator
{
    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// The user service
    /// </summary>
    protected readonly IProfileService Profile;

    /// <summary>
    /// The client store
    /// </summary>
    protected readonly IClientStore Clients;

    /// <summary>
    /// Custom validation logic for access tokens.
    /// </summary>
    /// <param name="result">The validation result so far.</param>
    /// <returns>
    /// The validation result
    /// </returns>
    public virtual Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result)
    {
        return Task.FromResult(result);
    }

    /// <summary>
    /// Custom validation logic for identity tokens.
    /// </summary>
    /// <param name="result">The validation result so far.</param>
    /// <returns>
    /// The validation result
    /// </returns>
    public virtual Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result)
    {
        return Task.FromResult(result);
    }
}