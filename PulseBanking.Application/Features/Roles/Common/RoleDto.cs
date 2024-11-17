namespace PulseBanking.Application.Features.Roles.Common;

public class RoleDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    public string? Description { get; init; }
}