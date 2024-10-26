// Create new file: src/PulseBanking.Application/Features/Accounts/Commands/CreateAccount/CreateAccountCommandValidator.cs
using FluentValidation;

namespace PulseBanking.Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Account number is required")
            .MaximumLength(50).WithMessage("Account number must not exceed 50 characters")
            .Matches("^[A-Za-z0-9-]{1,50}$").WithMessage("Account number can only contain letters, numbers, and hyphens");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Initial balance cannot be negative");
    }
}