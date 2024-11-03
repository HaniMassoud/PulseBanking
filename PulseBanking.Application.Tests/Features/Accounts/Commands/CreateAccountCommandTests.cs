// Update tests/PulseBanking.Application.Tests/Features/Accounts/Commands/CreateAccountCommandTests.cs
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using PulseBanking.Application.Common.Exceptions;
using PulseBanking.Application.Features.Accounts.Commands.CreateAccount;
using PulseBanking.Application.Features.Accounts.Common;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Application.Tests.Features.Accounts.Commands;

public class CreateAccountCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly CreateAccountCommandHandler _handler;
    private readonly Mock<DbSet<Account>> _accountsDbSetMock;
    private readonly Mock<DbSet<Customer>> _customersDbSetMock;

    public CreateAccountCommandTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantServiceMock = new Mock<ITenantService>();
        _accountsDbSetMock = new Mock<DbSet<Account>>();
        _customersDbSetMock = new Mock<DbSet<Customer>>();
        _handler = new CreateAccountCommandHandler(_contextMock.Object, _tenantServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAccount()
    {
        // Arrange
        var testTenant = CreateTestTenant();
        var customer = Customer.Create(
            testTenant.Id,
            "John",
            "Doe",
            "john.doe@example.com",
            "1234567890");

        // Use the customer's actual Id
        var command = new CreateAccountCommand
        {
            Number = "ACC-001",
            CustomerId = customer.Id,  // Use the actual customer Id
            InitialBalance = 100m
        };

        // Setup customer query mock
        var customers = new List<Customer> { customer };
        var customersMock = customers.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Customers).Returns(customersMock.Object);

        _contextMock.Setup(x => x.Accounts).Returns(_accountsDbSetMock.Object);
        _tenantServiceMock.Setup(x => x.GetCurrentTenant()).Returns(testTenant);

        Account? savedAccount = null;
        _accountsDbSetMock.Setup(d => d.Add(It.IsAny<Account>()))
            .Callback<Account>(account => savedAccount = account);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Number.Should().Be(command.Number);
        result.CustomerId.Should().Be(customer.Id);  // Verify against customer.Id
        result.Balance.Should().Be(command.InitialBalance);
        result.Status.Should().Be(AccountStatus.Active);

        savedAccount.Should().NotBeNull();
        savedAccount!.Number.Should().Be(command.Number);
        savedAccount.CustomerId.Should().Be(customer.Id);  // Verify against customer.Id
        savedAccount.Balance.Should().Be(command.InitialBalance);
        savedAccount.TenantId.Should().Be(testTenant.Id);

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldThrowNotFoundException()
    {
        // Arrange
        var testTenant = CreateTestTenant();
        var customerId = Guid.NewGuid();

        var command = new CreateAccountCommand
        {
            Number = "ACC-001",
            CustomerId = customerId,
            InitialBalance = 100m
        };

        var customers = new List<Customer>();
        var customersMock = customers.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Customers).Returns(customersMock.Object);

        _tenantServiceMock.Setup(x => x.GetCurrentTenant()).Returns(testTenant);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    private Tenant CreateTestTenant(string id = "test-tenant")
    {
        return new Tenant
        {
            Id = id,
            Name = "Test Tenant",
            DeploymentType = DeploymentType.Shared,
            Region = RegionCode.AUS,
            InstanceType = InstanceType.Production,
            ConnectionString = "test-connection",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            DataSovereigntyCompliant = true,
            TimeZone = "UTC",
            CurrencyCode = "USD",
            DefaultTransactionLimit = 10000m
        };
    }

}