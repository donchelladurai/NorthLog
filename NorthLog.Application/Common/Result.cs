namespace NorthLog.Application.Common;

/* To the interviewers:
     A minimal Result<T> for the application layer. Handlers return Result<T>, implementing the Result pattern. 
     instead of throwing for expected outcomes (not found, conflict, etc.). 

    The reasoning comes from a common anti-pattern in my experience: using exceptions for control flow. 

    Also because we discussed Generics, introduced in .NET 2, during the initial interview and I wanted to showcase a real world example.
*/

public sealed class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(string error)
    {
        Error = error;
        IsSuccess = false;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
}