namespace Kiwbi.Domain.Exceptions;

/// <summary>Base exception for invariant violations raised by domain entities.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
