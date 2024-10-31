// Create new file: src/PulseBanking.Application/Features/Tenants/Common/TenantDto.cs
namespace PulseBanking.Application.Features.Tenants.Common;

public class TenantDto
{
    public required string TenantId { get; init; }
    public required string Name { get; init; }
    public bool IsActive { get; init; }
}