// Create new file: src/PulseBanking.Api/Controllers/TenantTestController.cs
using Microsoft.AspNetCore.Mvc;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantTestController : ControllerBase
{
    private readonly ITenantManager _tenantManager;

    public TenantTestController(ITenantManager tenantManager)
    {
        _tenantManager = tenantManager;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var tenant = await _tenantManager.GetTenantAsync(HttpContext.Request.Headers["X-TenantId"]!);
        return Ok(new
        {
            message = $"Successfully connected to tenant: {tenant.Name}",
            tenant = new
            {
                tenant.TenantId,
                tenant.Name,
                tenant.CurrencyCode,
                tenant.IsActive,
                tenant.TimeZone
            }
        });
    }
}