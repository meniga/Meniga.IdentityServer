// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Extensions;
using Meniga.IdentityServer.Models;
using Meniga.IdentityServer.Services;
using Meniga.IdentityServer.Stores.Serialization;
using Microsoft.Extensions.Logging;

namespace Meniga.IdentityServer.Stores;

/// <summary>
/// Base class for persisting grants using the IPersistedGrantStore.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultGrantStore<T>
{
    /// <summary>
    /// The grant type being stored.
    /// </summary>
    protected string GrantType { get; }

    /// <summary>
    /// The logger.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// The PersistedGrantStore.
    /// </summary>
    protected IPersistedGrantStore Store { get; }

    /// <summary>
    /// The PersistentGrantSerializer;
    /// </summary>
    protected IPersistentGrantSerializer Serializer { get; }

    /// <summary>
    /// The HandleGenerationService.
    /// </summary>
    protected IHandleGenerationService HandleGenerationService { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultGrantStore{T}"/> class.
    /// </summary>
    /// <param name="grantType">Type of the grant.</param>
    /// <param name="store">The store.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="handleGenerationService">The handle generation service.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="System.ArgumentNullException">grantType</exception>
    protected DefaultGrantStore(string grantType,
        IPersistedGrantStore store,
        IPersistentGrantSerializer serializer,
        IHandleGenerationService handleGenerationService,
        ILogger logger)
    {
        if (grantType.IsMissing()) throw new ArgumentNullException(nameof(grantType));

        GrantType = grantType;
        Store = store;
        Serializer = serializer;
        HandleGenerationService = handleGenerationService;
        Logger = logger;
    }

    private const string KeySeparator = ":";

    /// <summary>
    /// Gets the hashed key.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    protected virtual string GetHashedKey(string value)
    {
        return (value + KeySeparator + GrantType).Sha256();
    }

    /// <summary>
    /// Gets the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected virtual async Task<T> GetItemAsync(string key)
    {
        var hashedKey = GetHashedKey(key);

        var grant = await Store.GetAsync(hashedKey);
        if (grant != null && grant.Type == GrantType)
        {
            try
            {
                return Serializer.Deserialize<T>(grant.Data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to deserialize JSON from grant store.");
            }
        }
        else
        {
            Logger.LogDebug("{grantType} grant with value: {key} not found in store.", GrantType, key);
        }

        return default;
    }

    /// <summary>
    /// Creates the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="description">The description.</param>
    /// <param name="created">The created.</param>
    /// <param name="lifetime">The lifetime.</param>
    /// <returns></returns>
    protected virtual async Task<string> CreateItemAsync(T item, string clientId, string subjectId, string sessionId, string description, DateTime created, int lifetime)
    {
        var handle = await HandleGenerationService.GenerateAsync();
        await StoreItemAsync(handle, item, clientId, subjectId, sessionId, description, created, created.AddSeconds(lifetime));
        return handle;
    }

    /// <summary>
    /// Stores the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="item">The item.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="description">The description.</param>
    /// <param name="created">The created time.</param>
    /// <param name="expiration">The expiration.</param>
    /// <param name="consumedTime">The consumed time.</param>
    /// <returns></returns>
    protected virtual Task StoreItemAsync(string key, T item, string clientId, string subjectId, string sessionId, string description, DateTime created, DateTime? expiration, DateTime? consumedTime = null)
    {
        key = GetHashedKey(key);

        var json = Serializer.Serialize(item);

        var grant = new PersistedGrant
        {
            Key = key,
            Type = GrantType,
            ClientId = clientId,
            SubjectId = subjectId,
            SessionId = sessionId,
            Description = description,
            CreationTime = created,
            Expiration = expiration,
            ConsumedTime = consumedTime,
            Data = json
        };

        return Store.StoreAsync(grant);
    }

    /// <summary>
    /// Removes the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected virtual Task RemoveItemAsync(string key)
    {
        key = GetHashedKey(key);
        return Store.RemoveAsync(key);
    }

    /// <summary>
    /// Removes all items for a subject id / cliend id combination.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns></returns>
    protected virtual Task RemoveAllAsync(string subjectId, string clientId)
    {
        return Store.RemoveAllAsync(new PersistedGrantFilter
        {
            SubjectId = subjectId,
            ClientId = clientId,
            Type = GrantType
        });
    }
}