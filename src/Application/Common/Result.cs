using System.Diagnostics.CodeAnalysis;
using CleanCqrs.Domain.Common;

namespace CleanCqrs.Application.Common;

/// <summary>
/// Lets generic code (such as pipeline behaviors) create a failed result of a concrete type
/// without reflection.
/// </summary>
/// <typeparam name="TSelf">The implementing result type.</typeparam>
public interface IFailureFactory<TSelf>
    where TSelf : IFailureFactory<TSelf>
{
    /// <summary>Creates a failed result carrying <paramref name="error"/>.</summary>
    static abstract TSelf Failure(Error error);
}

/// <summary>
/// Outcome of an operation that returns no value.
/// </summary>
public class Result : IFailureFactory<Result>
{
    /// <summary>Initializes a result, enforcing that success and error are consistent.</summary>
    /// <exception cref="ArgumentException">The combination of arguments is invalid.</exception>
    protected Result(bool isSuccess, Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (isSuccess != (error == Error.None))
        {
            throw new ArgumentException("A successful result must have no error and a failure must have one.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>True when the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>True when the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The failure reason, or <see cref="Error.None"/> on success.</summary>
    public Error Error { get; }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result.</summary>
    public static Result Failure(Error error) => new(false, error);
}

/// <summary>
/// Outcome of an operation that returns a <typeparamref name="TValue"/> on success.
/// </summary>
/// <typeparam name="TValue">Type of the success value.</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
    Justification = "Static factories are the point of the Result pattern and are required by IFailureFactory.")]
public sealed class Result<TValue> : Result, IFailureFactory<Result<TValue>>
{
    private readonly TValue? _value;

    private Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error) => _value = value;

    /// <summary>The success value.</summary>
    /// <exception cref="InvalidOperationException">The result is a failure.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    /// <summary>Creates a successful result holding <paramref name="value"/>.</summary>
    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);

    /// <summary>Creates a failed result.</summary>
    public static new Result<TValue> Failure(Error error) => new(default, false, error);

    /// <summary>Wraps a value in a successful result.</summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>Wraps an error in a failed result.</summary>
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
