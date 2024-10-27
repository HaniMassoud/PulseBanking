// Create new file: src/PulseBanking.Application/Features/Accounts/Commands/CreateAccount/CreateAccountCommand.cs
using MediatR;
using PulseBanking.Application.Features.Accounts.Common;

namespace PulseBanking.Application.Features.Accounts.Commands.CreateAccount;

public record CreateAccountCommand : IRequest<AccountDto>
{
    public required string Number { get; init; }
    public required Guid CustomerId { get; init; }
    public decimal InitialBalance { get; init; }
}