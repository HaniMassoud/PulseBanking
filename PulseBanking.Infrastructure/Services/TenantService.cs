// Create new file: src/PulseBanking.Infrastructure/Services/TenantService.cs (if it doesn't exist already)
using Microsoft.AspNetCore.Http;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantManager _tenantManager;
    private readonly string? _fixedTenantId;
    private string? _currentTenantId;  // Add this field

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        ITenantManager tenantManager,
        string? fixedTenantId = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantManager = tenantManager;
        _fixedTenantId = fixedTenantId;

        // Capture the tenant ID immediately if available
        if (_fixedTenantId == null && httpContextAccessor.HttpContext != null)
        {
            _currentTenantId = httpContextAccessor.HttpContext.Request.Headers["X-TenantId"].FirstOrDefault();
        }
    }

    public string GetCurrentTenant()
    {
        if (!string.IsNullOrEmpty(_fixedTenantId))
            return _fixedTenantId;

        if (!string.IsNullOrEmpty(_currentTenantId))
            return _currentTenantId;

        var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-TenantId"].FirstOrDefault();

        if (string.IsNullOrEmpty(tenantId))
        {
            throw new UnauthorizedAccessException("TenantId not specified");
        }

        _currentTenantId = tenantId;  // Store for future use
        return tenantId;
    }
}