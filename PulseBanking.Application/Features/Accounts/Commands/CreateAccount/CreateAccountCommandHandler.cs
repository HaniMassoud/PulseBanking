// Create new file: src/PulseBanking.Application/Features/Accounts/Commands/CreateAccount/CreateAccountCommandHandler.cs
using MediatR;
using PulseBanking.Application.Features.Accounts.Common;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public CreateAccountCommandHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenant();

        var account = Account.Create(
            tenantId: tenantId,
            number: request.Number,
            initialBalance: request.InitialBalance);

        _context.Accounts.Add(account);
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