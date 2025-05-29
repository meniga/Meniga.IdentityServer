// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Events;
using Microsoft.Extensions.Logging;

namespace Meniga.IdentityServer.Services;

/// <summary>
/// Default implementation of the event service. Write events raised to the log.
/// </summary>
public class DefaultEventSink : IEventSink
{
    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEventSink"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public DefaultEventSink(ILogger<DefaultEventService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Raises the specified event.
    /// </summary>
    /// <param name="evt">The event.</param>
    /// <exception cref="System.ArgumentNullException">evt</exception>
    public virtual Task PersistAsync(Event evt)
    {
        ArgumentNullException.ThrowIfNull(evt);

        _logger.LogInformation("{@event}", evt);

        return Task.CompletedTask;
    }
}