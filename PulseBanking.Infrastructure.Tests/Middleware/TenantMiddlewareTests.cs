using Microsoft.AspNetCore.Http;
using Moq;
using PulseBanking.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Xunit;
using PulseBanking.Application.Common.Models;  // Correct namespace for TenantSettings
using PulseBanking.Infrastructure.Middleware;

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
        _tenantManagerMock.Setup(x => x.GetTenantAsync("valid-tenant"))
            .ReturnsAsync(new TenantSettings
            {
                TenantId = "valid-tenant",
                Name = "Valid Bank",
                ConnectionString = "Server=localhost;Database=ValidBank;",
                // Optional properties will use their defaults if not specified
            });

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
        _tenantManagerMock.Setup(x => x.GetTenantAsync("inactive-tenant"))
            .ReturnsAsync(new TenantSettings
            {
                TenantId = "inactive-tenant",
                Name = "Inactive Bank",
                ConnectionString = "Server=localhost;Database=InactiveBank;",
                IsActive = false
                // Other properties will use their defaults
            });

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
}