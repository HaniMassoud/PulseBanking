using MediatR;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;

namespace PulseBanking.Application.Features.Transactions.Commands.CreateTransaction;

public record CreateTransactionCommand : IRequest<BankTransaction>
{
    public required Guid AccountId { get; init; }
    public required TransactionType Type { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Reference { get; init; }
    public string Description { get; init; } = string.Empty;
    public Guid? CounterpartyAccountId { get; init; }
    public DateTime? ValueDate { get; init; }
    public string? ExternalReference { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}