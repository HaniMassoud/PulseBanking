using MediatR;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Interfaces;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using System.Reflection;

namespace PulseBanking.Application.Common.Behaviors;

public class TenantValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantValidator _tenantValidator;
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantValidationBehavior<TRequest, TResponse>> _logger;

    public TenantValidationBehavior(
        ITenantValidator tenantValidator,
        ITenantService tenantService,
        ILogger<TenantValidationBehavior<TRequest, TResponse>> logger)
    {
        _tenantValidator = tenantValidator;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Check if the request is for the tenant registration endpoint
        if (request is ICreateTenantRequest)
        {
            _logger.LogDebug("Skipping tenant validation for tenant registration request");
            return await next();
        }

        // Get tenantId
        var tenant = _tenantService.GetCurrentTenant();
        if (tenant == null)
        {
            _logger.LogWarning("No tenant found in the current context");
            return await next();
        }
        var tenantId = tenant.Id;

        //var tenantId = await GetTenantIdAsync(request, cancellationToken);
        await _tenantValidator.ValidateAsync(cancellationToken);
        return await next();
    }

    //private async Task<string> GetTenantIdAsync(TRequest request, CancellationToken cancellationToken)
    //{
    //    var tenant = _tenantService.GetCurrentTenant();
    //    if (tenant == null)
    //    {
    //        _logger.LogWarning("No tenant found in the current context");
    //        return string.Empty;
    //    }

    //    return tenant.Id;
    //}
}

//public class TenantValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
//    where TRequest : notnull
//{
//    private readonly ITenantService _tenantService;
//    private readonly ITenantManager _tenantManager;
//    private readonly ILogger<TenantValidationBehavior<TRequest, TResponse>> _logger;

//    public TenantValidationBehavior(
//        ITenantService tenantService,
//        ITenantManager tenantManager,
//        ILogger<TenantValidationBehavior<TRequest, TResponse>> logger)
//    {
//        _tenantService = tenantService;
//        _tenantManager = tenantManager;
//        _logger = logger;
//    }

//    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//    {
//        var tenant = _tenantService.GetCurrentTenant();

//        if (tenant == null || string.IsNullOrEmpty(tenant.Id))
//        {
//            _logger.LogWarning("No tenant found in the current context");
//            throw new UnauthorizedAccessException("No tenant found in the current context");
//        }

//        var tenantExists = await _tenantManager.TenantExistsAsync(tenant.Id);
//        if (!tenantExists)
//        {
//            _logger.LogWarning("Invalid tenant ID: {TenantId}", tenant.Id);
//            throw new UnauthorizedAccessException($"Invalid tenant ID: {tenant.Id}");
//        }

//        var tenantDetails = await _tenantManager.GetTenantAsync(tenant.Id);
//        if (!tenantDetails.IsActive)
//        {
//            _logger.LogWarning("Tenant {TenantId} is inactive", tenant.Id);
//            throw new UnauthorizedAccessException($"Tenant {tenant.Id} is inactive");
//        }

//        return await next();
//    }
//}