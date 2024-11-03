using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Persistence.Configurations;

public class TransactionHistoryConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("TransactionHistory");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        // Update running balance to be stored as a complex type
        builder.OwnsOne(t => t.RunningBalance, rb =>
        {
            rb.Property(m => m.Amount)
                .HasColumnName("RunningBalanceAmount")
                .HasPrecision(18, 2);

            rb.Property(m => m.Currency)
                .HasColumnName("RunningBalanceCurrency")
                .HasMaxLength(3);
        });

        // Add indexes for common queries
        builder.HasIndex(t => new { t.TenantId, t.AccountId, t.ProcessedDate });
        builder.HasIndex(t => new { t.TenantId, t.Type });
        builder.HasIndex(t => new { t.TenantId, t.ValueDate });
    }
}