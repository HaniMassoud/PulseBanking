﻿// src/PulseBanking.Domain/Entities/Tenant.cs
using PulseBanking.Domain.Enums;

namespace PulseBanking.Domain.Entities;

public class Tenant
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    // Deployment Information
    public required DeploymentType DeploymentType { get; init; }
    public required RegionCode Region { get; init; }
    public required InstanceType InstanceType { get; init; }

    // Connection/Database Information
    public required string ConnectionString { get; init; }
    public string? DedicatedHostName { get; init; }  // For dedicated deployments

    // Settings
    public string CurrencyCode { get; init; } = "USD";
    public decimal DefaultTransactionLimit { get; init; } = 10000m;
    public string TimeZone { get; init; } = "UTC";
    public bool IsActive { get; init; } = true;

    // Audit
    public required DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }

    // Trial/Demo specific
    public DateTime? TrialEndsAt { get; init; }

    // Compliance/Regulatory
    public required bool DataSovereigntyCompliant { get; init; }
    public string? RegulatoryNotes { get; init; }
}