using Microsoft.AspNetCore.Http;
using Moq;
using PulseBanking.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Xunit;
using PulseBanking.Application.Common.Models;  // Correct namespace for TenantSettings
using PulseBanking.Infrastructure.Middleware;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Infrastructure.Tests.Middleware;

public class TenantMiddlewareTests
{
    private readonly Mock<ITenantManager> _tenantManagerMock;
    private readonly Mock<ILogger<TenantMiddleware>> _loggerMock;
    private readonly TenantMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public TenantMiddlewareTests()
    {
        _tenantManagerMock = new Mock<ITenantManager>();
        _loggerMock = new Mock<ILogger<TenantMiddleware>>();
        _httpContext = new DefaultHttpContext();

        _middleware = new TenantMiddleware(
            next: (innerHttpContext) => Task.CompletedTask,
            tenantManager: _tenantManagerMock.Object,
            logger: _loggerMock.Object
        );
    }

    [Fact]
    public async Task InvokeAsync_MissingTenantHeader_ReturnsBadRequest()
    {
        // Arrange
        _httpContext.Request.Path = "/api/some-endpoint";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_RegisterEndpoint_SkipsValidation()
    {
        // Arrange
        _httpContext.Request.Path = "/api/tenants/register";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ValidTenantHeader_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = new TenantMiddleware(
            next: (innerHttpContext) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            tenantManager: _tenantManagerMock.Object,
            logger: _loggerMock.Object
        );

        _httpContext.Request.Headers["X-TenantId"] = "valid-tenant";
        var testTenant = CreateTestTenantSettings("valid-tenant");
        _tenantManagerMock.Setup(x => x.GetTenantAsync("valid-tenant"))
            .ReturnsAsync(testTenant); 

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_InactiveTenant_ReturnsForbidden()
    {
        // Arrange
        _httpContext.Request.Headers["X-TenantId"] = "inactive-tenant";
        var testTenant = CreateTestTenantSettings("inactive-tenant", isActive: false); // Pass isActive as parameter
        _tenantManagerMock.Setup(x => x.GetTenantAsync("inactive-tenant"))
            .ReturnsAsync(testTenant);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_InvalidTenant_ReturnsBadRequest()
    {
        // Arrange
        _httpContext.Request.Headers["X-TenantId"] = "invalid-tenant";
        _tenantManagerMock.Setup(x => x.GetTenantAsync("invalid-tenant"))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, _httpContext.Response.StatusCode);
    }

    private TenantSettings CreateTestTenantSettings(string id, bool isActive = true)
    {
        return new TenantSettings
        {
            Id = id,
            Name = $"Test Tenant {id}",
            DeploymentType = DeploymentType.Shared,
            Region = RegionCode.AUS,
            InstanceType = InstanceType.Production,
            ConnectionString = $"Server=localhost;Database=Test_{id};",
            CreatedAt = DateTime.UtcNow,
            DataSovereigntyCompliant = true,
            IsActive = isActive, // Use the parameter here
            TimeZone = "UTC",
            CurrencyCode = "USD",
            DefaultTransactionLimit = 10000m
        };
    }
}