using FluentAssertions;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.Exceptions;

namespace PulseBanking.Domain.Tests.Entities;

public class AccountTests
{
    [Fact]
    public void Deposit_WhenAmountIsPositive_ShouldIncreaseBalance()
    {
        // Arrange
        var account = CreateActiveAccount();
        var initialBalance = account.Balance;
        var depositAmount = 100m;

        // Act
        account.Deposit(depositAmount);

        // Assert
        account.Balance.Should().Be(initialBalance + depositAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Deposit_WhenAmountIsNotPositive_ShouldThrowException(decimal invalidAmount)
    {
        // Arrange
        var account = CreateActiveAccount();

        // Act
        var action = () => account.Deposit(invalidAmount);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Amount must be positive");
    }

    [Fact]
    public void Deposit_WhenAccountNotActive_ShouldThrowException()
    {
        // Arrange
        var account = CreateInactiveAccount();

        // Act
        var action = () => account.Deposit(100);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Account not active");
    }

    [Fact]
    public void Withdraw_WhenAmountIsPositiveAndSufficient_ShouldDecreaseBalance()
    {
        // Arrange
        var account = CreateActiveAccount(initialBalance: 1000m);
        var withdrawAmount = 100m;
        var initialBalance = account.Balance;

        // Act
        account.Withdraw(withdrawAmount);

        // Assert
        account.Balance.Should().Be(initialBalance - withdrawAmount);
    }

    [Fact]
    public void Withdraw_WhenInsufficientFunds_ShouldThrowException()
    {
        // Arrange
        var account = CreateActiveAccount(initialBalance: 50m);

        // Act
        var action = () => account.Withdraw(100m);

        // Assert
        action.Should().Throw<InsufficientFundsException>();
    }

    private static Account CreateActiveAccount(decimal initialBalance = 0)
    {
        return Account.Create(
            Guid.NewGuid(),
            "TEST-001",
            initialBalance,
            AccountStatus.Active
        );
    }

    private static Account CreateInactiveAccount()
    {
        return Account.Create(
            Guid.NewGuid(),
            "TEST-002",
            0,
            AccountStatus.Inactive
        );
    }
}