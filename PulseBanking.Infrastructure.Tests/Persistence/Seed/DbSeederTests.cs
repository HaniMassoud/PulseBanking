using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;
using PulseBanking.Infrastructure.Persistence;
using PulseBanking.Infrastructure.Persistence.Seed;

namespace PulseBanking.Infrastructure.Tests.Persistence.Seed;

public class DbSeederTests
{
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<ILogger<ApplicationDbContext>> _loggerMock;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public DbSeederTests()
    {
        _tenantServiceMock = new Mock<ITenantService>();
        _tenantServiceMock.Setup(x => x.GetCurrentTenant()).Returns("test-tenant");

        // Set up UserManager mock
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object,
            null, null, null, null, null, null, null, null);

        // Set up RoleManager mock
        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            roleStoreMock.Object, null, null, null, null);

        _loggerMock = new Mock<ILogger<ApplicationDbContext>>();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task SeedDataAsync_ShouldCreateCustomerAndAccounts_WhenNoAccountsExist()
    {
        // Arrange
        var tenantId = "test-tenant";
        using var context = new ApplicationDbContext(_options, _tenantServiceMock.Object);

        // Set up UserManager mock behavior
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await DbSeeder.SeedDataAsync(
            context,
            tenantId,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _loggerMock.Object);

        // Assert
        var customer = await context.Customers.FirstOrDefaultAsync();
        Assert.NotNull(customer);
        Assert.Equal(tenantId, customer.TenantId);

        var accounts = await context.Accounts.ToListAsync();
        Assert.Equal(3, accounts.Count);
        Assert.All(accounts, account => Assert.Equal(tenantId, account.TenantId));
    }

    [Fact]
    public async Task SeedDataAsync_ShouldNotCreateDuplicates_WhenAccountsExist()
    {
        // Arrange
        var tenantId = "test-tenant";
        using var context = new ApplicationDbContext(_options, _tenantServiceMock.Object);

        // Add existing customer and account
        var existingCustomer = Customer.Create(
            tenantId: tenantId,
            firstName: "Existing",
            lastName: "Customer",
            email: "existing@test.com",
            phoneNumber: "1234567890");

        var existingAccount = Account.Create(
            tenantId: tenantId,
            number: "EXISTING-001",
            customerId: existingCustomer.Id,
            initialBalance: 100m);

        context.Customers.Add(existingCustomer);
        context.Accounts.Add(existingAccount);
        await context.SaveChangesAsync();

        // Act
        await DbSeeder.SeedDataAsync(
            context,
            tenantId,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _loggerMock.Object);

        // Assert
        var customerCount = await context.Customers.CountAsync();
        var accountCount = await context.Accounts.CountAsync();

        Assert.Equal(1, customerCount);
        Assert.Equal(1, accountCount);
    }

    [Fact]
    public async Task SeedDefaultTenantsAsync_ShouldCreateSystemAndDemoTenants_WhenNoTenantsExist()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options, _tenantServiceMock.Object);

        // Act
        await DbSeeder.SeedDefaultTenantsAsync(
            context,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _loggerMock.Object);

        // Assert
        var tenants = await context.Tenants.ToListAsync();
        Assert.Equal(2, tenants.Count);

        var systemTenant = tenants.FirstOrDefault(t => t.Id == "system");
        Assert.NotNull(systemTenant);
        Assert.True(systemTenant.IsActive);

        var demoTenant = tenants.FirstOrDefault(t => t.Id == "demo");
        Assert.NotNull(demoTenant);
        Assert.True(demoTenant.IsActive);
    }

    [Fact]
    public async Task SeedDefaultTenantsAsync_ShouldNotCreateDuplicates_WhenTenantsExist()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options, _tenantServiceMock.Object);

        // Add an existing tenant with all required properties
        var existingTenant = new Tenant
        {
            Id = "system",
            Name = "Existing System",
            DeploymentType = DeploymentType.Shared,
            Region = RegionCode.AUS,
            InstanceType = InstanceType.Production,
            ConnectionString = "Server=(local);Database=PulseBanking_ExistingSystem;Trusted_Connection=True;MultipleActiveResultSets=true;Trust Server Certificate=True;",
            CurrencyCode = "USD",
            DefaultTransactionLimit = 10000m,
            TimeZone = "UTC",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            DataSovereigntyCompliant = true
        };

        context.Tenants.Add(existingTenant);
        await context.SaveChangesAsync();

        // Act
        await DbSeeder.SeedDefaultTenantsAsync(
            context,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _loggerMock.Object);

        // Assert
        var tenantCount = await context.Tenants.CountAsync();
        Assert.Equal(1, tenantCount);

        var tenant = await context.Tenants.FirstAsync();
        Assert.Equal("system", tenant.Id);
        Assert.Equal("Existing System", tenant.Name);
        Assert.Equal(DeploymentType.Shared, tenant.DeploymentType);
        Assert.Equal(RegionCode.AUS, tenant.Region);
    }
}