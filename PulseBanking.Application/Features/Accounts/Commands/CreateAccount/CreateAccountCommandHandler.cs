// Create new file: src/PulseBanking.Application/Features/Accounts/Commands/CreateAccount/CreateAccountCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Common.Exceptions;
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
        var tenant = _tenantService.GetCurrentTenant();
        if (tenant == null)
            throw new UnauthorizedAccessException("No tenant context found");

        // Verify customer exists and belongs to the tenant
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundException("Customer not found");

        if (customer.TenantId != tenant.Id)
            throw new InvalidOperationException("Customer does not belong to the current tenant");

        var account = Account.Create(
            tenantId: tenant.Id,
            number: request.Number,
            customerId: request.CustomerId,
            initialBalance: request.InitialBalance);

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        return new AccountDto
        {
            Id = account.Id,
            Number = account.Number,
            CustomerId = account.CustomerId,  // Add this to DTO
            Balance = account.Balance,
            Status = account.Status
        };
    }
}