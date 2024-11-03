using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration from the API project
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../PulseBanking.Api"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        // Create a mock tenant service for design-time
        var tenantService = new DesignTimeTenantService();

        return new ApplicationDbContext(optionsBuilder.Options, tenantService);
    }
}

// Add this class in the same file
public class DesignTimeTenantService : ITenantService
{
    public string GetCurrentTenant()
    {
        return "design-time-tenant";
    }
}