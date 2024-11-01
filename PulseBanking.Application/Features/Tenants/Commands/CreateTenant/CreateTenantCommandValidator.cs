// src/PulseBanking.Application/Features/Tenants/Commands/CreateTenant/CreateTenantCommandValidator.cs
using FluentValidation;

namespace PulseBanking.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.BankName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .Must(BeValidTimeZone)
            .WithMessage("Invalid time zone");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency code must be a 3-letter code (e.g., USD, AUD)");

        RuleFor(x => x.DefaultTransactionLimit)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000000);

        RuleFor(x => x.AdminEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.AdminPassword)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.AdminFirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.AdminLastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.AdminPhoneNumber)
            .NotEmpty();
    }

    private bool BeValidTimeZone(string timeZone)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return true;
        }
        catch
        {
            return false;
        }
    }
}