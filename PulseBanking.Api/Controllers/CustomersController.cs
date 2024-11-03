// Create new file: src/PulseBanking.Api/Controllers/CustomersController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PulseBanking.Application.Features.Customers.Commands.CreateCustomer;
using PulseBanking.Application.Features.Customers.Commands.UpdateCustomer;
using PulseBanking.Application.Features.Customers.Queries.GetCustomer;
using PulseBanking.Application.Features.Customers.Queries.GetCustomerList;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantService _tenantService;

    public CustomersController(IMediator mediator, ITenantService tenantService)
    {
        _mediator = mediator;
        _tenantService = tenantService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CreateCustomerCommand command)
    {
        var tenantId = _tenantService.GetCurrentTenant();
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCustomer), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerList()
    {
        var query = new GetCustomerListQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        var query = new GetCustomerQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, UpdateCustomerCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}