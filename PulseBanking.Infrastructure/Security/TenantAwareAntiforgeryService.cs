// PulseBanking.Infrastructure/Security/TenantAwareAntiforgeryService.cs
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Security;

public class TenantAwareAntiforgeryService : IAntiforgery
{
    private readonly IAntiforgery _antiforgery;
    private readonly ITenantService _tenantService;

    public TenantAwareAntiforgeryService(IAntiforgery antiforgery, ITenantService tenantService)
    {
        _antiforgery = antiforgery;
        _tenantService = tenantService;
    }

    public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
    {
        ValidateTenantContext();
        return _antiforgery.GetAndStoreTokens(httpContext);
    }

    public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
    {
        ValidateTenantContext();
        return _antiforgery.GetTokens(httpContext);
    }

    public Task<bool> IsRequestValidAsync(HttpContext httpContext)
    {
        var tenant = _tenantService.GetCurrentTenant();
        if (tenant == null || !tenant.IsActive)
        {
            return Task.FromResult(false);
        }
        return _antiforgery.IsRequestValidAsync(httpContext);
    }

    public void SetCookieTokenAndHeader(HttpContext httpContext)
    {
        ValidateTenantContext();
        _antiforgery.SetCookieTokenAndHeader(httpContext);
    }

    public Task ValidateRequestAsync(HttpContext httpContext)
    {
        ValidateTenantContext();
        return _antiforgery.ValidateRequestAsync(httpContext);
    }

    private void ValidateTenantContext()
    {
        var tenant = _tenantService.GetCurrentTenant();
        if (tenant == null)
        {
            throw new InvalidOperationException("No tenant context found");
        }

        if (!tenant.IsActive)
        {
            throw new InvalidOperationException("Tenant is not active");
        }

        if (tenant.TrialEndsAt.HasValue && tenant.TrialEndsAt.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Tenant trial period has expired");
        }
    }
}