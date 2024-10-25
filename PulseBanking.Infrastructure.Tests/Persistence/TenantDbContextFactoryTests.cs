// tests/PulseBanking.Infrastructure.Tests/Persistence/TenantDbContextFactoryTests.cs
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Persistence;
using FluentAssertions;
using Moq;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Infrastructure.Tests.Persistence;

public class TenantDbContextFactoryTests
{
    private readonly Mock<ITenantManager> _mockTenantManager;
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;

    public TenantDbContextFactoryTests()
    {
        _mockTenantManager = new Mock<ITenantManager>();
        _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb");
    }

    [Fact]
    public void CreateDbContext_WithValidTenant_ReturnsConfiguredDbContext()
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
            defaultProvider: "InMemory");

        // Act
        var dbContext = factory.CreateDbContext("test-tenant");

        // Assert
        dbContext.Should().NotBeNull();
        dbContext.Should().BeOfType<ApplicationDbContext>();
    }

    [Fact]
    public void CreateDbContext_WithInvalidTenant_ThrowsException()
    {
        // Arrange
        _mockTenantManager.Setup(x => x.GetTenantAsync("invalid-tenant"))
            .ThrowsAsync(new KeyNotFoundException("Tenant not found"));

        var factory = new TenantDbContextFactory(
            _mockTenantManager.Object,
            _optionsBuilder,
            defaultProvider: "InMemory");

        // Act
        var act = () => factory.CreateDbContext("invalid-tenant");

        // Assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("Tenant not found");
    }

    [Fact]
    public async Task CreateDbContext_EnforcesQueryFilters()
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

        _mockTenantManager.Setup(x => x.GetTenantAsync("other-tenant"))
            .ReturnsAsync(new TenantSettings
            {
                TenantId = "other-tenant",
                Name = "Other Tenant",
                ConnectionString = "TestDb",
                IsActive = true
            });

        var factory = new TenantDbContextFactory(
            _mockTenantManager.Object,
            _optionsBuilder,
            defaultProvider: "InMemory");

        // Create test data
        var account1 = Account.Create(
            tenantId: "test-tenant",
            number: "ACC-001"
        );

        var account2 = Account.Create(
            tenantId: "other-tenant",
            number: "ACC-002"
        );

        // Save the test data
        using (var dbContext = factory.CreateDbContext("test-tenant"))
        {
            await dbContext.Database.EnsureDeletedAsync(); // Start with a clean database
            await dbContext.Database.EnsureCreatedAsync();

            await dbContext.Accounts.AddRangeAsync(account1, account2);
            await dbContext.SaveChangesAsync();
        }

        // Act
        using (var dbContext = factory.CreateDbContext("test-tenant"))
        {
            var accounts = await dbContext.Accounts.ToListAsync();

            // Assert
            accounts.Should().HaveCount(1);
            accounts.First().TenantId.Should().Be("test-tenant");
        }

        // Cleanup
        using (var dbContext = factory.CreateDbContext("test-tenant"))
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task CreateDbContext_IsolatesDataBetweenTenants()
    {
        // Arrange
        var tenant1Settings = new TenantSettings
        {
            TenantId = "tenant1",
            Name = "Tenant One",
            ConnectionString = "TestDb",
            IsActive = true
        };

        var tenant2Settings = new TenantSettings
        {
            TenantId = "tenant2",
            Name = "Tenant Two",
            ConnectionString = "TestDb",
            IsActive = true
        };

        _mockTenantManager.Setup(x => x.GetTenantAsync("tenant1"))
            .ReturnsAsync(tenant1Settings);
        _mockTenantManager.Setup(x => x.GetTenantAsync("tenant2"))
            .ReturnsAsync(tenant2Settings);

        var factory = new TenantDbContextFactory(
            _mockTenantManager.Object,
            _optionsBuilder,
            defaultProvider: "InMemory");

        // Create test data for tenant1
        using (var tenant1Context = factory.CreateDbContext("tenant1"))
        {
            await tenant1Context.Database.EnsureDeletedAsync();
            await tenant1Context.Database.EnsureCreatedAsync();

            var account1 = Account.Create(
                tenantId: "tenant1",
                number: "ACC-001"
            );
            tenant1Context.Accounts.Add(account1);
            await tenant1Context.SaveChangesAsync();
        }

        // Create test data for tenant2
        using (var tenant2Context = factory.CreateDbContext("tenant2"))
        {
            var account2 = Account.Create(
                tenantId: "tenant2",
                number: "ACC-002"
            );
            tenant2Context.Accounts.Add(account2);
            await tenant2Context.SaveChangesAsync();
        }

        // Act & Assert for tenant1
        using (var tenant1Context = factory.CreateDbContext("tenant1"))
        {
            var tenant1Accounts = await tenant1Context.Accounts.ToListAsync();
            tenant1Accounts.Should().HaveCount(1);
            tenant1Accounts.First().TenantId.Should().Be("tenant1");
        }

        // Act & Assert for tenant2
        using (var tenant2Context = factory.CreateDbContext("tenant2"))
        {
            var tenant2Accounts = await tenant2Context.Accounts.ToListAsync();
            tenant2Accounts.Should().HaveCount(1);
            tenant2Accounts.First().TenantId.Should().Be("tenant2");
        }

        // Cleanup
        using (var cleanupContext = factory.CreateDbContext("tenant1"))
        {
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }
}