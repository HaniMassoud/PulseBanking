// Update src/PulseBanking.Infrastructure/Persistence/Seed/DatabaseExtensions.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Persistence.Seed;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<CustomIdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<CustomIdentityRole>>();
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            var tenantManager = services.GetRequiredService<ITenantManager>();

            // Apply migrations
            await context.Database.MigrateAsync();

            // Seed default tenants and system admin
            await DbSeeder.SeedDefaultTenantsAsync(context, userManager, roleManager, logger);

            // Seed data for each tenant
            var tenants = await tenantManager.GetAllTenantsAsync();
            foreach (var tenant in tenants.Where(t => t.Id != "system"))
            {
                await DbSeeder.SeedDataAsync(context, tenant.Id, userManager, roleManager, logger);
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}