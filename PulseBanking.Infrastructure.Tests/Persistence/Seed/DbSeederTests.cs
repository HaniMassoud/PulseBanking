// Update tests/PulseBanking.Infrastructure.Tests/Persistence/Seed/DbSeederTests.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Persistence;
using PulseBanking.Infrastructure.Persistence.Seed;
using FluentAssertions;
using Moq;

namespace PulseBanking.Infrastructure.Tests.Persistence.Seed;

public class DbSeederTests
{
    private readonly Mock<ITenantManager> _mockTenantManager;
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;
    private readonly IServiceProvider _serviceProvider;

    public DbSeederTests()
    {
        _mockTenantManager = new Mock<ITenantManager>();
        _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_mockTenantManager.Object);
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task SeedDataAsync_ShouldCreateInitialAccounts()
    {
        // Arrange
        var testTenant = new TenantSettings
        {
            TenantId = "test-tenant",
            Name = "Test Tenant",
            ConnectionString = "TestDb",
            IsActive = true
        };
        _mockTenantManager.Setup(x => x.GetTenantAsync("test-tenant"))
            .ReturnsAsync(testTenant);
        var factory = new TenantDbContextFactory(
            _mockTenantManager.Object,
            _optionsBuilder,
            _serviceProvider);
        using var context = factory.CreateDbContext("test-tenant");

        // Act
        await DbSeeder.SeedDataAsync(context, "test-tenant");

        // Assert
        var accounts = await context.Accounts.ToListAsync();
        accounts.Should().HaveCount(3);
        accounts.Should().Contain(a => a.Number == "1000-001" && a.Balance == 1000m);
        accounts.Should().Contain(a => a.Number == "1000-002" && a.Balance == 5000m);
        accounts.Should().Contain(a => a.Number == "1000-003" && a.Balance == 0m);
    }

    [Fact]
    public async Task SeedDataAsync_WhenCalledTwice_ShouldNotDuplicateData()
    {
        // Arrange
        var testTenant = new TenantSettings
        {
            TenantId = "test-tenant",
            Name = "Test Tenant",
            ConnectionString = "TestDb",
            IsActive = true
        };
        _mockTenantManager.Setup(x => x.GetTenantAsync("test-tenant"))
            .ReturnsAsync(testTenant);
        var factory = new TenantDbContextFactory(
            _mockTenantManager.Object,
            _optionsBuilder,
            _serviceProvider);
        using var context = factory.CreateDbContext("test-tenant");

        // Act
        await DbSeeder.SeedDataAsync(context, "test-tenant");
        await DbSeeder.SeedDataAsync(context, "test-tenant");

        // Assert
        var accounts = await context.Accounts.ToListAsync();
        accounts.Should().HaveCount(3);
    }
}