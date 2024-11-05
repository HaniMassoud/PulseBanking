// Update src/PulseBanking.Application/Features/Tenants/Commands/CreateTenant/CreateTenantCommand.cs
using MediatR;
using PulseBanking.Application.Common.Interfaces;
using PulseBanking.Application.Features.Tenants.Common;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Application.Features.Tenants.Commands.CreateTenant;

public record CreateTenantCommand : IRequest<TenantDto>, ICreateTenantRequest
{
    public required string BankName { get; init; }
    public required string TimeZone { get; init; }
    public required string CurrencyCode { get; init; }
    public decimal DefaultTransactionLimit { get; init; }

    // Deployment Configuration
    public required DeploymentType DeploymentType { get; init; }
    public required RegionCode Region { get; init; }
    public required InstanceType InstanceType { get; init; }
    public required bool DataSovereigntyCompliant { get; init; }

    // Admin User Information
    public required string AdminFirstName { get; init; }
    public required string AdminLastName { get; init; }
    public required string AdminEmail { get; init; }
    public required string AdminPassword { get; init; }
    public required string AdminPhoneNumber { get; init; }
}