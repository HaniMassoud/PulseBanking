using MediatR;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Common.Behaviors;

public class TenantValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantService _tenantService;
    private readonly ITenantManager _tenantManager;
    private readonly ILogger<TenantValidationBehavior<TRequest, TResponse>> _logger;

    public TenantValidationBehavior(
        ITenantService tenantService,
        ITenantManager tenantManager,
        ILogger<TenantValidationBehavior<TRequest, TResponse>> logger)
    {
        _tenantService = tenantService;
        _tenantManager = tenantManager;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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

        return await next();
    }
}