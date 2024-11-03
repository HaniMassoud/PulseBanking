using MediatR;
using Microsoft.AspNetCore.Mvc;
using PulseBanking.Application.Common.Interfaces;
using PulseBanking.Application.Features.Transactions.Commands.CreateTransaction;
using PulseBanking.Application.Features.Transactions.Queries.GetTransaction;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantService _tenantService;
    private readonly ITransactionProcessor _transactionProcessor;

    public TransactionsController(
        IMediator mediator,
        ITenantService tenantService,
        ITransactionProcessor transactionProcessor)
    {
        _mediator = mediator;
        _tenantService = tenantService;
        _transactionProcessor = transactionProcessor;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BankTransaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var query = new GetTransactionQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BankTransaction), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransaction(CreateTransactionCommand command)
    {
        var result = await _mediator.Send(command);

        // Process the transaction
        await _transactionProcessor.ProcessTransactionAsync(result);

        return CreatedAtAction(nameof(GetTransaction), new { id = result.Id }, result);
    }

    [HttpPost("{id}/reverse")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReverseTransaction(Guid id, [FromBody] string reason)
    {
        await _transactionProcessor.ReverseTransactionAsync(id, reason);
        return Ok();
    }
}