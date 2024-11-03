using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Common.Exceptions;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Application.Features.Transactions.Queries.GetTransaction;

public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, BankTransaction>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetTransactionQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<BankTransaction> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenant();

        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(BankTransaction), request.Id);

        if (transaction.TenantId != tenantId)
            throw new UnauthorizedAccessException("Transaction does not belong to the current tenant");

        return transaction;
    }
}