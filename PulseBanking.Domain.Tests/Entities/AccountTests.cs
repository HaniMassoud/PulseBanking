// Update tests/PulseBanking.Domain.Tests/Entities/AccountTests.cs
using FluentAssertions;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.Exceptions;

namespace PulseBanking.Domain.Tests.Entities;

public class AccountTests
{
    private const string TEST_TENANT_ID = "test-tenant-001";
    private static readonly Guid TEST_CUSTOMER_ID = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateAccount()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var number = "ACC-001";
        var initialBalance = 100m;

        // Act
        var account = Account.Create(
            tenantId: TEST_TENANT_ID,
            number: number,
            customerId: customerId,
            initialBalance: initialBalance);

        // Assert
        account.Should().NotBeNull();
        account.TenantId.Should().Be(TEST_TENANT_ID);
        account.Number.Should().Be(number);
        account.CustomerId.Should().Be(customerId, "CustomerID should be set during creation");
        account.Balance.Should().Be(initialBalance);
        account.Status.Should().Be(AccountStatus.Active);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidTenantId_ShouldThrowException(string invalidTenantId)
    {
        // Act
        var action = () => Account.Create(invalidTenantId, "ACC-001", TEST_CUSTOMER_ID);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("TenantId cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidAccountNumber_ShouldThrowException(string invalidNumber)
    {
        // Act
        var action = () => Account.Create(TEST_TENANT_ID, invalidNumber, TEST_CUSTOMER_ID);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Number cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowException()
    {
        // Act
        var action = () => Account.Create(TEST_TENANT_ID, "ACC-001", Guid.Empty);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("CustomerId cannot be empty*");
    }

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

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Withdraw_WhenAmountIsNotPositive_ShouldThrowException(decimal invalidAmount)
    {
        // Arrange
        var account = CreateActiveAccount(initialBalance: 1000m);

        // Act
        var action = () => account.Withdraw(invalidAmount);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Amount must be positive");
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

    [Fact]
    public void Withdraw_WhenAccountNotActive_ShouldThrowException()
    {
        // Arrange
        var account = CreateInactiveAccount();

        // Act
        var action = () => account.Withdraw(100);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Account not active");
    }

    private static Account CreateActiveAccount(decimal initialBalance = 0)
    {
        return Account.Create(
            TEST_TENANT_ID,
            "TEST-001",
            TEST_CUSTOMER_ID,
            initialBalance,
            AccountStatus.Active
        );
    }

    private static Account CreateInactiveAccount()
    {
        return Account.Create(
            TEST_TENANT_ID,
            "TEST-002",
            TEST_CUSTOMER_ID,
            0,
            AccountStatus.Inactive
        );
    }
}