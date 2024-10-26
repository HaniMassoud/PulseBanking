// Update src/PulseBanking.Infrastructure/Middleware/TenantMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantManager _tenantManager;
    private const string TENANT_HEADER = "X-TenantId";

    public TenantMiddleware(RequestDelegate next, ITenantManager tenantManager)
    {
        _next = next;
        _tenantManager = tenantManager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(TENANT_HEADER, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var response = JsonSerializer.Serialize(new { error = $"Missing {TENANT_HEADER} header" });
            await context.Response.WriteAsync(response);
            return;
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var response = JsonSerializer.Serialize(new { error = $"{TENANT_HEADER} header cannot be empty" });
            await context.Response.WriteAsync(response);
            return;
        }

        try
        {
            // Get tenant settings to validate the tenant exists and is active
            var tenant = await _tenantManager.GetTenantAsync(tenantId!);
            if (!tenant.IsActive)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                var response = JsonSerializer.Serialize(new { error = "Tenant is not active" });
                await context.Response.WriteAsync(response);
                return;
            }
        }
        catch (KeyNotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var response = JsonSerializer.Serialize(new { error = "Invalid tenant" });
            await context.Response.WriteAsync(response);
            return;
        }

        await _next(context);
    }
}