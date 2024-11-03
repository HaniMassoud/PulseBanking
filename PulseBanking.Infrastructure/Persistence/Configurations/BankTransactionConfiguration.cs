using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseBanking.Domain.Entities;
using System.Text.Json;

namespace PulseBanking.Infrastructure.Persistence.Configurations;

public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("TransactionHistory");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        // Amount stored as Value Object
        builder.OwnsOne(t => t.Amount, amount =>
        {
            amount.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            amount.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Running Balance stored as Value Object
        builder.OwnsOne(t => t.RunningBalance, balance =>
        {
            balance.Property(m => m.Amount)
                .HasColumnName("RunningBalanceAmount")
                .HasPrecision(18, 2);

            balance.Property(m => m.Currency)
                .HasColumnName("RunningBalanceCurrency")
                .HasMaxLength(3);
        });

        builder.Property(t => t.Reference)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.ExternalReference)
            .HasMaxLength(100);

        builder.Property(t => t.ProcessedDate)
            .IsRequired();

        builder.Property(t => t.ValueDate)
            .IsRequired();

        // Configure Metadata dictionary with value converter and comparer
        builder.Property(t => t.Metadata)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>(),
                new ValueComparer<Dictionary<string, string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => new Dictionary<string, string>(c)
                )
            );

        // Relationships
        builder.HasOne(t => t.Account)
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CounterpartyAccount)
            .WithMany()
            .HasForeignKey(t => t.CounterpartyAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.ProcessedDate);
        builder.HasIndex(t => new { t.TenantId, t.Reference });
        builder.HasIndex(t => new { t.TenantId, t.ExternalReference })
            .HasFilter("[ExternalReference] IS NOT NULL");
        builder.HasIndex(t => new { t.TenantId, t.AccountId, t.ProcessedDate });
        builder.HasIndex(t => new { t.TenantId, t.Type });
        builder.HasIndex(t => new { t.TenantId, t.ValueDate });
    }
}