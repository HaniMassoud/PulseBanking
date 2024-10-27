// Create new file: src/PulseBanking.Infrastructure/Persistence/Seed/DbSeeder.cs
using Microsoft.EntityFrameworkCore;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
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