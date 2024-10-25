// Update src/PulseBanking.Infrastructure/Persistence/TenantDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Services;

namespace PulseBanking.Infrastructure.Persistence;

// Update src/PulseBanking.Infrastructure/Persistence/TenantDbContextFactory.cs
public class TenantDbContextFactory : ITenantDbContextFactory<ApplicationDbContext>
{
    private readonly ITenantManager _tenantManager;
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;

    public TenantDbContextFactory(
        ITenantManager tenantManager,
        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder)
    {
        _tenantManager = tenantManager;
        _optionsBuilder = optionsBuilder;
    }

    public ApplicationDbContext CreateDbContext(string tenantId)
    {
        var tenantSettings = _tenantManager.GetTenantAsync(tenantId).GetAwaiter().GetResult();

        var tenantService = new TenantService(
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            _tenantManager);

        return new ApplicationDbContext(_optionsBuilder.Options, tenantService);
    }
}