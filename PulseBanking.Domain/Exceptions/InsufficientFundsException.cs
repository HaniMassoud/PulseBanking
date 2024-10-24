// In PulseBanking.Domain/Exceptions/InsufficientFundsException.cs
namespace PulseBanking.Domain.Exceptions;

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException()
        : base("Insufficient funds for this transaction.")
    {
    }

    public InsufficientFundsException(string message)
        : base(message)
    {
    }

    public InsufficientFundsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}