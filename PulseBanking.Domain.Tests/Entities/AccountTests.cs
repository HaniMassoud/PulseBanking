// Update tests/PulseBanking.Domain.Tests/Entities/AccountTests.cs
using FluentAssertions;
using PulseBanking.Domain.Entities;
using PulseBanking.Domain.Enums;
using PulseBanking.Domain.Exceptions;

namespace PulseBanking.Domain.Tests.Entities;

public class AccountTests
{
    private const string TEST_TENANT_ID = "test-tenant-001";

    // ... existing test methods stay the same ...

    private static Account CreateActiveAccount(decimal initialBalance = 0)
    {
        return Account.Create(
            TEST_TENANT_ID,  // Add tenant ID
            "TEST-001",
            initialBalance,
            AccountStatus.Active
        );
    }

    private static Account CreateInactiveAccount()
    {
        return Account.Create(
            TEST_TENANT_ID,  // Add tenant ID
            "TEST-002",
            0,
            AccountStatus.Inactive
        );
    }
}