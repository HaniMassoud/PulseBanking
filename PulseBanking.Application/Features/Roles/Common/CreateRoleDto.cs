﻿namespace PulseBanking.Application.Features.Roles.Common;

public class CreateRoleDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }

}