using PulseBanking.Domain.Enums;
using PulseBanking.Domain.Exceptions;

namespace PulseBanking.Domain.Entities;

public class Account
{
    public Guid Id { get; private init; }
    public required string Number { get; init; }  // removed 'private'
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }

    // Protected constructor for testing
    protected Account() { }

    // Factory method for creating accounts
    public static Account Create(Guid id, string number, decimal initialBalance = 0, AccountStatus status = AccountStatus.Active)
    {
        return new Account
        {
            Id = id,
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