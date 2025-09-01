using System;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// Follows the Result pattern for functional error handling.
/// </summary>
/// <typeparam name="TSuccess">The type of the success value</typeparam>
/// <typeparam name="TError">The type of the error value</typeparam>
public sealed record Result<TSuccess, TError>
{
    private readonly TSuccess? _success;
    private readonly TError? _error;

    /// <summary>
    /// Whether this result represents a successful operation.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Whether this result represents a failed operation.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the success value (only valid when IsSuccess is true).
    /// </summary>
    public TSuccess Value => IsSuccess
        ? _success!
        : throw new InvalidOperationException("Cannot access Value on a failed result");

    /// <summary>
    /// Gets the error value (only valid when IsFailure is true).
    /// </summary>
    public TError Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful result");

    private Result(TSuccess? success, TError? error, bool isSuccess)
    {
        _success = success;
        _error = error;
        IsSuccess = isSuccess;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    public static Result<TSuccess, TError> Success(TSuccess value) =>
        new(value, default, true);

    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    public static Result<TSuccess, TError> Failure(TError error) =>
        new(default, error, false);

    /// <summary>
    /// Transforms the success value if this result is successful.
    /// </summary>
    public Result<TNewSuccess, TError> Map<TNewSuccess>(Func<TSuccess, TNewSuccess> mapper)
    {
        if (mapper == null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }

        return IsSuccess
            ? Result<TNewSuccess, TError>.Success(mapper(Value))
            : Result<TNewSuccess, TError>.Failure(Error);
    }

    /// <summary>
    /// Transforms the error value if this result is a failure.
    /// </summary>
    public Result<TSuccess, TNewError> MapError<TNewError>(Func<TError, TNewError> mapper)
    {
        if (mapper == null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }

        return IsFailure
            ? Result<TSuccess, TNewError>.Failure(mapper(Error))
            : Result<TSuccess, TNewError>.Success(Value);
    }

    /// <summary>
    /// Chains operations that return Results, only proceeding if this result is successful.
    /// </summary>
    public Result<TNewSuccess, TError> Bind<TNewSuccess>(Func<TSuccess, Result<TNewSuccess, TError>> binder)
    {
        if (binder == null)
        {
            throw new ArgumentNullException(nameof(binder));
        }

        return IsSuccess
            ? binder(Value)
            : Result<TNewSuccess, TError>.Failure(Error);
    }

    /// <summary>
    /// Executes an action if the result is successful, returning this result unchanged.
    /// </summary>
    public Result<TSuccess, TError> OnSuccess(Action<TSuccess> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (IsSuccess)
        {
            action(Value);
        }

        return this;
    }

    /// <summary>
    /// Executes an action if the result is a failure, returning this result unchanged.
    /// </summary>
    public Result<TSuccess, TError> OnFailure(Action<TError> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (IsFailure)
        {
            action(Error);
        }

        return this;
    }

    /// <summary>
    /// Returns the success value if successful, otherwise returns the provided default value.
    /// </summary>
    public TSuccess ValueOr(TSuccess defaultValue) =>
        IsSuccess ? Value : defaultValue;

    /// <summary>
    /// Returns the success value if successful, otherwise computes and returns a default value.
    /// </summary>
    public TSuccess ValueOr(Func<TError, TSuccess> defaultValueProvider)
    {
        if (defaultValueProvider == null)
        {
            throw new ArgumentNullException(nameof(defaultValueProvider));
        }

        return IsSuccess ? Value : defaultValueProvider(Error);
    }

    /// <summary>
    /// String representation for debugging and logging.
    /// </summary>
    public override string ToString()
    {
        return IsSuccess
            ? $"Success({Value})"
            : $"Failure({Error})";
    }
}

/// <summary>
/// Static helper methods for creating Results.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    public static Result<TSuccess, TError> Success<TSuccess, TError>(TSuccess value) =>
        Result<TSuccess, TError>.Success(value);

    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    public static Result<TSuccess, TError> Failure<TSuccess, TError>(TError error) =>
        Result<TSuccess, TError>.Failure(error);
}
