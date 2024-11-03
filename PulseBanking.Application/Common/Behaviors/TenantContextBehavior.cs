using MediatR;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Interfaces;
using System.Globalization;

namespace PulseBanking.Application.Common.Behaviors;

public class TenantContextBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantService _tenantService;
    private readonly ITenantManager _tenantManager;
    private readonly ILogger<TenantContextBehavior<TRequest, TResponse>> _logger;

    public TenantContextBehavior(
        ITenantService tenantService,
        ITenantManager tenantManager,
        ILogger<TenantContextBehavior<TRequest, TResponse>> logger)
    {
        _tenantService = tenantService;
        _tenantManager = tenantManager;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenant();

        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenant = await _tenantManager.GetTenantAsync(tenantId);

            // Set thread's current culture based on tenant settings if needed
            if (!string.IsNullOrEmpty(tenant.TimeZone))
            {
                try
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(tenant.TimeZone);
                    // Note: We can't set the system time zone, but we can use it for date/time conversions
                    // Store it in the ambient context if needed
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(tenant.TimeZone);
                }
                catch (TimeZoneNotFoundException ex)
                {
                    _logger.LogWarning(ex, "Invalid timezone {TimeZone} specified for tenant {TenantId}",
                        tenant.TimeZone, tenantId);
                }
            }

            _logger.LogInformation(
                "Request processing for Tenant: {TenantId}, Type: {RequestType}",
                tenantId,
                typeof(TRequest).Name);
        }

        return await next();
    }
}