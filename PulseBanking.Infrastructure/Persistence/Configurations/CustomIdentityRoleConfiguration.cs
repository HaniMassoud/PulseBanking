using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Persistence.Configurations;

public class CustomIdentityRoleConfiguration : IEntityTypeConfiguration<CustomIdentityRole>
{
    public void Configure(EntityTypeBuilder<CustomIdentityRole> builder)
    {
        builder.ToTable("Roles");

        builder.Property(r => r.Id)
            .HasMaxLength(128);

        builder.Property(r => r.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Name)
            .IsRequired()  // Make Name required
            .HasMaxLength(256);

        builder.Property(r => r.NormalizedName)
            .HasMaxLength(256);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        // Create compound unique index for TenantId + NormalizedName
        builder.HasIndex(r => new { r.TenantId, r.NormalizedName })
            .IsUnique()
            .HasFilter("[NormalizedName] IS NOT NULL")
            .HasDatabaseName("IX_Roles_TenantId_NormalizedName");

        // The query filter will be handled by the ApplicationDbContext
    }
}