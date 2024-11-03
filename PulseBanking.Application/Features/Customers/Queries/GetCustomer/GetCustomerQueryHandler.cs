using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Common.Exceptions;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Features.Customers.Queries.GetCustomer;

public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, Customer>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetCustomerQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Customer> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var tenant = _tenantService.GetCurrentTenant();
        if (tenant == null)
            throw new UnauthorizedAccessException("No tenant context found");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.Id);
        }

        // Verify tenant access
        if (customer.TenantId != tenant.Id)
        {
            throw new UnauthorizedAccessException("Customer does not belong to the current tenant");
        }

        return customer;
    }
}