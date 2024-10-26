// Create new file: src/PulseBanking.Application/Features/Accounts/Queries/GetAccount/GetAccountQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Features.Accounts.Common;
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Application.Features.Accounts.Queries.GetAccount;

public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, AccountDto>
{
    private readonly IApplicationDbContext _context;

    public GetAccountQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account with ID {request.Id} not found.");
        }

        return new AccountDto
        {
            Id = account.Id,
            Number = account.Number,
            Balance = account.Balance,
            Status = account.Status
        };
    }
}