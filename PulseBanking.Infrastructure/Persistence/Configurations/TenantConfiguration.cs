// src/PulseBanking.Infrastructure/Persistence/Configurations/TenantConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        // Basic Information
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Deployment Information
        builder.Property(t => t.DeploymentType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Region)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.InstanceType)
            .IsRequired()
            .HasConversion<string>();

        // Connection Information
        builder.Property(t => t.ConnectionString)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.DedicatedHostName)
            .HasMaxLength(255);

        // Settings with defaults
        builder.Property(t => t.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(t => t.DefaultTransactionLimit)
            .HasPrecision(18, 2)
            .HasDefaultValue(10000m);

        builder.Property(t => t.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("UTC");

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit Fields
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.LastModifiedAt);

        builder.Property(t => t.LastModifiedBy)
            .HasMaxLength(100);

        // Trial/Demo
        builder.Property(t => t.TrialEndsAt);

        // Compliance
        builder.Property(t => t.DataSovereigntyCompliant)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.RegulatoryNotes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(t => new { t.Id, t.IsActive })
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName("IX_Tenants_Id_IsActive");

        builder.HasIndex(t => new { t.Region, t.DeploymentType, t.InstanceType })
            .HasDatabaseName("IX_Tenants_Region_Deployment_Instance");

        builder.HasIndex(t => new { t.InstanceType, t.TrialEndsAt })
            .HasFilter("[InstanceType] IN ('Demo', 'Trial')")
            .HasDatabaseName("IX_Tenants_Trial_Management");

        builder.HasIndex(t => t.Name)
            .HasDatabaseName("IX_Tenants_Name");

        builder.HasIndex(t => t.DedicatedHostName)
            .HasFilter("[DeploymentType] = 'Dedicated'")
            .HasDatabaseName("IX_Tenants_DedicatedHost");
    }
}