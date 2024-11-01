// Update src/PulseBanking.Infrastructure/Persistence/TenantDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PulseBanking.Infrastructure.Persistence;

// Update src/PulseBanking.Infrastructure/Persistence/TenantDbContextFactory.cs
public class TenantDbContextFactory : ITenantDbContextFactory<ApplicationDbContext>
{
    private readonly ITenantManager _tenantManager;
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;
    private readonly IServiceProvider _serviceProvider;

    public TenantDbContextFactory(
        ITenantManager tenantManager,
        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder, IServiceProvider serviceProvider)
    {
        _tenantManager = tenantManager;
        _optionsBuilder = optionsBuilder;
        _serviceProvider = serviceProvider;
    }

    public ApplicationDbContext CreateDbContext(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            // Create a default TenantService instance for the default DbContext
            var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var defaultTenantService = new TenantService(httpContextAccessor, _tenantManager);
            return new ApplicationDbContext(_optionsBuilder.Options, defaultTenantService);
        }

        var tenantSettings = _tenantManager.GetTenantAsync(tenantId).GetAwaiter().GetResult();
        
        var tenantService = new TenantService(
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            _tenantManager);

        return new ApplicationDbContext(_optionsBuilder.Options, tenantService);
    }
}