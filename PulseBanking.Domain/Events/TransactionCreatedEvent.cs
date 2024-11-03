using PulseBanking.Domain.Enums;

namespace PulseBanking.Domain.Events;

public class TransactionCreatedEvent
{
    public Guid TransactionId { get; }
    public string TenantId { get; }
    public Guid AccountId { get; }
    public TransactionType Type { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public DateTime Timestamp { get; }

    public TransactionCreatedEvent(
        Guid transactionId,
        string tenantId,
        Guid accountId,
        TransactionType type,
        decimal amount,
        string currency)
    {
        TransactionId = transactionId;
        TenantId = tenantId;
        AccountId = accountId;
        Type = type;
        Amount = amount;
        Currency = currency;
        Timestamp = DateTime.UtcNow;
    }
}