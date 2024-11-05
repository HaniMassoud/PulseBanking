namespace PulseBanking.Application.Features.Users.Common;

public class CreateUserDto
{
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string PhoneNumber { get; init; }
    public List<string> Roles { get; init; } = new();
    public required string TenantId { get; init; }
}