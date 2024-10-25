// Update src/PulseBanking.Domain/Entities/Account.cs
using PulseBanking.Domain.Common;
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.Exceptions;

namespace PulseBanking.Domain.Entities;

public class Account : BaseEntity
{
    public string Number { get; private init; } = string.Empty;  // Remove required, add default
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }

    // Protected constructor for testing
    protected Account() { }

    private Account(string tenantId, string number, decimal initialBalance = 0, AccountStatus status = AccountStatus.Active)
    {
        TenantId = tenantId;
        Number = number;
        Balance = initialBalance;
        Status = status;
    }

    public static Account Create(string tenantId, string number, decimal initialBalance = 0, AccountStatus status = AccountStatus.Active)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Number cannot be empty", nameof(number));

        return new Account(tenantId, number, initialBalance, status);
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