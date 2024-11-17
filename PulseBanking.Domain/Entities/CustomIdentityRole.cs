using Microsoft.AspNetCore.Identity;


namespace PulseBanking.Domain.Entities;

public class CustomIdentityRole : IdentityRole
{
    public string TenantId { get; set; } = string.Empty;  // Changed from required to having a default
    public string? Description { get; set; }

    public CustomIdentityRole() : base() { }

    public CustomIdentityRole(string roleName) : base(roleName) { }

    public static CustomIdentityRole Create(string tenantId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));


        return new CustomIdentityRole(name)
        {
            TenantId = tenantId,
            Description = description
        };
    }

}

