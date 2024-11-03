// DTOs/UserDto.cs
namespace PulseBanking.Application.Features.Users.Common;
public class UserDto
{
    public required string Id { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required bool EmailConfirmed { get; init; }
    public string? PhoneNumber { get; init; }
    public required bool PhoneNumberConfirmed { get; init; }
    public required bool TwoFactorEnabled { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public required bool LockoutEnabled { get; init; }
    public required int AccessFailedCount { get; init; }
}