// Create new file: src/PulseBanking.Application/Features/Accounts/Common/AccountDto.cs
using PulseBanking.Domain.Enums;

namespace PulseBanking.Application.Features.Accounts.Common;

public class AccountDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AccountStatus Status { get; set; }
}