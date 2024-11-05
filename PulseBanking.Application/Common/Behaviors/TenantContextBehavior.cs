using MediatR;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Interfaces;
using System.Globalization;
using PulseBanking.Domain.Entities;
using PulseBanking.Application.Common.Interfaces;

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
        // Check if the request is for the tenant registration endpoint
        if (request is ICreateTenantRequest)
        {
            _logger.LogDebug("Skipping tenant context setup for tenant registration request");
            return await next();
        }

        var tenant = _tenantService.GetCurrentTenant();

        if (tenant != null && !string.IsNullOrEmpty(tenant.Id))
        {
            var tenantDetails = await _tenantManager.GetTenantAsync(tenant.Id);

            // Set thread's current culture based on tenant settings if needed
            if (!string.IsNullOrEmpty(tenantDetails.TimeZone))
            {
                try
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(tenantDetails.TimeZone);
                    // Note: We can't set the system time zone, but we can use it for date/time conversions
                    // Store it in the ambient context if needed
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(tenantDetails.TimeZone);
                }
                catch (TimeZoneNotFoundException ex)
                {
                    _logger.LogWarning(ex, "Invalid timezone {TimeZone} specified for tenant {TenantId}",
                        tenantDetails.TimeZone, tenant.Id);
                }
            }

            _logger.LogInformation(
                "Request processing for Tenant: {TenantId}, Type: {RequestType}",
                tenant.Id,
                typeof(TRequest).Name);
        }

        return await next();
    }
}