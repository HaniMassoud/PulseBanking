// Update src/PulseBanking.Infrastructure/Middleware/TenantMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace PulseBanking.Infrastructure.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private const string TENANT_HEADER = "X-TenantId";

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
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

        await _next(context);
    }
}