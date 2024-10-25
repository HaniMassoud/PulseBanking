// Update src/PulseBanking.Infrastructure/Persistence/TenantDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Services;

namespace PulseBanking.Infrastructure.Persistence;

public class TenantDbContextFactory : ITenantDbContextFactory<ApplicationDbContext>
{
    private readonly ITenantManager _tenantManager;
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;
    private readonly string _defaultProvider;

    public TenantDbContextFactory(
        ITenantManager tenantManager,
        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder,
        string defaultProvider = "SqlServer")
    {
        _tenantManager = tenantManager;
        _optionsBuilder = optionsBuilder;
        _defaultProvider = defaultProvider;
    }

    public ApplicationDbContext CreateDbContext(string tenantId)
    {
        var tenantSettings = _tenantManager.GetTenantAsync(tenantId).GetAwaiter().GetResult();

        if (_defaultProvider == "SqlServer")
        {
            _optionsBuilder.UseSqlServer(tenantSettings.ConnectionString);
        }

        var tenantService = new TenantService(
            new HttpContextAccessor(),
            _tenantManager,
            fixedTenantId: tenantId);  // Pass the tenant ID directly

        return new ApplicationDbContext(_optionsBuilder.Options, tenantService);
    }
}