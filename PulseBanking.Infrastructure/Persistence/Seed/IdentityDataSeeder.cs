// Add new file: src/PulseBanking.Infrastructure/Persistence/Seed/IdentityDataSeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Persistence.Seed;

public static class IdentityDataSeeder
{
    public static async Task SeedRolesAsync(ILogger logger, RoleManager<CustomIdentityRole> roleManager, string tenantId)
    {
        // Define default roles
        var roles = new[]
        {
            "SystemAdmin",      // Can manage all tenants and system settings
            "TenantAdmin",      // Can manage their tenant's settings and users
            "BankManager",      // Can manage accounts and view all transactions
            "CustomerService",  // Can view accounts and help customers
            "Customer",        // Regular bank customer
            "ReadOnly"         // Audit/compliance users
        };

        foreach (var roleName in roles)
        {
            try
            {
                var normalizedName = roleName.ToUpperInvariant();
                var roleExists = await roleManager.Roles
                    .AnyAsync(r => r.NormalizedName == normalizedName);

                if (!roleExists)
                {
                    var role = new CustomIdentityRole
                    {
                        Name = roleName,
                        NormalizedName = normalizedName,
                        TenantId = tenantId
                    };
                    // Set TenantId through reflection since it's a shadow property
                    //roleManager.GetType()
                    //    .GetProperty("TenantId")?
                    //    .SetValue(role, tenantId);

                    var result = await roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Created role {RoleName} for tenant {TenantId}", roleName, tenantId);
                    }
                    else
                    {
                        logger.LogError("Failed to create role {RoleName}: {Errors}",
                            roleName,
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding role {RoleName}", roleName);
            }
        }
    }

    public static async Task SeedSystemAdminAsync(
        ILogger logger,
        UserManager<CustomIdentityUser> userManager,
        RoleManager<CustomIdentityRole> roleManager,
        string systemTenantId)
    {
        try
        {
            // Create system admin if it doesn't exist
            var adminEmail = "sysadmin@pulsebanking.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new CustomIdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                // Set TenantId through reflection
                userManager.GetType()
                    .GetProperty("TenantId")?
                    .SetValue(adminUser, systemTenantId);

                var result = await userManager.CreateAsync(adminUser, "Admin123!@#");
                if (result.Succeeded)
                {
                    result = await userManager.AddToRoleAsync(adminUser, "SystemAdmin");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Created system admin user");
                    }
                    else
                    {
                        logger.LogError("Failed to add system admin to role: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogError("Failed to create system admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding system admin user");
        }
    }

    public static async Task SeedTenantAdminAsync(
        ILogger logger,
        UserManager<CustomIdentityUser> userManager,
        string tenantId,
        string email,
        string password)
    {
        try
        {
            var adminUser = await userManager.FindByEmailAsync(email);

            if (adminUser == null)
            {
                adminUser = new CustomIdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                // Set TenantId through reflection
                userManager.GetType()
                    .GetProperty("TenantId")?
                    .SetValue(adminUser, tenantId);

                var result = await userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                {
                    result = await userManager.AddToRoleAsync(adminUser, "TenantAdmin");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Created tenant admin user for {TenantId}", tenantId);
                    }
                    else
                    {
                        logger.LogError("Failed to add tenant admin to role: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogError("Failed to create tenant admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding tenant admin user for {TenantId}", tenantId);
        }
    }
}