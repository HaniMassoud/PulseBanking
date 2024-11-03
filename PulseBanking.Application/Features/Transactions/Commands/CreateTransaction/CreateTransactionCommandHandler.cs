using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Exceptions;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.ValueObjects;

namespace PulseBanking.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, BankTransaction>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IDateTime _dateTime;
    private readonly ILogger<CreateTransactionCommandHandler> _logger;

    public CreateTransactionCommandHandler(
        IApplicationDbContext context,
        ITenantService tenantService,
        IDateTime dateTime,
        ILogger<CreateTransactionCommandHandler> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<BankTransaction> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var tenant = _tenantService.GetCurrentTenant();
        if (tenant == null)
            throw new UnauthorizedAccessException("No tenant context found");

        // Verify account exists and belongs to tenant
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);

        if (account.TenantId != tenant.Id)
            throw new UnauthorizedAccessException("Account does not belong to the current tenant");


        // Create Money value object
        var amount = Money.FromDecimal(request.Amount, request.Currency);

        // For transfers, validate counterparty account
        if (request.Type == TransactionType.Transfer && request.CounterpartyAccountId == null)
        {
            throw new ValidationException("CounterpartyAccountId", "Counterparty account is required for transfers");
        }

        // Create transaction
        var transaction = BankTransaction.Create(
            tenantId: tenant.Id,
            accountId: request.AccountId,
            type: request.Type,
            amount: amount,
            reference: request.Reference,
            description: request.Description,
            counterpartyAccountId: request.CounterpartyAccountId,
            valueDate: request.ValueDate,
            externalReference: request.ExternalReference,
            metadata: request.Metadata
        );

        // Update account balance based on transaction type
        switch (request.Type)
        {
            case TransactionType.Deposit:
            case TransactionType.DirectCredit:
            case TransactionType.Interest:
            case TransactionType.Refund:
                account.Deposit(amount.Amount);
                break;

            case TransactionType.Withdrawal:
            case TransactionType.DirectDebit:
            case TransactionType.Fee:
            case TransactionType.ATMWithdrawal:
            case TransactionType.POSPayment:
                account.Withdraw(amount.Amount);
                break;

            case TransactionType.Transfer:
                if (request.CounterpartyAccountId == null)
                    throw new ValidationException("CounterpartyAccountId", "Counterparty account is required for transfers");

                // Handle transfer logic
                account.Withdraw(amount.Amount);
                // Note: Counterparty credit should be handled separately
                break;

            default:
                throw new ValidationException("TransactionType", $"Unsupported transaction type: {request.Type}");
        }

        // Update running balance
        transaction.UpdateRunningBalance(Money.FromDecimal(account.Balance, request.Currency));

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return transaction;
    }
}