// Update src/PulseBanking.Infrastructure/Persistence/Seed/DatabaseExtensions.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Persistence.Seed;

public static class DatabaseExtensions
{
    // Update src/PulseBanking.Infrastructure/Persistence/Seed/DatabaseExtensions.cs
    public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            // Get the DbContext directly using DbContextFactory
            var options = services.GetRequiredService<DbContextOptionsBuilder<ApplicationDbContext>>().Options;
            var tenantManager = services.GetRequiredService<ITenantManager>();

            // Create a special tenant service for seeding that doesn't require tenant header
            var tenantService = new Services.TenantService(
                new Microsoft.AspNetCore.Http.HttpContextAccessor(),
                tenantManager,
                fixedTenantId: "system");  // Use a system tenant for migrations

            // Create context with system tenant
            using var context = new ApplicationDbContext(options, tenantService);

            // Apply migrations
            await context.Database.MigrateAsync();

            // Seed default tenants
            await DbSeeder.SeedDefaultTenantsAsync(context);

            // Get all tenants and seed data for each
            var tenants = await tenantManager.GetAllTenantsAsync();
            foreach (var tenant in tenants)
            {
                // Create a new context for each tenant
                var tenantSpecificService = new Services.TenantService(
                    new Microsoft.AspNetCore.Http.HttpContextAccessor(),
                    tenantManager,
                    fixedTenantId: tenant.TenantId);
                using var tenantContext = new ApplicationDbContext(options, tenantSpecificService);
                await DbSeeder.SeedDataAsync(tenantContext, tenant.TenantId);
            }
        }
        catch (Exception ex)
        {
            // In a real application, you would want to use proper logging here
            Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
            throw;
        }
    }
}