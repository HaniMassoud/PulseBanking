// Create new file: src/PulseBanking.Infrastructure/Persistence/Seed/DatabaseExtensions.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Persistence.Seed;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<IApplicationDbContext>() as ApplicationDbContext;
            var tenantManager = services.GetRequiredService<ITenantManager>();

            if (context == null)
                throw new InvalidOperationException("ApplicationDbContext not found");

            // Apply migrations
            await context.Database.MigrateAsync();

            // Get all tenants and seed data for each
            var tenants = await tenantManager.GetAllTenantsAsync();
            foreach (var tenant in tenants)
            {
                await DbSeeder.SeedDataAsync(context, tenant.TenantId);
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