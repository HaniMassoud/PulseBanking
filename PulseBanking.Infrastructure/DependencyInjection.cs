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
        // Create and configure the options builder
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        // Register base DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        // Register services
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ITenantManager, TenantManager>();
        services.AddScoped<ITenantService, TenantService>();

        // Register the options builder
        services.AddSingleton(optionsBuilder);

        // Register the factory
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