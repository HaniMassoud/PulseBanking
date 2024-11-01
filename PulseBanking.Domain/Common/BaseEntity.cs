// Update src/PulseBanking.Domain/Common/BaseEntity.cs
namespace PulseBanking.Domain.Common;

public abstract class BaseEntity
{
    protected BaseEntity() { }

    public Guid Id { get; protected init; } = Guid.NewGuid();
    public string TenantId { get; protected init; } = string.Empty;

    // Audit fields
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public string? CreatedBy { get; protected set; }
    public DateTime? LastModifiedAt { get; protected set; }
    public string? LastModifiedBy { get; protected set; }
}