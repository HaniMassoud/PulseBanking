// Create new file: tests/PulseBanking.Api.Tests/Controllers/AccountsControllerTests.cs
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PulseBanking.Api.Controllers;
using PulseBanking.Application.Features.Accounts.Commands.CreateAccount;
using PulseBanking.Application.Features.Accounts.Common;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Api.Tests.Controllers;

public class AccountsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _tenantServiceMock = new Mock<ITenantService>();
        _controller = new AccountsController(_mediatorMock.Object, _tenantServiceMock.Object);
    }

    [Fact]
    public async Task CreateAccount_ValidCommand_ReturnsCreatedResponse()
    {
        // Arrange
        var testTenant = CreateTestTenant();
        var command = new CreateAccountCommand
        {
            Number = "ACC-001",
            CustomerId = Guid.NewGuid(),  // Add this
            InitialBalance = 100m
        };

        var accountDto = new AccountDto
        {
            Id = Guid.NewGuid(),
            Number = command.Number,
            Balance = command.InitialBalance,
            Status = AccountStatus.Active
        };

        _tenantServiceMock.Setup(x => x.GetCurrentTenant())
            .Returns(testTenant);

        _mediatorMock.Setup(x => x.Send(command, default))
            .ReturnsAsync(accountDto);

        // Act
        var result = await _controller.CreateAccount(command);

        // Assert
        var createdAtResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnValue = createdAtResult.Value.Should().BeOfType<AccountDto>().Subject;

        returnValue.Should().BeEquivalentTo(accountDto);
        createdAtResult.ActionName.Should().Be(nameof(AccountsController.GetAccount));
        createdAtResult.RouteValues!["id"].Should().Be(accountDto.Id);
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