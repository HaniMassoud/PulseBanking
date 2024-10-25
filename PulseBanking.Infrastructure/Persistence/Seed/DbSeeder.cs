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
            var accounts = new List<Account>
            {
                Account.Create(
                    tenantId: tenantId,
                    number: "1000-001",
                    initialBalance: 1000m,
                    status: AccountStatus.Active
                ),
                Account.Create(
                    tenantId: tenantId,
                    number: "1000-002",
                    initialBalance: 5000m,
                    status: AccountStatus.Active
                ),
                Account.Create(
                    tenantId: tenantId,
                    number: "1000-003",
                    initialBalance: 0m,
                    status: AccountStatus.Inactive
                )
            };

            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();
        }
    }
}