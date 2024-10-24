// Create new file: src/PulseBanking.Infrastructure/Services/TenantService.cs
using Microsoft.AspNetCore.Http;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentTenant()
    {
        var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-TenantId"].FirstOrDefault();
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new UnauthorizedAccessException("TenantId not specified");
        }
        return tenantId;
    }
}