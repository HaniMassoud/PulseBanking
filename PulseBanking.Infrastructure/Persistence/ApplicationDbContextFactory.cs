using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

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

        // Create a logger factory and logger for design-time
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ApplicationDbContext>();

        // Create a mock tenant service for design-time
        var tenantService = new DesignTimeTenantService();

        return new ApplicationDbContext(optionsBuilder.Options, tenantService, logger);
    }
}

// Add this class in the same file
public class DesignTimeTenantService : ITenantService
{
    public Tenant GetCurrentTenant()
    {
        return new Tenant
        {
            Id = "design-time-tenant",
            Name = "Design Time Tenant",
            DeploymentType = DeploymentType.Shared,
            Region = RegionCode.AUS,
            InstanceType = InstanceType.Production,
            ConnectionString = "design-time-connection",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            DataSovereigntyCompliant = true,
            TimeZone = "UTC",
            CurrencyCode = "USD",
            DefaultTransactionLimit = 10000m
        };
    }
}