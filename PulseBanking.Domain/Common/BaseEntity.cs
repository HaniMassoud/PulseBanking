// Update src/PulseBanking.Domain/Common/BaseEntity.cs
namespace PulseBanking.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected init; }  // Changed from private to protected
    public required string TenantId { get; init; }
}