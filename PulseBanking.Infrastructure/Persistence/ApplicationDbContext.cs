using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Common;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Persistence;

//public class ApplicationDbContext : IdentityDbContext<CustomIdentityUser>, IApplicationDbContext
public class ApplicationDbContext : IdentityDbContext
    <CustomIdentityUser,
    CustomIdentityRole,
    String,
    IdentityUserClaim<string>,
    IdentityUserRole<string>,
    IdentityUserLogin<string>,
    IdentityRoleClaim<string>,
    IdentityUserToken<string>>,
    IApplicationDbContext
{
    private readonly ITenantService _tenantService;
    private readonly string? _currentTenantId;
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService tenantService, ILogger<ApplicationDbContext> logger)
        : base(options)
    {
        _tenantService = tenantService;
        try
        {
            var tenant = _tenantService.GetCurrentTenant();
            _currentTenantId = tenant?.Id;
        }
        catch
        {
            _currentTenantId = null;
        }
        _logger = logger;
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<BankTransaction> Transactions => Set<BankTransaction>();
    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //base.OnModelCreating(modelBuilder);

        // Configure CustomIdentityRole
        modelBuilder.Entity<CustomIdentityRole>(entity =>
        {
            entity.ToTable("Roles");

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
            entity.Property(e => e.ConcurrencyStamp).IsConcurrencyToken();
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasKey(r => r.Id);

            entity.HasIndex(r => r.NormalizedName).HasDatabaseName("RoleNameIndex")
                .IsUnique()
                .HasFilter("[NormalizedName] IS NOT NULL");

            entity.HasIndex(r => r.TenantId);

            entity.HasIndex(r => new { r.TenantId, r.NormalizedName })
                .HasDatabaseName("IX_Roles_TenantId_NormalizedName")
                .IsUnique()
                .HasFilter("[NormalizedName] IS NOT NULL");
        });

        // Configure CustomIdentityUser
        modelBuilder.Entity<CustomIdentityUser>(entity =>
        {
            entity.ToTable("Users");

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.NormalizedUserName)
                .HasDatabaseName("UserNameIndex")
                .IsUnique()
                .HasFilter("[NormalizedUserName] IS NOT NULL");
            entity.HasIndex(e => e.NormalizedEmail)
                .HasDatabaseName("EmailIndex");
        });

        modelBuilder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("UserRoles");

            entity.HasKey(r => new { r.UserId, r.RoleId });

            entity.Property(e => e.UserId).HasMaxLength(128);
            entity.Property(e => e.RoleId).HasMaxLength(128);

            entity.HasOne<CustomIdentityUser>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            entity.HasOne<CustomIdentityRole>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });

        modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("RoleClaims");

            entity.HasKey(rc => rc.Id);

            entity.Property(rc => rc.RoleId).HasMaxLength(128);
            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();

            entity.HasIndex("TenantId");

            entity.HasOne<CustomIdentityRole>()
                .WithMany()
                .HasForeignKey(rc => rc.RoleId)
                .IsRequired();
        });

        modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("UserClaims");

            entity.HasKey(uc => uc.Id);

            entity.Property(uc => uc.UserId).HasMaxLength(128);
            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();

            entity.HasIndex("TenantId");

            entity.HasOne<CustomIdentityUser>()
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("UserLogins");

            entity.HasKey(l => new { l.LoginProvider, l.ProviderKey });

            entity.Property(l => l.LoginProvider).HasMaxLength(128);
            entity.Property(l => l.ProviderKey).HasMaxLength(128);
            entity.Property(l => l.UserId).HasMaxLength(128);
            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();

            entity.HasIndex("TenantId");

            entity.HasOne<CustomIdentityUser>()
                .WithMany()
                .HasForeignKey(ul => ul.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("UserTokens");

            // Remove size limitation for Name column since it's part of the primary key
            entity.Property(t => t.LoginProvider).HasMaxLength(128);
            entity.Property(t => t.UserId).HasMaxLength(128);
            // Don't set MaxLength for Name since it's part of PK

            entity.Property<string>("TenantId").HasMaxLength(50).IsRequired();

            entity.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            entity.HasIndex("TenantId");

            entity.HasOne<CustomIdentityUser>()
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .IsRequired();
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
        modelBuilder.Entity<CustomIdentityUser>().HasQueryFilter(
            u => _currentTenantId == null || EF.Property<string>(u, "TenantId") == _currentTenantId);
        //modelBuilder.Entity<CustomIdentityRole>().HasQueryFilter(
        //    r => _currentTenantId == null || EF.Property<string>(r, "TenantId") == _currentTenantId);
        //modelBuilder.Entity<CustomIdentityRole>().HasQueryFilter(
        //    r => _currentTenantId == null || r.TenantId == _currentTenantId);
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
                _logger.LogInformation("Processing entry while _currentTenantId is: {_currentTenantId}", _currentTenantId);

                if (entry.Entity is CustomIdentityUser ||
                    entry.Entity is CustomIdentityRole ||
                    entry.Entity is IdentityUserClaim<string> ||
                    entry.Entity is IdentityUserLogin<string> ||
                    entry.Entity is IdentityRoleClaim<string> ||
                    entry.Entity is IdentityUserToken<string>)
                {
                    var currentTenantId = entry.Property("TenantId").CurrentValue?.ToString();
                    if (string.IsNullOrEmpty(currentTenantId))
                    {
                        throw new InvalidOperationException("TenantId is required for new Identity entities");
                    }

                    _logger.LogInformation("Processing entry with TenantId {tenantId}", currentTenantId);
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}