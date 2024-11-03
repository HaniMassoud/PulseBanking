// Create new file: src/PulseBanking.Application/Features/Customers/Queries/GetCustomerList/GetCustomerListQuery.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using PulseBanking.Application.Interfaces;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Features.Customers.Queries.GetCustomerList;

public record GetCustomerListQuery : IRequest<List<Customer>>;

