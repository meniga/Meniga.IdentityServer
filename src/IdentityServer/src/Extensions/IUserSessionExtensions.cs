// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Extensions;
using Meniga.IdentityServer.Models;

namespace Meniga.IdentityServer.Services;

/// <summary>
/// Extension for IUserSession.
/// </summary>
public static class IUserSessionExtensions
{
    /// <summary>
    /// Creates a LogoutNotificationContext for the current user session.
    /// </summary>
    /// <returns></returns>
    public static async Task<LogoutNotificationContext> GetLogoutNotificationContext(this IUserSession session)
    {
        var clientIds = await session.GetClientListAsync();

        if (clientIds.Any())
        {
            var user = await session.GetUserAsync();
            var sub = user.GetSubjectId();
            var sid = await session.GetSessionIdAsync();

            return new LogoutNotificationContext
            {
                SubjectId = sub,
                SessionId = sid,
                ClientIds = clientIds
            };
        }

        return null;
    }
}