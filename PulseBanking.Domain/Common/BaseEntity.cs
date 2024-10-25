// Update src/PulseBanking.Domain/Common/BaseEntity.cs
namespace PulseBanking.Domain.Common;

public abstract class BaseEntity
{
    protected BaseEntity() { }

    public Guid Id { get; protected init; } = Guid.NewGuid();
    public string TenantId { get; protected init; } = string.Empty;  // Remove required, add default
}