// Create new file: src/PulseBanking.Infrastructure/Services/TenantService.cs (if it doesn't exist already)
using Microsoft.AspNetCore.Http;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantManager _tenantManager;
    private readonly string? _fixedTenantId;

    public TenantService(IHttpContextAccessor httpContextAccessor, ITenantManager tenantManager, string? fixedTenantId = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantManager = tenantManager;
        _fixedTenantId = fixedTenantId;
    }

    public string GetCurrentTenant()
    {
        // If we have a fixed tenant ID (for testing or seeding), use it
        if (!string.IsNullOrEmpty(_fixedTenantId))
            return _fixedTenantId;

        var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-TenantId"].FirstOrDefault();
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new UnauthorizedAccessException("TenantId not specified");
        }

        return tenantId;
    }
}