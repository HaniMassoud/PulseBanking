using PulseBanking.Domain.Common;
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.ValueObjects;

namespace PulseBanking.Domain.Entities;

public class BankTransaction : BaseEntity
{
    private BankTransaction(
        string tenantId,
        Guid accountId,
        TransactionType type,
        Money amount,
        string reference,
        string description,
        Guid? counterpartyAccountId = null,
        DateTime? valueDate = null,
        string? externalReference = null,
        Dictionary<string, string>? metadata = null)
    {
        TenantId = tenantId;
        AccountId = accountId;
        Type = type;
        Amount = amount;
        Reference = reference;
        Description = description;
        CounterpartyAccountId = counterpartyAccountId;
        ValueDate = valueDate ?? DateTime.UtcNow;
        ProcessedDate = DateTime.UtcNow;
        ExternalReference = externalReference;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    protected BankTransaction()
    {
        AccountId = Guid.Empty;
        Type = TransactionType.Deposit;  // Default value
        Amount = Money.Zero("USD");      // Default value
        Reference = string.Empty;
        Description = string.Empty;
        ValueDate = DateTime.UtcNow;
        ProcessedDate = DateTime.UtcNow;
        Metadata = new Dictionary<string, string>();
    }

    public Guid AccountId { get; private init; }
    public Account? Account { get; private set; }

    public Guid? CounterpartyAccountId { get; private init; }
    public Account? CounterpartyAccount { get; private set; }

    public TransactionType Type { get; private init; }
    public Money Amount { get; private init; }
    public Money? RunningBalance { get; private set; }

    public string Reference { get; private init; }
    public string Description { get; private init; }

    public DateTime ValueDate { get; private init; }
    public DateTime ProcessedDate { get; private init; }

    public string? ExternalReference { get; private init; }
    public Dictionary<string, string> Metadata { get; private init; }

    public static BankTransaction Create(
        string tenantId,
        Guid accountId,
        TransactionType type,
        Money amount,
        string reference,
        string description,
        Guid? counterpartyAccountId = null,
        DateTime? valueDate = null,
        string? externalReference = null,
        Dictionary<string, string>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        if (accountId == Guid.Empty)
            throw new ArgumentException("AccountId cannot be empty", nameof(accountId));

        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Reference cannot be empty", nameof(reference));

        if (amount == null)
            throw new ArgumentNullException(nameof(amount), "Amount cannot be null");

        return new BankTransaction(
            tenantId,
            accountId,
            type,
            amount,
            reference,
            description,
            counterpartyAccountId,
            valueDate,
            externalReference,
            metadata);
    }

    public void UpdateRunningBalance(Money balance)
    {
        if (balance.Currency != Amount.Currency)
            throw new InvalidOperationException("Running balance currency must match transaction currency");

        RunningBalance = balance;
    }
}