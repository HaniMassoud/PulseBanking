using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Application.Features.Customers.Queries.GetCustomerList;

public class GetCustomerListQueryHandler : IRequestHandler<GetCustomerListQuery, List<Customer>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> Handle(GetCustomerListQuery request, CancellationToken cancellationToken)
    {
        return await _context.Customers.ToListAsync(cancellationToken);
    }
}