// src/PulseBanking.Domain/Constants/Roles.cs

namespace PulseBanking.Domain.Constants;

public static class Roles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string TenantAdmin = "TenantAdmin";
    public const string BankManager = "BankManager";
    public const string CustomerService = "CustomerService";
    public const string Customer = "Customer";
    public const string ReadOnly = "ReadOnly";

    public static readonly IReadOnlyList<string> AllRoles = new[]
    {
        SystemAdmin,
        TenantAdmin,
        BankManager,
        CustomerService,
        Customer,
        ReadOnly
    };

    public static readonly IReadOnlyDictionary<string, string> RoleDescriptions = new Dictionary<string, string>
    {
        { SystemAdmin, "Full system access across all tenants" },
        { TenantAdmin, "Full access to tenant-specific settings and user management" },
        { BankManager, "Access to manage accounts and view all transactions" },
        { CustomerService, "Access to help customers and view account information" },
        { Customer, "Basic account access" },
        { ReadOnly, "View-only access for auditing purposes" }
    };
}