using System.Diagnostics.CodeAnalysis;

namespace Application.Core;

public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static Result Combine(params Result[] results)
    {
        if (results.Length == 0)
            return Success();

        var failures = results.Where(r => r.IsFailure).ToArray();
        
        if (failures.Length == 0)
            return Success();

        if (failures.Length == 1)
            return failures[0];

        var errors = failures.Select(f => f.Error).ToArray();
        return Failure(new ValidationError(errors));
    }

    public static Result Combine(IEnumerable<Result> results) => Combine(results.ToArray());

    public static Result<TValue> Combine<TValue>(TValue value, params Result[] results)
    {
        var combinedResult = Combine(results);
        return combinedResult.IsSuccess 
            ? Success(value) 
            : Failure<TValue>(combinedResult.Error);
    }

    public static Result<TValue> Combine<TValue>(TValue value, IEnumerable<Result> results) => 
        Combine(value, results.ToArray());
    
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) => value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static Result<TValue> ValidationFailure(Error error) => new(default, false, error);
}
