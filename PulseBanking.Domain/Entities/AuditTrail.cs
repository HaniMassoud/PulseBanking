using PulseBanking.Domain.Common;

namespace PulseBanking.Domain.Entities;

public class AuditTrail : BaseEntity
{
    private AuditTrail(
        string tenantId,
        string action,
        string entityName,
        string entityId,
        string? details,
        DateTime timestamp,
        string? userId,
        string? userName)
    {
        TenantId = tenantId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        Details = details;
        Timestamp = timestamp;
        UserId = userId;
        UserName = userName;
    }

    protected AuditTrail() { } // Required for EF Core

    public string Action { get; private init; } = string.Empty;
    public string EntityName { get; private init; } = string.Empty;
    public string EntityId { get; private init; } = string.Empty;
    public string? Details { get; private init; }
    public DateTime Timestamp { get; private init; }
    public string? UserId { get; private init; }
    public string? UserName { get; private init; }

    public static AuditTrail Create(
        string tenantId,
        string action,
        string entityName,
        string entityId,
        string? details = null,
        DateTime? timestamp = null,
        string? userId = null,
        string? userName = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));

        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("EntityName cannot be empty", nameof(entityName));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("EntityId cannot be empty", nameof(entityId));

        return new AuditTrail(
            tenantId,
            action,
            entityName,
            entityId,
            details,
            timestamp ?? DateTime.UtcNow,
            userId,
            userName);
    }
}