// Create new file: tests/PulseBanking.Application.Tests/Features/Accounts/Commands/CreateAccountCommandTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using PulseBanking.Application.Features.Accounts.Commands.CreateAccount;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Application.Tests.Features.Accounts.Commands;

public class CreateAccountCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tenantServiceMock = new Mock<ITenantService>();
        _handler = new CreateAccountCommandHandler(_contextMock.Object, _tenantServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAccount()
    {
        // Arrange
        const string tenantId = "test-tenant";
        var command = new CreateAccountCommand
        {
            Number = "ACC-001",
            InitialBalance = 100m
        };

        var accounts = new List<Account>();
        var dbSetMock = accounts.AsQueryable().BuildMockDbSet();

        _contextMock.Setup(x => x.Accounts)
            .Returns(dbSetMock.Object);

        _tenantServiceMock.Setup(x => x.GetCurrentTenant())
            .Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Number.Should().Be(command.Number);
        result.Balance.Should().Be(command.InitialBalance);
        result.Status.Should().Be(AccountStatus.Active);

        _contextMock.Verify(x => x.Accounts.Add(It.IsAny<Account>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}