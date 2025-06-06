﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityModel;
using Meniga.IdentityServer.Extensions;
using Meniga.IdentityServer.Models;
using Meniga.IdentityServer.Services;
using Meniga.IdentityServer.Stores;
using Meniga.IdentityServer.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Meniga.IdentityServer.Configuration;

namespace Meniga.IdentityServer.ResponseHandling;

/// <summary>
/// The authorize response generator
/// </summary>
/// <seealso cref="IdentityServer4.ResponseHandling.IAuthorizeResponseGenerator" />
public class AuthorizeResponseGenerator : IAuthorizeResponseGenerator
{
    /// <summary>
    /// The token service
    /// </summary>
    protected readonly ITokenService TokenService;

    /// <summary>
    /// The authorization code store
    /// </summary>
    protected readonly IAuthorizationCodeStore AuthorizationCodeStore;

    /// <summary>
    /// The event service
    /// </summary>
    protected readonly IEventService Events;

    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// The clock
    /// </summary>
    protected readonly TimeProvider Clock;

    /// <summary>
    /// The key material service
    /// </summary>
    protected readonly IKeyMaterialService KeyMaterialService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeResponseGenerator"/> class.
    /// </summary>
    /// <param name="clock">The clock.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="tokenService">The token service.</param>
    /// <param name="keyMaterialService"></param>
    /// <param name="authorizationCodeStore">The authorization code store.</param>
    /// <param name="events">The events.</param>
    public AuthorizeResponseGenerator(
        TimeProvider clock,
        ITokenService tokenService,
        IKeyMaterialService keyMaterialService,
        IAuthorizationCodeStore authorizationCodeStore,
        ILogger<AuthorizeResponseGenerator> logger,
        IEventService events)
    {
        Clock = clock;
        TokenService = tokenService;
        KeyMaterialService = keyMaterialService;
        AuthorizationCodeStore = authorizationCodeStore;
        Events = events;
        Logger = logger;
    }

    /// <summary>
    /// Creates the response
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    /// <exception cref="System.InvalidOperationException">invalid grant type: " + request.GrantType</exception>
    public virtual Task<AuthorizeResponse> CreateResponseAsync(ValidatedAuthorizeRequest request)
    {
        if (request.GrantType == GrantType.AuthorizationCode)
        {
            return CreateCodeFlowResponseAsync(request);
        }
        if (request.GrantType == GrantType.Implicit)
        {
            return CreateImplicitFlowResponseAsync(request);
        }
        if (request.GrantType == GrantType.Hybrid)
        {
            return CreateHybridFlowResponseAsync(request);
        }

        Logger.LogError("Unsupported grant type: " + request.GrantType);
        throw new InvalidOperationException("invalid grant type: " + request.GrantType);
    }

    /// <summary>
    /// Creates the response for a hybrid flow request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected virtual async Task<AuthorizeResponse> CreateHybridFlowResponseAsync(ValidatedAuthorizeRequest request)
    {
        Logger.LogDebug("Creating Hybrid Flow response.");

        var code = await CreateCodeAsync(request);
        var id = await AuthorizationCodeStore.StoreAuthorizationCodeAsync(code);

        var response = await CreateImplicitFlowResponseAsync(request, id);
        response.Code = id;

        return response;
    }

    /// <summary>
    /// Creates the response for a code flow request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected virtual async Task<AuthorizeResponse> CreateCodeFlowResponseAsync(ValidatedAuthorizeRequest request)
    {
        Logger.LogDebug("Creating Authorization Code Flow response.");

        var code = await CreateCodeAsync(request);
        var id = await AuthorizationCodeStore.StoreAuthorizationCodeAsync(code);

        var response = new AuthorizeResponse
        {
            Request = request,
            Code = id,
            SessionState = request.GenerateSessionStateValue()
        };

        return response;
    }

    /// <summary>
    /// Creates the response for a implicit flow request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="authorizationCode"></param>
    /// <returns></returns>
    protected virtual async Task<AuthorizeResponse> CreateImplicitFlowResponseAsync(ValidatedAuthorizeRequest request, string authorizationCode = null)
    {
        Logger.LogDebug("Creating Implicit Flow response.");

        string accessTokenValue = null;
        var accessTokenLifetime = 0;

        var responseTypes = request.ResponseType.FromSpaceSeparatedString();

        if (responseTypes.Contains(OidcConstants.ResponseTypes.Token))
        {
            var tokenRequest = new TokenCreationRequest
            {
                Subject = request.Subject,
                ValidatedResources = request.ValidatedResources,

                ValidatedRequest = request
            };

            var accessToken = await TokenService.CreateAccessTokenAsync(tokenRequest);
            accessTokenLifetime = accessToken.Lifetime;

            accessTokenValue = await TokenService.CreateSecurityTokenAsync(accessToken);
        }

        string jwt = null;
        if (responseTypes.Contains(OidcConstants.ResponseTypes.IdToken))
        {
            string stateHash = null;
            if (request.State.IsPresent())
            {
                var credential = await KeyMaterialService.GetSigningCredentialsAsync(request.Client.AllowedIdentityTokenSigningAlgorithms);
                if (credential == null)
                {
                    throw new InvalidOperationException("No signing credential is configured.");
                }

                var algorithm = credential.Algorithm;
                stateHash = CryptoHelper.CreateHashClaimValue(request.State, algorithm);
            }

            var tokenRequest = new TokenCreationRequest
            {
                ValidatedRequest = request,
                Subject = request.Subject,
                ValidatedResources = request.ValidatedResources,
                Nonce = request.Raw.Get(OidcConstants.AuthorizeRequest.Nonce),
                IncludeAllIdentityClaims = !request.AccessTokenRequested,
                AccessTokenToHash = accessTokenValue,
                AuthorizationCodeToHash = authorizationCode,
                StateHash = stateHash
            };

            var idToken = await TokenService.CreateIdentityTokenAsync(tokenRequest);
            jwt = await TokenService.CreateSecurityTokenAsync(idToken);
        }

        var response = new AuthorizeResponse
        {
            Request = request,
            AccessToken = accessTokenValue,
            AccessTokenLifetime = accessTokenLifetime,
            IdentityToken = jwt,
            SessionState = request.GenerateSessionStateValue()
        };

        return response;
    }

    /// <summary>
    /// Creates an authorization code
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected virtual async Task<AuthorizationCode> CreateCodeAsync(ValidatedAuthorizeRequest request)
    {
        string stateHash = null;
        if (request.State.IsPresent())
        {
            var credential = await KeyMaterialService.GetSigningCredentialsAsync(request.Client.AllowedIdentityTokenSigningAlgorithms);
            if (credential == null)
            {
                throw new InvalidOperationException("No signing credential is configured.");
            }

            var algorithm = credential.Algorithm;
            stateHash = CryptoHelper.CreateHashClaimValue(request.State, algorithm);
        }

        var code = new AuthorizationCode
        {
            CreationTime = Clock.GetUtcNow().UtcDateTime,
            ClientId = request.Client.ClientId,
            Lifetime = request.Client.AuthorizationCodeLifetime,
            Subject = request.Subject,
            SessionId = request.SessionId,
            Description = request.Description,
            CodeChallenge = request.CodeChallenge.Sha256(),
            CodeChallengeMethod = request.CodeChallengeMethod,

            IsOpenId = request.IsOpenIdRequest,
            RequestedScopes = request.ValidatedResources.RawScopeValues,
            RedirectUri = request.RedirectUri,
            Nonce = request.Nonce,
            StateHash = stateHash,

            WasConsentShown = request.WasConsentShown
        };

        return code;
    }
}