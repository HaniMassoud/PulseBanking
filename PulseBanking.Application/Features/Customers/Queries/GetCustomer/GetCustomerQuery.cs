// Create new file: src/PulseBanking.Application/Features/Customers/Queries/GetCustomer/GetCustomerQuery.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Features.Customers.Queries.GetCustomer;

public record GetCustomerQuery(Guid Id) : IRequest<Customer>;

