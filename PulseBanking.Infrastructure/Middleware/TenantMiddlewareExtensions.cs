// Create new file: src/PulseBanking.Infrastructure/Middleware/TenantMiddlewareExtensions.cs
using Microsoft.AspNetCore.Builder;

namespace PulseBanking.Infrastructure.Middleware;

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}