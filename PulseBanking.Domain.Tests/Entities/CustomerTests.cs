// Create new file: tests/PulseBanking.Domain.Tests/Entities/CustomerTests.cs
using FluentAssertions;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Domain.Tests.Entities;

public class CustomerTests
{
    private const string TEST_TENANT_ID = "test-tenant-001";

    [Fact]
    public void Create_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "1234567890";

        // Act
        var customer = Customer.Create(
            TEST_TENANT_ID,
            firstName,
            lastName,
            email,
            phoneNumber);

        // Assert
        customer.Should().NotBeNull();
        customer.TenantId.Should().Be(TEST_TENANT_ID);
        customer.FirstName.Should().Be(firstName);
        customer.LastName.Should().Be(lastName);
        customer.Email.Should().Be(email);
        customer.PhoneNumber.Should().Be(phoneNumber);
        customer.Accounts.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidTenantId_ShouldThrowException(string invalidTenantId)
    {
        // Act
        var action = () => Customer.Create(
            invalidTenantId,
            "John",
            "Doe",
            "john.doe@example.com",
            "1234567890");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("TenantId cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidFirstName_ShouldThrowException(string invalidFirstName)
    {
        // Act
        var action = () => Customer.Create(
            TEST_TENANT_ID,
            invalidFirstName,
            "Doe",
            "john.doe@example.com",
            "1234567890");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("FirstName cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidLastName_ShouldThrowException(string invalidLastName)
    {
        // Act
        var action = () => Customer.Create(
            TEST_TENANT_ID,
            "John",
            invalidLastName,
            "john.doe@example.com",
            "1234567890");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("LastName cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidEmail_ShouldThrowException(string invalidEmail)
    {
        // Act
        var action = () => Customer.Create(
            TEST_TENANT_ID,
            "John",
            "Doe",
            invalidEmail,
            "1234567890");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be empty*");
    }

    [Fact]
    public void AddAccount_WithMatchingTenant_ShouldAddAccountToCustomer()
    {
        // Arrange
        var customer = Customer.Create(
            TEST_TENANT_ID,
            "John",
            "Doe",
            "john.doe@example.com",
            "1234567890");

        var account = Account.Create(
            TEST_TENANT_ID,
            "ACC-001",
            customer.Id);

        // Act
        customer.AddAccount(account);

        // Assert
        customer.Accounts.Should().HaveCount(1);
        customer.Accounts.First().Should().Be(account);
    }

    [Fact]
    public void AddAccount_WithDifferentTenant_ShouldThrowException()
    {
        // Arrange
        var customer = Customer.Create(
            TEST_TENANT_ID,
            "John",
            "Doe",
            "john.doe@example.com",
            "1234567890");

        var account = Account.Create(
            "different-tenant",
            "ACC-001",
            customer.Id);

        // Act
        var action = () => customer.AddAccount(account);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Account must belong to the same tenant");
    }
}