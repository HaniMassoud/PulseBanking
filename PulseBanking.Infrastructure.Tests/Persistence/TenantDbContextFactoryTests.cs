// Update tests/PulseBanking.Infrastructure.Tests/Persistence/TenantDbContextFactoryTests.cs
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Persistence;
using FluentAssertions;
using Moq;
using PulseBanking.Domain.Entities;
using PulseBanking.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace PulseBanking.Infrastructure.Tests.Persistence;

public class TenantDbContextFactoryTests
{
    private readonly Mock<ITenantManager> _mockTenantManager;
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;

    public TenantDbContextFactoryTests()
    {
        _mockTenantManager = new Mock<ITenantManager>();
        _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}");
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
            _optionsBuilder);

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
            _optionsBuilder);

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

        // Create a test context with tenant1
        var tenantService1 = new TenantService(new HttpContextAccessor(), _mockTenantManager.Object, "test-tenant");
        using var context1 = new ApplicationDbContext(_optionsBuilder.Options, tenantService1);

        // Add test data
        var account1 = Account.Create(
            tenantId: "test-tenant",
            number: "ACC-001"
        );

        var account2 = Account.Create(
            tenantId: "other-tenant",
            number: "ACC-002"
        );

        await context1.Accounts.AddRangeAsync(account1, account2);
        await context1.SaveChangesAsync();

        // Act - Create a new context with tenant filter
        var tenantService2 = new TenantService(new HttpContextAccessor(), _mockTenantManager.Object, "test-tenant");
        using var context2 = new ApplicationDbContext(_optionsBuilder.Options, tenantService2);
        var accounts = await context2.Accounts.ToListAsync();

        // Assert
        accounts.Should().HaveCount(1);
        accounts.First().TenantId.Should().Be("test-tenant");

        // Cleanup
        await context1.Database.EnsureDeletedAsync();
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

        // Create contexts with specific tenant services
        var tenantService1 = new TenantService(new HttpContextAccessor(), _mockTenantManager.Object, "tenant1");
        var tenantService2 = new TenantService(new HttpContextAccessor(), _mockTenantManager.Object, "tenant2");

        // Create test data for tenant1
        using (var context1 = new ApplicationDbContext(_optionsBuilder.Options, tenantService1))
        {
            var account1 = Account.Create(
                tenantId: "tenant1",
                number: "ACC-001"
            );
            await context1.Accounts.AddAsync(account1);
            await context1.SaveChangesAsync();
        }

        // Create test data for tenant2
        using (var context2 = new ApplicationDbContext(_optionsBuilder.Options, tenantService2))
        {
            var account2 = Account.Create(
                tenantId: "tenant2",
                number: "ACC-002"
            );
            await context2.Accounts.AddAsync(account2);
            await context2.SaveChangesAsync();
        }

        // Act & Assert for tenant1
        using (var context1 = new ApplicationDbContext(_optionsBuilder.Options, tenantService1))
        {
            var tenant1Accounts = await context1.Accounts.ToListAsync();
            tenant1Accounts.Should().HaveCount(1);
            tenant1Accounts.First().TenantId.Should().Be("tenant1");
        }

        // Act & Assert for tenant2
        using (var context2 = new ApplicationDbContext(_optionsBuilder.Options, tenantService2))
        {
            var tenant2Accounts = await context2.Accounts.ToListAsync();
            tenant2Accounts.Should().HaveCount(1);
            tenant2Accounts.First().TenantId.Should().Be("tenant2");
        }

        // Cleanup
        using (var context = new ApplicationDbContext(_optionsBuilder.Options, tenantService1))
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}