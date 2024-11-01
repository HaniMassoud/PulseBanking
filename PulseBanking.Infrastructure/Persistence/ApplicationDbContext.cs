// src/PulseBanking.Infrastructure/Persistence/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Common;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantService _tenantService;
    private readonly string? _currentTenantId;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
        try
        {
            _currentTenantId = _tenantService.GetCurrentTenant();
        }
        catch
        {
            _currentTenantId = null;
        }
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure audit fields defaults for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("CreatedAt")
                    .HasDefaultValueSql("GETUTCDATE()");
            }
        }

        // Multi-tenant query filters
        modelBuilder.Entity<Account>().HasQueryFilter(
            a => _currentTenantId == null || a.TenantId == _currentTenantId);
        modelBuilder.Entity<Customer>().HasQueryFilter(
            c => _currentTenantId == null || c.TenantId == _currentTenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                var tenantId = entry.Property("TenantId").CurrentValue?.ToString();
                if (string.IsNullOrEmpty(tenantId))
                {
                    if (_currentTenantId != null)
                    {
                        entry.Property("TenantId").CurrentValue = _currentTenantId;
                    }
                    else
                    {
                        throw new InvalidOperationException("TenantId is required for new entities");
                    }
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}