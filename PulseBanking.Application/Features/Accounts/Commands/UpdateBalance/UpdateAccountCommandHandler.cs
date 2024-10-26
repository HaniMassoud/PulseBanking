// Create new file: src/PulseBanking.Application/Features/Accounts/Commands/UpdateBalance/UpdateBalanceCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Features.Accounts.Common;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Application.Features.Accounts.Commands.UpdateBalance;

public class UpdateBalanceCommandHandler : IRequestHandler<UpdateBalanceCommand, AccountDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateBalanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDto> Handle(UpdateBalanceCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account with ID {request.AccountId} not found.");
        }

        switch (request.UpdateType)
        {
            case BalanceUpdateType.Deposit:
                account.Deposit(request.Amount);
                break;

            case BalanceUpdateType.Withdraw:
                account.Withdraw(request.Amount);
                break;

            default:
                throw new ArgumentException($"Unsupported update type: {request.UpdateType}");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new AccountDto
        {
            Id = account.Id,
            Number = account.Number,
            Balance = account.Balance,
            Status = account.Status
        };
    }
}