﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Meniga.IdentityServer.Extensions;
using Meniga.IdentityServer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;

namespace Meniga.IdentityServer.Validation;

/// <summary>
/// Validates secrets using the registered validators
/// </summary>
public class SecretValidator : ISecretsListValidator
{
    private readonly ILogger _logger;
    private readonly IEnumerable<ISecretValidator> _validators;
    private readonly TimeProvider _clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretValidator"/> class.
    /// </summary>
    /// <param name="clock">The clock.</param>
    /// <param name="validators">The validators.</param>
    /// <param name="logger">The logger.</param>
    public SecretValidator(TimeProvider clock, IEnumerable<ISecretValidator> validators, ILogger<ISecretsListValidator> logger)
    {
        _clock = clock;
        _validators = validators;
        _logger = logger;
    }

    /// <summary>
    /// Validates the secret.
    /// </summary>
    /// <param name="parsedSecret">The parsed secret.</param>
    /// <param name="secrets">The secrets.</param>
    /// <returns></returns>
    public async Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
    {
        var secretsArray = secrets as Secret[] ?? secrets.ToArray();

        var expiredSecrets = secretsArray.Where(s => s.Expiration.HasExpired(_clock.GetUtcNow().UtcDateTime)).ToList();
        if (expiredSecrets.Any())
        {
            expiredSecrets.ForEach(
                ex => _logger.LogInformation("Secret [{description}] is expired", ex.Description ?? "no description"));
        }

        var currentSecrets = secretsArray.Where(s => !s.Expiration.HasExpired(_clock.GetUtcNow().UtcDateTime)).ToArray();

        // see if a registered validator can validate the secret
        foreach (var validator in _validators)
        {
            var secretValidationResult = await validator.ValidateAsync(currentSecrets, parsedSecret);

            if (secretValidationResult.Success)
            {
                _logger.LogDebug("Secret validator success: {0}", validator.GetType().Name);
                return secretValidationResult;
            }
        }

        _logger.LogDebug("Secret validators could not validate secret");
        return new SecretValidationResult { Success = false };
    }
}