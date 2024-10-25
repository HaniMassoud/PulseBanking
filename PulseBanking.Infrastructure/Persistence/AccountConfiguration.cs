// Create new file: src/PulseBanking.Infrastructure/Persistence/Configurations/AccountConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Balance)
            .HasPrecision(18, 2);

        // Create an index on TenantId to improve query performance
        builder.HasIndex(a => a.TenantId);

        // Create a unique index on TenantId and Number combination
        builder.HasIndex(a => new { a.TenantId, a.Number }).IsUnique();
    }
}