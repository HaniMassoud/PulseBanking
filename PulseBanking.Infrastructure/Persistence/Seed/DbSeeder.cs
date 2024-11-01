// Update src/PulseBanking.Infrastructure/Persistence/Seed/DbSeeder.cs
using Microsoft.EntityFrameworkCore;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    // Update src/PulseBanking.Infrastructure/Persistence/Seed/DbSeeder.cs
    public static async Task SeedDefaultTenantsAsync(ApplicationDbContext context)
    {
        // Only seed if no tenants exist
        if (!await context.Tenants.AnyAsync())
        {
            var now = DateTime.UtcNow;
            var tenants = new List<Tenant>
        {
            new Tenant
            {
                // Required properties
                Id = "aus-shared-prod",
                Name = "Australia Shared Production",
                DeploymentType = DeploymentType.Shared,
                Region = RegionCode.AUS,
                InstanceType = InstanceType.Production,
                ConnectionString = "Server=(local);Database=PulseBanking_AUS_Shared;Trusted_Connection=True;MultipleActiveResultSets=true;Trust Server Certificate=True;",
                CreatedAt = now,
                DataSovereigntyCompliant = true,

                // Optional properties (with default values)
                DedicatedHostName = null,
                CurrencyCode = "AUD",  // Override default USD
                DefaultTransactionLimit = 10000m,
                TimeZone = "Australia/Sydney",  // Override default UTC
                IsActive = true,
                CreatedBy = "System",
                LastModifiedAt = null,
                LastModifiedBy = null,
                TrialEndsAt = null,
                RegulatoryNotes = "Compliant with Australian banking regulations"
            },
            new Tenant
            {
                // Required properties
                Id = "demo-instance",
                Name = "Pulse Banking Demo",
                DeploymentType = DeploymentType.Shared,
                Region = RegionCode.AUS,
                InstanceType = InstanceType.Demo,
                ConnectionString = "Server=(local);Database=PulseBanking_Demo;Trusted_Connection=True;MultipleActiveResultSets=true;Trust Server Certificate=True;",
                CreatedAt = now,
                DataSovereigntyCompliant = true,

                // Optional properties (with default values)
                DedicatedHostName = null,
                CurrencyCode = "USD",  // Using default
                DefaultTransactionLimit = 1000m,  // Lower limit for demo
                TimeZone = "UTC",  // Using default
                IsActive = true,
                CreatedBy = "System",
                LastModifiedAt = null,
                LastModifiedBy = null,
                TrialEndsAt = now.AddYears(1),
                RegulatoryNotes = "Demo instance for evaluation purposes"
            }
        };

            await context.Tenants.AddRangeAsync(tenants);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedDataAsync(ApplicationDbContext context, string tenantId)
    {
        // Only seed if no accounts exist for this tenant
        if (!await context.Accounts.AnyAsync(a => a.TenantId == tenantId))
        {
            // First seed a customer
            var customer = Customer.Create(
                tenantId: tenantId,
                firstName: "John",
                lastName: "Doe",
                email: "john.doe@tenant1.com",
                phoneNumber: "1234567890"
            );
            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();

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
        }
    }
}