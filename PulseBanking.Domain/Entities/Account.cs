// In PulseBanking.Domain/Entities/Account.cs
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.Exceptions;

namespace PulseBanking.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }

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