// Create new file: src/PulseBanking.Infrastructure/Persistence/ApplicationDbContextFactory.cs
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Services;

namespace PulseBanking.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        // Create a mock tenant service for design-time
        var tenantManager = new TenantManager(configuration);
        var tenantService = new TenantService(new HttpContextAccessor(), tenantManager);

        return new ApplicationDbContext(optionsBuilder.Options, tenantService);
    }
}