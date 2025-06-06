﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Meniga.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meniga.IdentityServer.EntityFramework.Interfaces
{
    /// <summary>
    /// Abstraction for the configuration context.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IConfigurationDbContext : IDisposable
    {
        /// <summary>
        /// Gets or sets the clients.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        DbSet<Client> Clients { get; set; }
        
        /// <summary>
        /// Gets or sets the clients' CORS origins.
        /// </summary>
        /// <value>
        /// The clients CORS origins.
        /// </value>
        DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }

        /// <summary>
        /// Gets or sets the identity resources.
        /// </summary>
        /// <value>
        /// The identity resources.
        /// </value>
        DbSet<IdentityResource> IdentityResources { get; set; }

        /// <summary>
        /// Gets or sets the API resources.
        /// </summary>
        /// <value>
        /// The API resources.
        /// </value>
        DbSet<ApiResource> ApiResources { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The identity resources.
        /// </value>
        DbSet<ApiScope> ApiScopes { get; set; }
    }
}