// Create new file: src/PulseBanking.Api/Controllers/AccountsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PulseBanking.Application.Features.Accounts.Commands.CreateAccount;
using PulseBanking.Application.Features.Accounts.Commands.UpdateBalance;
using PulseBanking.Application.Features.Accounts.Queries.GetAccount;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantService _tenantService;

    public AccountsController(IMediator mediator, ITenantService tenantService)
    {
        _mediator = mediator;
        _tenantService = tenantService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountCommand command)
    {
        // Verify tenant context before sending command
        var tenantId = _tenantService.GetCurrentTenant();  // This should work since middleware passed

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var query = new GetAccountQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(Guid id, [FromBody] decimal amount)
    {
        var command = new UpdateBalanceCommand
        {
            AccountId = id,
            Amount = amount,
            UpdateType = BalanceUpdateType.Deposit
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] decimal amount)
    {
        var command = new UpdateBalanceCommand
        {
            AccountId = id,
            Amount = amount,
            UpdateType = BalanceUpdateType.Withdraw
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}