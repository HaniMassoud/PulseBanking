// Create new file: src/PulseBanking.Domain/Entities/Customer.cs
using PulseBanking.Domain.Common;

namespace PulseBanking.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    private readonly List<Account> _accounts = new();
    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    protected Customer() { }

    private Customer(
        string tenantId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber)
    {
        TenantId = tenantId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public static Customer Create(
        string tenantId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be empty", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return new Customer(tenantId, firstName, lastName, email, phoneNumber);
    }

    public void AddAccount(Account account)
    {
        if (account.TenantId != TenantId)
            throw new InvalidOperationException("Account must belong to the same tenant");

        _accounts.Add(account);
    }
}