using System;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Immutable discriminated union representing either success with value or failure with error.
/// Follows functional programming principles for null-safe error handling.
/// Implements CUPID properties: Predictable, Idiomatic, Domain-based.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
/// <typeparam name="E">The type of the error value</typeparam>
public sealed record Result<T, E>
{
    private readonly T? _value;
    private readonly E? _error;

    /// <summary>
    /// Gets whether this result represents a success state.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets whether this result represents an error state.
    /// </summary>
    public bool IsError => !IsSuccess;

    /// <summary>
    /// Gets the success value. Throws InvalidOperationException if called on error result.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on error Result");

    /// <summary>
    /// Gets the error value. Throws InvalidOperationException if called on success result.
    /// </summary>
    public E Error => IsError
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on success Result");

    private Result(T value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _error = default;
        IsSuccess = true;
    }

    private Result(E error)
    {
        _value = default;
        _error = error ?? throw new ArgumentNullException(nameof(error));
        IsSuccess = false;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    /// <param name="value">The success value</param>
    /// <returns>A successful result</returns>
    public static Result<T, E> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    /// <param name="error">The error value</param>
    /// <returns>A failed result</returns>
    public static Result<T, E> Failure(E error) => new(error);

    /// <summary>
    /// Maps the success value to a new type, leaving error unchanged.
    /// Implements functional composition for chaining operations.
    /// </summary>
    /// <typeparam name="U">The type to map to</typeparam>
    /// <param name="mapper">Function to transform the success value</param>
    /// <returns>A new result with transformed success value or original error</returns>
    public Result<U, E> Map<U>(Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return IsSuccess
            ? Result<U, E>.Success(mapper(Value))
            : Result<U, E>.Failure(Error);
    }

    /// <summary>
    /// Flat maps the success value to a new result, enabling chaining of operations that may fail.
    /// Implements monadic bind operation for Result type.
    /// </summary>
    /// <typeparam name="U">The type of the new success value</typeparam>
    /// <param name="mapper">Function that may produce another result</param>
    /// <returns>The result of the mapper or original error</returns>
    public Result<U, E> FlatMap<U>(Func<T, Result<U, E>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return IsSuccess ? mapper(Value) : Result<U, E>.Failure(Error);
    }

    /// <summary>
    /// Maps the error value to a new type, leaving success unchanged.
    /// </summary>
    /// <typeparam name="F">The type to map error to</typeparam>
    /// <param name="mapper">Function to transform the error value</param>
    /// <returns>A new result with transformed error or original success value</returns>
    public Result<T, F> MapError<F>(Func<E, F> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return IsError
            ? Result<T, F>.Failure(mapper(Error))
            : Result<T, F>.Success(Value);
    }

    /// <summary>
    /// Executes one of two functions based on the result state.
    /// Provides safe pattern matching for result handling.
    /// </summary>
    /// <typeparam name="U">The return type</typeparam>
    /// <param name="onSuccess">Function to execute on success</param>
    /// <param name="onError">Function to execute on error</param>
    /// <returns>The result of the appropriate function</returns>
    public U Match<U>(Func<T, U> onSuccess, Func<E, U> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        return IsSuccess ? onSuccess(Value) : onError(Error);
    }

    /// <summary>
    /// Returns the success value or a default value if this is an error result.
    /// </summary>
    /// <param name="defaultValue">Value to return on error</param>
    /// <returns>Success value or default value</returns>
    public T GetValueOrDefault(T defaultValue) => IsSuccess ? Value : defaultValue;

    /// <summary>
    /// Returns the success value or the result of calling the provided function on error.
    /// </summary>
    /// <param name="defaultValueProvider">Function to provide default value</param>
    /// <returns>Success value or default value from function</returns>
    public T GetValueOrDefault(Func<E, T> defaultValueProvider)
    {
        ArgumentNullException.ThrowIfNull(defaultValueProvider);
        return IsSuccess ? Value : defaultValueProvider(Error);
    }

    /// <summary>
    /// Implicit conversion from success value to Result.
    /// </summary>
    public static implicit operator Result<T, E>(T value) => Success(value);

    public override string ToString() =>
        IsSuccess ? $"Success({Value})" : $"Failure({Error})";
}
