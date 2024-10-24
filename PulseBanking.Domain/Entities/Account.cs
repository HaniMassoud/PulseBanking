// Update src/PulseBanking.Domain/Entities/Account.cs
using PulseBanking.Domain.Common;
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.Exceptions;

namespace PulseBanking.Domain.Entities;

public class Account : BaseEntity
{
    public required string Number { get; init; }
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }

    protected Account() { }

    public static Account Create(string tenantId, string number, decimal initialBalance = 0, AccountStatus status = AccountStatus.Active)
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Number = number,
            Balance = initialBalance,
            Status = status
        };
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