// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Hosting;
using Meniga.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Meniga.IdentityServer.Extensions;
using Meniga.IdentityServer.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meniga.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for a custom redirect
/// </summary>
/// <seealso cref="IdentityServer4.Hosting.IEndpointResult" />
public class CustomRedirectResult : IEndpointResult
{
    private readonly ValidatedAuthorizeRequest _request;
    private readonly string _url;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomRedirectResult"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="url">The URL.</param>
    /// <exception cref="System.ArgumentNullException">
    /// request
    /// or
    /// url
    /// </exception>
    public CustomRedirectResult(ValidatedAuthorizeRequest request, string url)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (url.IsMissing()) throw new ArgumentNullException(nameof(url));

        _request = request;
        _url = url;
    }

    internal CustomRedirectResult(
        ValidatedAuthorizeRequest request,
        string url,
        IdentityServerOptions options) 
        : this(request, url)
    {
        _options = options;
    }

    private IdentityServerOptions _options;

    private void Init(HttpContext context)
    {
        _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
    }

    /// <summary>
    /// Executes the result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns></returns>
    public Task ExecuteAsync(HttpContext context)
    {
        Init(context);

        var returnUrl = context.GetIdentityServerBasePath().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.Authorize;
        returnUrl = returnUrl.AddQueryString(_request.Raw.ToQueryString());

        if (!_url.IsLocalUrl())
        {
            // this converts the relative redirect path to an absolute one if we're 
            // redirecting to a different server
            returnUrl = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
        }

        var url = _url.AddQueryString(_options.UserInteraction.CustomRedirectReturnUrlParameter, returnUrl);
        context.Response.RedirectToAbsoluteUrl(url);

        return Task.CompletedTask;
    }
}