// Update tests/PulseBanking.Infrastructure.Tests/Services/TenantManagerTests.cs
using Microsoft.Extensions.Configuration;
using PulseBanking.Application.Common.Models;
using PulseBanking.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace PulseBanking.Infrastructure.Tests.Services;

public class TenantManagerTests
{
    private readonly IConfiguration _configuration;

    public TenantManagerTests()
    {
        // Setup configuration with test data
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Tenants:tenant1:Id", "tenant1"},
            {"Tenants:tenant1:Name", "Test Bank 1"},
            {"Tenants:tenant1:ConnectionString", "TestConnection1"},
            {"Tenants:tenant1:CurrencyCode", "USD"},
            {"Tenants:tenant1:DefaultTransactionLimit", "10000"},
            {"Tenants:tenant1:TimeZone", "UTC"},
            {"Tenants:tenant1:IsActive", "true"},

            {"Tenants:tenant2:Id", "tenant2"},
            {"Tenants:tenant2:Name", "Test Bank 2"},
            {"Tenants:tenant2:ConnectionString", "TestConnection2"},
            {"Tenants:tenant2:CurrencyCode", "EUR"},
            {"Tenants:tenant2:DefaultTransactionLimit", "15000"},
            {"Tenants:tenant2:TimeZone", "UTC"},
            {"Tenants:tenant2:IsActive", "false"},
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task GetTenantAsync_WithValidId_ReturnsTenantSettings()
    {
        // Arrange
        var tenantManager = new TenantManager(_configuration);

        // Act
        var result = await tenantManager.GetTenantAsync("tenant1");

        // Assert
        result.Should().NotBeNull();
        result.TenantId.Should().Be("tenant1");
        result.Name.Should().Be("Test Bank 1");
        result.ConnectionString.Should().Be("TestConnection1");
        result.CurrencyCode.Should().Be("USD");
        result.DefaultTransactionLimit.Should().Be(10000);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetTenantAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var tenantManager = new TenantManager(_configuration);

        // Act
        Func<Task> act = () => tenantManager.GetTenantAsync("nonexistent");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetAllTenantsAsync_ReturnsAllConfiguredTenants()
    {
        // Arrange
        var tenantManager = new TenantManager(_configuration);

        // Act
        var result = await tenantManager.GetAllTenantsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.TenantId == "tenant1");
        result.Should().Contain(t => t.TenantId == "tenant2");
    }

    [Fact]
    public async Task TenantExistsAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var tenantManager = new TenantManager(_configuration);

        // Act
        var result = await tenantManager.TenantExistsAsync("tenant1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TenantExistsAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var tenantManager = new TenantManager(_configuration);

        // Act
        var result = await tenantManager.TenantExistsAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
    }
}