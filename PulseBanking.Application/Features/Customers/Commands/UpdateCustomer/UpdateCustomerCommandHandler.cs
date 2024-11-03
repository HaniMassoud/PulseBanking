using MediatR;
using PulseBanking.Application.Common.Exceptions;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PulseBanking.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Customer>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.Id);
        }

        customer.UpdateDetails(request.FirstName, request.LastName, request.Email, request.PhoneNumber);

        await _context.SaveChangesAsync(cancellationToken);

        return customer;
    }
}