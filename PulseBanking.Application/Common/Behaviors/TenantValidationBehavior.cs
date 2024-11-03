using MediatR;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Interfaces;

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
        var tenantId = _tenantService.GetCurrentTenant();

        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID found in the current context");
            throw new UnauthorizedAccessException("No tenant ID found in the current context");
        }

        var tenantExists = await _tenantManager.TenantExistsAsync(tenantId);
        if (!tenantExists)
        {
            _logger.LogWarning("Invalid tenant ID: {TenantId}", tenantId);
            throw new UnauthorizedAccessException($"Invalid tenant ID: {tenantId}");
        }

        var tenant = await _tenantManager.GetTenantAsync(tenantId);
        if (!tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is inactive", tenantId);
            throw new UnauthorizedAccessException($"Tenant {tenantId} is inactive");
        }

        return await next();
    }
}