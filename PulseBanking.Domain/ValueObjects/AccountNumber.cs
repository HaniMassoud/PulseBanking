using PulseBanking.Domain.Common;

namespace PulseBanking.Domain.ValueObjects;

public class AccountNumber : ValueObject
{
    public string Value { get; }

    private AccountNumber(string value)
    {
        Value = value;
    }

    public static AccountNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account number cannot be empty", nameof(value));

        // Remove any spaces or special characters
        value = new string(value.Where(char.IsLetterOrDigit).ToArray());

        if (value.Length < 8 || value.Length > 34) // IBAN max length is 34
            throw new ArgumentException("Account number must be between 8 and 34 characters", nameof(value));

        return new AccountNumber(value);
    }

    public static AccountNumber Generate(string bankCode, string branchCode, string sequence)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
            throw new ArgumentException("Bank code cannot be empty", nameof(bankCode));

        if (string.IsNullOrWhiteSpace(branchCode))
            throw new ArgumentException("Branch code cannot be empty", nameof(branchCode));

        if (string.IsNullOrWhiteSpace(sequence))
            throw new ArgumentException("Sequence cannot be empty", nameof(sequence));

        var accountNumber = $"{bankCode}{branchCode}{sequence}";
        return Create(accountNumber);
    }

    public string Format()
    {
        // Format the account number in groups of 4 digits
        return string.Join(" ", Value.Chunk(4).Select(c => new string(c)));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Format();
}