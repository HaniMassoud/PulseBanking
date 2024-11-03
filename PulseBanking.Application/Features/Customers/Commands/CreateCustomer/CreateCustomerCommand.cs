using MediatR;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand : IRequest<Customer>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
}