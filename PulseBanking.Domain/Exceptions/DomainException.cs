namespace PulseBanking.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException()
    {
    }

    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public virtual string Code { get; } = "DomainError";
    public virtual string Title { get; } = "A domain error has occurred.";
    public virtual int StatusCode { get; } = 400;
}

// Example of a specific domain exception
public class InvalidTenantOperationException : DomainException
{
    public InvalidTenantOperationException(string message)
        : base(message)
    {
    }

    public override string Code => "InvalidTenantOperation";
    public override string Title => "Invalid operation for the current tenant.";
    public override int StatusCode => 403;
}

// Example of another specific domain exception
public class InvalidAccountOperationException : DomainException
{
    public InvalidAccountOperationException(string message)
        : base(message)
    {
    }

    public override string Code => "InvalidAccountOperation";
    public override string Title => "Invalid operation for the specified account.";
    public override int StatusCode => 400;
}