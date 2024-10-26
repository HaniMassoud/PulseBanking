// Create new file: src/PulseBanking.Application/Features/Accounts/Commands/UpdateBalance/UpdateBalanceCommand.cs
using MediatR;
using PulseBanking.Application.Features.Accounts.Common;

namespace PulseBanking.Application.Features.Accounts.Commands.UpdateBalance;

public record UpdateBalanceCommand : IRequest<AccountDto>
{
    public required Guid AccountId { get; init; }
    public required decimal Amount { get; init; }
    public required BalanceUpdateType UpdateType { get; init; }
}

public enum BalanceUpdateType
{
    Deposit,
    Withdraw
}