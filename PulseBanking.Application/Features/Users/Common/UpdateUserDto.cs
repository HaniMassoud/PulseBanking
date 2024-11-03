namespace PulseBanking.Application.Features.Users.Common;
public class UpdateUserDto
{
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public required bool EmailConfirmed { get; init; }
    public required bool PhoneNumberConfirmed { get; init; }
    public required bool TwoFactorEnabled { get; init; }
    public required bool LockoutEnabled { get; init; }
}