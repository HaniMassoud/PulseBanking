using Microsoft.Extensions.Logging;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Infrastructure.Services;

public class TenantValidator : ITenantValidator
{
    private readonly ITenantService _tenantService;
    private readonly ITenantManager _tenantManager;
    private readonly ILogger<TenantValidator> _logger;

    public TenantValidator(
        ITenantService tenantService,
        ITenantManager tenantManager,
        ILogger<TenantValidator> logger)
    {
        _tenantService = tenantService;
        _tenantManager = tenantManager;
        _logger = logger;
    }

    public async Task ValidateAsync(CancellationToken cancellationToken)
    {
        var tenant = _tenantService.GetCurrentTenant();
        if (tenant == null || string.IsNullOrEmpty(tenant.Id))
        {
            _logger.LogWarning("No tenant found in the current context");
            throw new UnauthorizedAccessException("No tenant found in the current context");
        }

        var tenantExists = await _tenantManager.TenantExistsAsync(tenant.Id);
        if (!tenantExists)
        {
            _logger.LogWarning("Invalid tenant ID: {TenantId}", tenant.Id);
            throw new UnauthorizedAccessException($"Invalid tenant ID: {tenant.Id}");
        }

        var tenantDetails = await _tenantManager.GetTenantAsync(tenant.Id);
        if (!tenantDetails.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is inactive", tenant.Id);
            throw new UnauthorizedAccessException($"Tenant {tenant.Id} is inactive");
        }
    }
}