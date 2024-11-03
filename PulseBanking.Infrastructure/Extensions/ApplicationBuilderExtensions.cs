using Microsoft.AspNetCore.Builder;
using PulseBanking.Infrastructure.Security;

namespace PulseBanking.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantAwareAntiforgeryMiddleware>();
        return app;
    }
}