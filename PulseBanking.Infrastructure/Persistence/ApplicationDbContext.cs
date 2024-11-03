using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Common;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>, IApplicationDbContext
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
    public DbSet<BankTransaction> Transactions => Set<BankTransaction>();
    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customize Identity tables
        modelBuilder.Entity<IdentityUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.Id).HasMaxLength(128);
            // Add TenantId column to Users table
            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();
            entity.HasIndex("TenantId");
        });

        modelBuilder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.Id).HasMaxLength(128);
            // Add TenantId column to Roles table
            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();
            entity.HasIndex("TenantId");
        });

        modelBuilder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.Property(e => e.UserId).HasMaxLength(128);
            entity.Property(e => e.RoleId).HasMaxLength(128);
        });

        modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("UserClaims");
            entity.Property(e => e.UserId).HasMaxLength(128);
            // Add TenantId column to UserClaims table
            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();
            entity.HasIndex("TenantId");
        });

        modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("UserLogins");
            entity.Property(e => e.UserId).HasMaxLength(128);
            // Add TenantId column to UserLogins table
            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();
            entity.HasIndex("TenantId");
        });

        modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("RoleClaims");
            entity.Property(e => e.RoleId).HasMaxLength(128);
            // Add TenantId column to RoleClaims table
            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();
            entity.HasIndex("TenantId");
        });

        modelBuilder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("UserTokens");
            entity.Property(e => e.UserId).HasMaxLength(128);
            // Add TenantId column to UserTokens table
            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();
            entity.HasIndex("TenantId");
        });

        // Apply all other configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure audit fields defaults
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
        modelBuilder.Entity<BankTransaction>().HasQueryFilter(
            t => _currentTenantId == null || t.TenantId == _currentTenantId);
        modelBuilder.Entity<AuditTrail>().HasQueryFilter(
            a => _currentTenantId == null || a.TenantId == _currentTenantId);

        // Add query filters for Identity entities
        modelBuilder.Entity<IdentityUser>().HasQueryFilter(
            u => _currentTenantId == null || EF.Property<string>(u, "TenantId") == _currentTenantId);
        modelBuilder.Entity<IdentityRole>().HasQueryFilter(
            r => _currentTenantId == null || EF.Property<string>(r, "TenantId") == _currentTenantId);
        modelBuilder.Entity<IdentityUserClaim<string>>().HasQueryFilter(
            uc => _currentTenantId == null || EF.Property<string>(uc, "TenantId") == _currentTenantId);
        modelBuilder.Entity<IdentityUserLogin<string>>().HasQueryFilter(
            ul => _currentTenantId == null || EF.Property<string>(ul, "TenantId") == _currentTenantId);
        modelBuilder.Entity<IdentityRoleClaim<string>>().HasQueryFilter(
            rc => _currentTenantId == null || EF.Property<string>(rc, "TenantId") == _currentTenantId);
        modelBuilder.Entity<IdentityUserToken<string>>().HasQueryFilter(
            ut => _currentTenantId == null || EF.Property<string>(ut, "TenantId") == _currentTenantId);
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

        // Set TenantId for new Identity entities
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is IdentityUser ||
                    entry.Entity is IdentityRole ||
                    entry.Entity is IdentityUserClaim<string> ||
                    entry.Entity is IdentityUserLogin<string> ||
                    entry.Entity is IdentityRoleClaim<string> ||
                    entry.Entity is IdentityUserToken<string>)
                {
                    entry.Property("TenantId").CurrentValue = _currentTenantId ??
                        throw new InvalidOperationException("TenantId is required for new Identity entities");
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}