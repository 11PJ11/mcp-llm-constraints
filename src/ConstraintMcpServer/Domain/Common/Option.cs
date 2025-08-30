using System;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Immutable discriminated union representing optional values without null references.
/// Follows functional programming principles for null-safe value handling.
/// Implements CUPID properties: Predictable, Idiomatic, Domain-based.
/// </summary>
/// <typeparam name="T">The type of the optional value</typeparam>
public sealed record Option<T>
{
    private readonly T? _value;

    /// <summary>
    /// Gets whether this option contains a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets whether this option is empty (contains no value).
    /// </summary>
    public bool IsNone => !HasValue;

    /// <summary>
    /// Gets the contained value. Throws InvalidOperationException if called on empty option.
    /// </summary>
    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on empty Option");

    private Option(T value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        HasValue = true;
    }

    private Option()
    {
        _value = default;
        HasValue = false;
    }

    /// <summary>
    /// Creates an option containing the given value.
    /// </summary>
    /// <param name="value">The value to wrap</param>
    /// <returns>An option containing the value</returns>
    public static Option<T> Some(T value) => new(value);

    /// <summary>
    /// Creates an empty option.
    /// </summary>
    /// <returns>An empty option</returns>
    public static Option<T> None() => new();

    /// <summary>
    /// Creates an option from a potentially null value.
    /// Returns None if value is null, Some(value) otherwise.
    /// </summary>
    /// <param name="value">The potentially null value</param>
    /// <returns>Some(value) if value is not null, None otherwise</returns>
    public static Option<T> FromNullable(T? value) =>
        value is not null ? Some(value) : None();

    /// <summary>
    /// Maps the contained value to a new type, leaving empty option unchanged.
    /// Implements functor mapping for Option type.
    /// </summary>
    /// <typeparam name="U">The type to map to</typeparam>
    /// <param name="mapper">Function to transform the contained value</param>
    /// <returns>A new option with transformed value or empty option</returns>
    public Option<U> Map<U>(Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return HasValue
            ? Option<U>.Some(mapper(Value))
            : Option<U>.None();
    }

    /// <summary>
    /// Flat maps the contained value to a new option, enabling chaining of optional operations.
    /// Implements monadic bind operation for Option type.
    /// </summary>
    /// <typeparam name="U">The type of the new optional value</typeparam>
    /// <param name="mapper">Function that may produce another option</param>
    /// <returns>The result of the mapper or empty option</returns>
    public Option<U> FlatMap<U>(Func<T, Option<U>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return HasValue ? mapper(Value) : Option<U>.None();
    }

    /// <summary>
    /// Filters the option based on a predicate.
    /// Returns the option if it has value and predicate returns true, None otherwise.
    /// </summary>
    /// <param name="predicate">Predicate to test the value</param>
    /// <returns>The option if predicate passes, None otherwise</returns>
    public Option<T> Filter(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return HasValue && predicate(Value) ? this : None();
    }

    /// <summary>
    /// Executes one of two functions based on whether the option has a value.
    /// Provides safe pattern matching for option handling.
    /// </summary>
    /// <typeparam name="U">The return type</typeparam>
    /// <param name="onSome">Function to execute when option has value</param>
    /// <param name="onNone">Function to execute when option is empty</param>
    /// <returns>The result of the appropriate function</returns>
    public U Match<U>(Func<T, U> onSome, Func<U> onNone)
    {
        ArgumentNullException.ThrowIfNull(onSome);
        ArgumentNullException.ThrowIfNull(onNone);

        return HasValue ? onSome(Value) : onNone();
    }

    /// <summary>
    /// Executes an action if the option has a value.
    /// Provides safe side-effect execution for optional values.
    /// </summary>
    /// <param name="action">Action to execute on the value</param>
    public void IfPresent(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (HasValue)
        {
            action(Value);
        }
    }

    /// <summary>
    /// Returns the contained value or a default value if the option is empty.
    /// </summary>
    /// <param name="defaultValue">Value to return if option is empty</param>
    /// <returns>Contained value or default value</returns>
    public T GetValueOrDefault(T defaultValue) => HasValue ? Value : defaultValue;

    /// <summary>
    /// Returns the contained value or the result of calling the provided function.
    /// </summary>
    /// <param name="defaultValueProvider">Function to provide default value</param>
    /// <returns>Contained value or default value from function</returns>
    public T GetValueOrDefault(Func<T> defaultValueProvider)
    {
        ArgumentNullException.ThrowIfNull(defaultValueProvider);
        return HasValue ? Value : defaultValueProvider();
    }

    /// <summary>
    /// Converts the option to a nullable value.
    /// Returns the contained value or null if the option is empty.
    /// </summary>
    /// <returns>The value or null</returns>
    public T? ToNullable() => HasValue ? Value : default;

    /// <summary>
    /// Implicit conversion from value to Option.
    /// </summary>
    public static implicit operator Option<T>(T value) => Some(value);

    public override string ToString() =>
        HasValue ? $"Some({Value})" : "None";
}
