using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseBanking.Application.Common.Exceptions;
using PulseBanking.Application.Common.Interfaces;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.ValueObjects;

namespace PulseBanking.Infrastructure.Services;

public class TransactionProcessor : ITransactionProcessor
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IDateTime _dateTime;
    private readonly IAuditService _auditService;
    private readonly ILogger<TransactionProcessor> _logger;

    public TransactionProcessor(
        IApplicationDbContext context,
        ITenantService tenantService,
        IDateTime dateTime,
        IAuditService auditService,
        ILogger<TransactionProcessor> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _dateTime = dateTime;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task ProcessTransactionAsync(
    BankTransaction transaction,
    CancellationToken cancellationToken = default)
    {
        var tenant = _tenantService.GetCurrentTenant()
            ?? throw new UnauthorizedAccessException("No tenant context found");

        if (transaction.TenantId != tenant.Id)
            throw new UnauthorizedAccessException("Transaction does not belong to the current tenant");

        try
        {
            // Load the account with tracking
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == transaction.AccountId, cancellationToken)
                ?? throw new NotFoundException(nameof(Account), transaction.AccountId);

            // Process based on transaction type
            switch (transaction.Type)
            {
                case TransactionType.Deposit:
                case TransactionType.DirectCredit:
                    account.Deposit(transaction.Amount.Amount);
                    break;

                case TransactionType.Withdrawal:
                case TransactionType.DirectDebit:
                    account.Withdraw(transaction.Amount.Amount);
                    break;

                case TransactionType.Transfer:
                    if (transaction.CounterpartyAccountId == null)
                        throw new InvalidOperationException("Counterparty account is required for transfers");

                    var counterparty = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.Id == transaction.CounterpartyAccountId, cancellationToken)
                        ?? throw new NotFoundException(nameof(Account), transaction.CounterpartyAccountId);

                    account.Withdraw(transaction.Amount.Amount);
                    counterparty.Deposit(transaction.Amount.Amount);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported transaction type: {transaction.Type}");
            }

            // Update running balance
            transaction.UpdateRunningBalance(Money.FromDecimal(account.Balance, transaction.Amount.Currency));

            // Add audit trail
            _auditService.AddAuditTrail(
                "ProcessTransaction",
                nameof(BankTransaction),
                transaction.Id.ToString(),
                $"Processed {transaction.Type} transaction for amount {transaction.Amount}");

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Transaction {TransactionId} processed successfully for account {AccountId}",
                transaction.Id,
                account.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing transaction {TransactionId} for account {AccountId}",
                transaction.Id,
                transaction.AccountId);
            throw;
        }
    }

    public async Task ReverseTransactionAsync(
        Guid transactionId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(BankTransaction), transactionId);

        // Create reversal transaction
        var reversal = BankTransaction.Create(
            tenantId: transaction.TenantId,
            accountId: transaction.AccountId,
            type: TransactionType.Adjustment,
            amount: transaction.Amount.Multiply(-1),  // Reverse the amount
            reference: $"REV-{transaction.Reference}",
            description: $"Reversal of transaction {transaction.Reference}: {reason}",
            counterpartyAccountId: transaction.CounterpartyAccountId,
            valueDate: _dateTime.UtcNow);

        await ProcessTransactionAsync(reversal, cancellationToken);

        _auditService.AddAuditTrail(
            "ReverseTransaction",
            nameof(BankTransaction),
            transaction.Id.ToString(),
            $"Transaction reversed: {reason}");

        await _context.SaveChangesAsync(cancellationToken);
    }
}