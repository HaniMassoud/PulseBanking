// Create tests/PulseBanking.Infrastructure.Tests/Middleware/TenantMiddlewareTests.cs
using Microsoft.AspNetCore.Http;
using PulseBanking.Application.Common.Models;
using PulseBanking.Application.Interfaces;
using PulseBanking.Infrastructure.Middleware;
using FluentAssertions;
using Moq;
using System.Text.Json;

namespace PulseBanking.Infrastructure.Tests.Middleware;

public class TenantMiddlewareTests
{
    private readonly Mock<ITenantManager> _mockTenantManager;
    private readonly Mock<RequestDelegate> _nextMiddleware;

    public TenantMiddlewareTests()
    {
        _mockTenantManager = new Mock<ITenantManager>();
        _nextMiddleware = new Mock<RequestDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_WithValidActiveTenant_CallsNextMiddleware()
    {
        // Arrange
        var middleware = new TenantMiddleware(_nextMiddleware.Object, _mockTenantManager.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-TenantId"] = "valid-tenant";

        _mockTenantManager.Setup(x => x.GetTenantAsync("valid-tenant"))
            .ReturnsAsync(new TenantSettings
            {
                TenantId = "valid-tenant",
                Name = "Test Tenant",
                ConnectionString = "test-connection",
                IsActive = true
            });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMiddleware.Verify(next => next(context), Times.Once);
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_WithMissingTenantHeader_ReturnsBadRequest()
    {
        // Arrange
        var middleware = new TenantMiddleware(_nextMiddleware.Object, _mockTenantManager.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _nextMiddleware.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyTenantHeader_ReturnsBadRequest()
    {
        // Arrange
        var middleware = new TenantMiddleware(_nextMiddleware.Object, _mockTenantManager.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-TenantId"] = string.Empty;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _nextMiddleware.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithInactiveTenant_ReturnsForbidden()
    {
        // Arrange
        var middleware = new TenantMiddleware(_nextMiddleware.Object, _mockTenantManager.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-TenantId"] = "inactive-tenant";

        _mockTenantManager.Setup(x => x.GetTenantAsync("inactive-tenant"))
            .ReturnsAsync(new TenantSettings
            {
                TenantId = "inactive-tenant",
                Name = "Inactive Tenant",
                ConnectionString = "test-connection",
                IsActive = false
            });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _nextMiddleware.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithNonexistentTenant_ReturnsBadRequest()
    {
        // Arrange
        var middleware = new TenantMiddleware(_nextMiddleware.Object, _mockTenantManager.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-TenantId"] = "nonexistent-tenant";

        _mockTenantManager.Setup(x => x.GetTenantAsync("nonexistent-tenant"))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _nextMiddleware.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    private async Task<string> ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}