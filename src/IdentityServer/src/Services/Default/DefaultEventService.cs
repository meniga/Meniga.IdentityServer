// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Meniga.IdentityServer.Configuration;
using Meniga.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;

namespace Meniga.IdentityServer.Events;

/// <summary>
/// The default event service
/// </summary>
/// <seealso cref="IdentityServer4.Services.IEventService" />
public class DefaultEventService : IEventService
{
    /// <summary>
    /// The options
    /// </summary>
    protected readonly IdentityServerOptions Options;

    /// <summary>
    /// The context
    /// </summary>
    protected readonly IHttpContextAccessor Context;

    /// <summary>
    /// The sink
    /// </summary>
    protected readonly IEventSink Sink;

    /// <summary>
    /// The clock
    /// </summary>
    protected readonly TimeProvider Clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEventService"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="context">The context.</param>
    /// <param name="sink">The sink.</param>
    /// <param name="clock">The clock.</param>
    public DefaultEventService(IdentityServerOptions options, IHttpContextAccessor context, IEventSink sink, TimeProvider clock)
    {
        Options = options;
        Context = context;
        Sink = sink;
        Clock = clock;
    }

    /// <summary>
    /// Raises the specified event.
    /// </summary>
    /// <param name="evt">The event.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException">evt</exception>
    public async Task RaiseAsync(Event evt)
    {
        ArgumentNullException.ThrowIfNull(evt);

        if (CanRaiseEvent(evt))
        {
            await PrepareEventAsync(evt);
            await Sink.PersistAsync(evt);
        }
    }

    /// <summary>
    /// Indicates if the type of event will be persisted.
    /// </summary>
    /// <param name="evtType"></param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentOutOfRangeException"></exception>
    public bool CanRaiseEventType(EventTypes evtType)
    {
        switch (evtType)
        {
            case EventTypes.Failure:
                return Options.Events.RaiseFailureEvents;
            case EventTypes.Information:
                return Options.Events.RaiseInformationEvents;
            case EventTypes.Success:
                return Options.Events.RaiseSuccessEvents;
            case EventTypes.Error:
                return Options.Events.RaiseErrorEvents;
            default:
                throw new ArgumentOutOfRangeException(nameof(evtType));
        }
    }

    /// <summary>
    /// Determines whether this event would be persisted.
    /// </summary>
    /// <param name="evt">The evt.</param>
    /// <returns>
    ///   <c>true</c> if this event would be persisted; otherwise, <c>false</c>.
    /// </returns>
    protected virtual bool CanRaiseEvent(Event evt)
    {
        return CanRaiseEventType(evt.EventType);
    }

    /// <summary>
    /// Prepares the event.
    /// </summary>
    /// <param name="evt">The evt.</param>
    /// <returns></returns>
    protected virtual Task PrepareEventAsync(Event evt)
    {
        evt.ActivityId = Context.HttpContext.TraceIdentifier;
        evt.TimeStamp = Clock.GetUtcNow().UtcDateTime;
        evt.ProcessId = Environment.ProcessId;

        if (Context.HttpContext.Connection.LocalIpAddress != null)
        {
            evt.LocalIpAddress = Context.HttpContext.Connection.LocalIpAddress + ":" + Context.HttpContext.Connection.LocalPort;
        }
        else
        {
            evt.LocalIpAddress = "unknown";
        }

        if (Context.HttpContext.Connection.RemoteIpAddress != null)
        {
            evt.RemoteIpAddress = Context.HttpContext.Connection.RemoteIpAddress.ToString();
        }
        else
        {
            evt.RemoteIpAddress = "unknown";
        }

        return evt.PrepareAsync();
    }
}