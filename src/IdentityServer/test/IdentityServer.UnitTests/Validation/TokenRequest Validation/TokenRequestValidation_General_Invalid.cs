// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Meniga.IdentityModel;
using IdentityServer.UnitTests.Validation.Setup;
using Meniga.IdentityServer;
using Meniga.IdentityServer.Models;
using Meniga.IdentityServer.Stores;
using Xunit;

namespace IdentityServer.UnitTests.Validation.TokenRequest_Validation
{
    public class TokenRequestValidation_General_Invalid
    {
        private const string Category = "TokenRequest Validation - General - Invalid";

        private IClientStore _clients = new InMemoryClientStore(TestClients.Get());
        private ClaimsPrincipal _subject = new IdentityServerUser("bob").CreatePrincipal();

        [Fact]
        [Trait("Category", Category)]
        public async Task Parameters_Null()
        {
            var validator = Factory.CreateTokenRequestValidator();

            Func<Task> act = () => validator.ValidateRequestAsync(null, null);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_Null()
        {
            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, "valid");
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            Func<Task> act = () => validator.ValidateRequestAsync(parameters, null);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Grant_Type()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var store = Factory.CreateAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                CreationTime = DateTime.UtcNow,
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                Subject = _subject
            };

            var handle = await store.StoreAuthorizationCodeAsync(code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "unknown");
            parameters.Add(OidcConstants.TokenRequest.Code, handle);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Protocol_Type()
        {
            var client = await _clients.FindEnabledClientByIdAsync("client.cred.wsfed");
            var codeStore = Factory.CreateAuthorizationCodeStore();

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore:codeStore);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "client_credentials");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.InvalidClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_Grant_Type()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var store = Factory.CreateAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                CreationTime = DateTime.UtcNow,
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                Subject = _subject
            };

            var handle = await store.StoreAuthorizationCodeAsync(code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.Code, handle);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(OidcConstants.TokenErrors.UnsupportedGrantType);
        }
    }
}