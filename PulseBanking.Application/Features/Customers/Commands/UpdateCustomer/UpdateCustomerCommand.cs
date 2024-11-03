using MediatR;
using PulseBanking.Domain.Entities;

namespace PulseBanking.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand : IRequest<Customer>
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
}