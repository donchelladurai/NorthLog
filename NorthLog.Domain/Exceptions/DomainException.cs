namespace NorthLog.Domain.Exceptions;

public class DomainException(string message) : Exception(message)
{
}