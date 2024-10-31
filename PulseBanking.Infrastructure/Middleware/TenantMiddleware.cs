using Microsoft.AspNetCore.Http;
using System.Text.Json;
using PulseBanking.Application.Interfaces;
using Microsoft.Extensions.Logging;
using PulseBanking.Infrastructure.Attributes;

namespace PulseBanking.Infrastructure.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantManager _tenantManager;
    private readonly ILogger<TenantMiddleware> _logger;
    private const string TENANT_HEADER = "X-TenantId";

    public TenantMiddleware(
        RequestDelegate next,
        ITenantManager tenantManager,
        ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _tenantManager = tenantManager;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Request.Path;  // Use path instead of endpoint for now

        // Skip tenant validation for registration endpoint
        if (endpoint.Value?.EndsWith("/register", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogDebug("Skipping tenant validation for registration endpoint");
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(TENANT_HEADER, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var response = JsonSerializer.Serialize(new { error = $"Missing {TENANT_HEADER} header" });
            await context.Response.WriteAsync(response);
            return;
        }

        // Rest of your existing code...
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