using MediatR;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Customer>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public CreateCustomerCommandHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Customer> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenant();

        var customer = Customer.Create(
            tenantId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer;
    }
}

