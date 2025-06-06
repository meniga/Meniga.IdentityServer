// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Extensions;
using Meniga.IdentityServer.Models;
using Meniga.IdentityServer.Stores;
using Meniga.IdentityServer.Validation;
using Microsoft.Extensions.Logging;

namespace Meniga.IdentityServer.Services;

internal class OidcReturnUrlParser : IReturnUrlParser
{
    private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;
    private readonly ILogger _logger;
    private readonly IUserSession _userSession;
    private readonly IAuthorizeRequestValidator _validator;
        
    public OidcReturnUrlParser(
        IAuthorizeRequestValidator validator,
        IUserSession userSession,
        ILogger<OidcReturnUrlParser> logger,
        IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
    {
        _validator = validator;
        _userSession = userSession;
        _logger = logger;
        _authorizationParametersMessageStore = authorizationParametersMessageStore;
    }

    public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
    {
        if (IsValidReturnUrl(returnUrl))
        {
            var parameters = returnUrl.ReadQueryStringAsNameValueCollection();
            if (_authorizationParametersMessageStore != null)
            {
                var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
                var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
                parameters = entry?.Data.FromFullDictionary() ?? [];
            }

            var user = await _userSession.GetUserAsync();
            var result = await _validator.ValidateAsync(parameters, user);
            if (!result.IsError)
            {
                _logger.LogTrace("AuthorizationRequest being returned");
                return new AuthorizationRequest(result.ValidatedRequest);
            }
        }

        _logger.LogTrace("No AuthorizationRequest being returned");
        return null;
    }

    public bool IsValidReturnUrl(string returnUrl)
    {
        if (returnUrl.IsLocalUrl())
        {
            var index = returnUrl.IndexOf('?');
            if (index >= 0) returnUrl = returnUrl[..index];

            if (returnUrl.EndsWith(Constants.ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
                returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeCallback, StringComparison.Ordinal))
            {
                _logger.LogTrace("returnUrl is valid");
                return true;
            }
        }

        _logger.LogTrace("returnUrl is not valid");
        return false;
    }
}