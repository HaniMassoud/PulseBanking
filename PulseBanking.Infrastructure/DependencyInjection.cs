// Update src/PulseBanking.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Persistence;
using PulseBanking.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace PulseBanking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register base DbContext options
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        // Register tenant-specific services
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ITenantManager, TenantManager>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantDbContextFactory<ApplicationDbContext>, TenantDbContextFactory>();

        // Register DbContext using factory
        services.AddScoped<IApplicationDbContext>(provider =>
        {
            var factory = provider.GetRequiredService<ITenantDbContextFactory<ApplicationDbContext>>();
            var tenantService = provider.GetRequiredService<ITenantService>();
            return factory.CreateDbContext(tenantService.GetCurrentTenant());
        });

        return services;
    }
}


//// Update src/PulseBanking.Infrastructure/DependencyInjection.cs
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using PulseBanking.Application.Interfaces;
//using PulseBanking.Infrastructure.Persistence;
//using PulseBanking.Infrastructure.Services;
//using Microsoft.AspNetCore.Http;

//namespace PulseBanking.Infrastructure;

//public static class DependencyInjection
//{
//    public static IServiceCollection AddInfrastructureServices(
//        this IServiceCollection services,
//        IConfiguration configuration)
//    {
//        services.AddDbContext<ApplicationDbContext>(options =>
//            options.UseSqlServer(
//                configuration.GetConnectionString("DefaultConnection"),
//                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

//        services.AddScoped<IApplicationDbContext>(provider =>
//            provider.GetRequiredService<ApplicationDbContext>());

//        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//        services.AddScoped<ITenantService, TenantService>();

//        services.AddSingleton<ITenantManager, TenantManager>();

//        return services;
//    }
//}