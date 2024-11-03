using PulseBanking.Domain.Common;

namespace PulseBanking.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency code cannot be empty", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money FromDecimal(decimal amount, string currency)
        => new(decimal.Round(amount, 2, MidpointRounding.ToEven), currency);

    public static Money Zero(string currency) => FromDecimal(0, currency);

    public Money Add(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies. Expected {Currency} but got {other.Currency}");

        return FromDecimal(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies. Expected {Currency} but got {other.Currency}");

        return FromDecimal(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        return FromDecimal(Amount * multiplier, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal right) => left.Multiply(right);
    public static Money operator *(decimal left, Money right) => right.Multiply(left);

    public bool IsPositive() => Amount > 0;
    public bool IsNegative() => Amount < 0;
    public bool IsZero() => Amount == 0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}