﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using System.Threading.Tasks;
using Meniga.IdentityServer.Models;
using Meniga.IdentityServer.Services;

namespace IdentityServer.UnitTests.Common
{
    public class MockReturnUrlParser : ReturnUrlParser
    {
        public AuthorizationRequest AuthorizationRequestResult { get; set; }
        public bool IsValidReturnUrlResult { get; set; }

        public MockReturnUrlParser() : base(Enumerable.Empty<IReturnUrlParser>())
        {
        }

        public override Task<AuthorizationRequest> ParseAsync(string returnUrl)
        {
            return Task.FromResult(AuthorizationRequestResult);
        }

        public override bool IsValidReturnUrl(string returnUrl)
        {
            return IsValidReturnUrlResult;
        }
    }
}
