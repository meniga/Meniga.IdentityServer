// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Extensions;
using Meniga.IdentityServer.Models;

namespace Meniga.IdentityServer.Stores;

/// <summary>
/// In-memory resource store
/// </summary>
public class InMemoryResourcesStore : IResourceStore
{
    private readonly IEnumerable<IdentityResource> _identityResources;
    private readonly IEnumerable<ApiResource> _apiResources;
    private readonly IEnumerable<ApiScope> _apiScopes;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryResourcesStore" /> class.
    /// </summary>
    public InMemoryResourcesStore(
        IEnumerable<IdentityResource> identityResources = null, 
        IEnumerable<ApiResource> apiResources = null, 
        IEnumerable<ApiScope> apiScopes = null)
    {
        if (identityResources?.HasDuplicates(m => m.Name) == true)
        {
            throw new ArgumentException("Identity resources must not contain duplicate names");
        }

        if (apiResources?.HasDuplicates(m => m.Name) == true)
        {
            throw new ArgumentException("Api resources must not contain duplicate names");
        }
            
        if (apiScopes?.HasDuplicates(m => m.Name) == true)
        {
            throw new ArgumentException("Scopes must not contain duplicate names");
        }

        _identityResources = identityResources ?? [];
        _apiResources = apiResources ?? [];
        _apiScopes = apiScopes ?? [];
    }

    /// <inheritdoc/>
    public Task<Resources> GetAllResourcesAsync()
    {
        var result = new Resources(_identityResources, _apiResources, _apiScopes);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
    {
        ArgumentNullException.ThrowIfNull(apiResourceNames);

        var query = from a in _apiResources
            where apiResourceNames.Contains(a.Name)
            select a;
        return Task.FromResult(query);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        ArgumentNullException.ThrowIfNull(scopeNames);

        var identity = from i in _identityResources
            where scopeNames.Contains(i.Name)
            select i;

        return Task.FromResult(identity);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        ArgumentNullException.ThrowIfNull(scopeNames);

        var query = from a in _apiResources
            where a.Scopes.Any(scopeNames.Contains)
            select a;

        return Task.FromResult(query);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
    {
        ArgumentNullException.ThrowIfNull(scopeNames);

        var query =
            from x in _apiScopes
            where scopeNames.Contains(x.Name)
            select x;
            
        return Task.FromResult(query);
    }
}