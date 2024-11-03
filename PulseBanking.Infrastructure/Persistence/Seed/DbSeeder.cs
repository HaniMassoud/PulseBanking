// Update src/PulseBanking.Infrastructure/Persistence/Seed/DbSeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedDefaultTenantsAsync(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        // Only seed if no tenants exist
        if (!await context.Tenants.AnyAsync())
        {
            var now = DateTime.UtcNow;
            var systemTenant = new Tenant
            {
                Id = "system",
                Name = "System Tenant",
                DeploymentType = DeploymentType.Shared,
                Region = RegionCode.AUS,
                InstanceType = InstanceType.Production,
                ConnectionString = "Server=(local);Database=PulseBanking_System;Trusted_Connection=True;MultipleActiveResultSets=true;Trust Server Certificate=True;",
                CreatedAt = now,
                DataSovereigntyCompliant = true,
                CurrencyCode = "USD",
                DefaultTransactionLimit = 1000000m,
                TimeZone = "UTC",
                IsActive = true,
                CreatedBy = "System",
                RegulatoryNotes = "System tenant for platform administration"
            };

            var demoTenant = new Tenant
            {
                Id = "demo",
                Name = "Demo Bank",
                DeploymentType = DeploymentType.Shared,
                Region = RegionCode.AUS,
                InstanceType = InstanceType.Demo,
                ConnectionString = "Server=(local);Database=PulseBanking_Demo;Trusted_Connection=True;MultipleActiveResultSets=true;Trust Server Certificate=True;",
                CreatedAt = now,
                DataSovereigntyCompliant = true,
                CurrencyCode = "USD",
                DefaultTransactionLimit = 1000m,
                TimeZone = "UTC",
                IsActive = true,
                CreatedBy = "System",
                TrialEndsAt = now.AddMonths(1),
                RegulatoryNotes = "Demo tenant for evaluation purposes"
            };

            await context.Tenants.AddRangeAsync(systemTenant, demoTenant);
            await context.SaveChangesAsync();

            // Seed roles for system tenant
            await IdentityDataSeeder.SeedRolesAsync(logger, roleManager, systemTenant.Id);

            // Create system admin
            await IdentityDataSeeder.SeedSystemAdminAsync(logger, userManager, roleManager, systemTenant.Id);

            // Seed roles for demo tenant
            await IdentityDataSeeder.SeedRolesAsync(logger, roleManager, demoTenant.Id);

            // Create demo admin
            await IdentityDataSeeder.SeedTenantAdminAsync(
                logger,
                userManager,
                demoTenant.Id,
                "admin@demobank.com",
                "Demo123!@#");
        }
    }

    public static async Task SeedDataAsync(
        ApplicationDbContext context,
        string tenantId,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        // Seed roles for new tenant
        await IdentityDataSeeder.SeedRolesAsync(logger, roleManager, tenantId);

        // Only seed if no accounts exist for this tenant
        if (!await context.Accounts.AnyAsync(a => a.TenantId == tenantId))
        {
            // First seed a customer
            var customer = Customer.Create(
                tenantId: tenantId,
                firstName: "John",
                lastName: "Doe",
                email: "john.doe@demobank.com",
                phoneNumber: "1234567890"
            );
            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();

            // Create demo accounts
            var accounts = new List<Account>
            {
                Account.Create(
                    tenantId: tenantId,
                    number: "1000-001",
                    customerId: customer.Id,
                    initialBalance: 1000m,
                    status: AccountStatus.Active
                ),
                Account.Create(
                    tenantId: tenantId,
                    number: "1000-002",
                    customerId: customer.Id,
                    initialBalance: 5000m,
                    status: AccountStatus.Active
                ),
                Account.Create(
                    tenantId: tenantId,
                    number: "1000-003",
                    customerId: customer.Id,
                    initialBalance: 0m,
                    status: AccountStatus.Inactive
                )
            };

            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();

            // Create a user account for the customer
            var customerUser = new IdentityUser
            {
                UserName = customer.Email,
                Email = customer.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(customerUser, "Customer123!@#");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customerUser, "Customer");
            }
        }
    }
}