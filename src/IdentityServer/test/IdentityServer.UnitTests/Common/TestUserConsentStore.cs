﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Meniga.IdentityServer.Models;
using Meniga.IdentityServer.Services;
using Meniga.IdentityServer.Stores;
using Meniga.IdentityServer.Stores.Serialization;

namespace IdentityServer.UnitTests.Common
{
    public class TestUserConsentStore : IUserConsentStore
    {
        private DefaultUserConsentStore _userConsentStore;
        private InMemoryPersistedGrantStore _grantStore = new InMemoryPersistedGrantStore();

        public TestUserConsentStore()
        {
            _userConsentStore = new DefaultUserConsentStore(
               _grantStore,
               new PersistentGrantSerializer(),
                new DefaultHandleGenerationService(),
               TestLogger.Create<DefaultUserConsentStore>());
        }

        public Task StoreUserConsentAsync(Consent consent)
        {
            return _userConsentStore.StoreUserConsentAsync(consent);
        }

        public Task<Consent> GetUserConsentAsync(string subjectId, string clientId)
        {
            return _userConsentStore.GetUserConsentAsync(subjectId, clientId);
        }

        public Task RemoveUserConsentAsync(string subjectId, string clientId)
        {
            return _userConsentStore.RemoveUserConsentAsync(subjectId, clientId);
        }
    }
}
