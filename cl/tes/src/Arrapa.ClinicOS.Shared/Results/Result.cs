using System.Diagnostics.CodeAnalysis;

namespace Arrapa.ClinicOS.Shared.Results;

/// <summary>
/// Non-generic result representing success or a single <see cref="Error"/>.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Results.Error.None)
        {
            throw new InvalidOperationException("A successful result cannot have an error.");
        }

        if (!isSuccess && error == Results.Error.None)
        {
            throw new InvalidOperationException("A failed result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Results.Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Result of a computation that returns <typeparamref name="TValue"/> on success.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error) => _value = value;

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<TValue> Success(TValue value) => new(value, true, Results.Error.None);
    public new static Result<TValue> Failure(Error error) => new(default, false, error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
