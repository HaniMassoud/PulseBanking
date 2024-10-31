// Update src/PulseBanking.Api/Controllers/TenantsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PulseBanking.Infrastructure.Attributes;
using PulseBanking.Application.Features.Tenants.Commands.CreateTenant;
using PulseBanking.Application.Features.Tenants.Common;

namespace PulseBanking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(IMediator mediator, ILogger<TenantsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("register")]
    [SkipTenantValidation]
    public async Task<IActionResult> Register(CreateTenantCommand command)
    {
        _logger.LogInformation("Received tenant registration request at /register endpoint: {@Command}", command);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _mediator.Send(command);
            _logger.LogInformation("Successfully registered tenant: {@Result}", result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register tenant: {Message}", ex.Message);
            return BadRequest(new { error = "Failed to register tenant", message = ex.Message });
        }
    }

    // Your other tenant-related endpoints that DO require tenant header can go here
    [HttpPost]
    public async Task<IActionResult> CreateTenant(CreateTenantCommand command)
    {
        // This endpoint would still require the tenant header
        _logger.LogInformation("Received create tenant request: {@Command}", command);

        try
        {
            var result = await _mediator.Send(command);
            _logger.LogInformation("Successfully created tenant: {@Result}", result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tenant");
            return BadRequest(new { error = "Failed to create tenant", message = ex.Message });
        }
    }
}