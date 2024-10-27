// Update src/PulseBanking.Domain/Entities/Account.cs
using PulseBanking.Domain.Common;
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.Exceptions;

namespace PulseBanking.Domain.Entities;

public class Account : BaseEntity
{
    public string Number { get; init; } = string.Empty;  // Remove required, add default
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    public Guid CustomerId { get; private init; }  // Nullable for migration
    public Customer? Customer { get; private set; }

    protected Account() { }  // Required for EF Core

    private Account(
        string tenantId,
        string number,
        Guid customerId,
        decimal initialBalance,
        AccountStatus status)
    {
        TenantId = tenantId;
        Number = number;
        CustomerId = customerId;
        Balance = initialBalance;
        Status = status;
    }

    public static Account Create(
        string tenantId,
        string number,
        Guid customerId,
        decimal initialBalance = 0,
        AccountStatus status = AccountStatus.Active)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Number cannot be empty", nameof(number));
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));

        return new Account(tenantId, number, customerId, initialBalance, status);
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new InvalidOperationException("Amount must be positive");
        if (Status != AccountStatus.Active) throw new InvalidOperationException("Account not active");

        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new InvalidOperationException("Amount must be positive");
        if (Status != AccountStatus.Active) throw new InvalidOperationException("Account not active");
        if (Balance < amount) throw new InsufficientFundsException();

        Balance -= amount;
    }
}