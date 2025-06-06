﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Models;
using System.Threading.Tasks;

namespace Meniga.IdentityServer.Stores
{
    /// <summary>
    /// Retrieval of client configuration
    /// </summary>
    public interface IClientStore
    {
        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>The client</returns>
        Task<Client> FindClientByIdAsync(string clientId);
    }
}