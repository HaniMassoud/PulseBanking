// Create new file: src/PulseBanking.Application/Features/Accounts/Commands/UpdateBalance/UpdateBalanceCommandValidator.cs
using FluentValidation;

namespace PulseBanking.Application.Features.Accounts.Commands.UpdateBalance;

public class UpdateBalanceCommandValidator : AbstractValidator<UpdateBalanceCommand>
{
    public UpdateBalanceCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.UpdateType)
            .IsInEnum().WithMessage("Invalid update type");
    }
}