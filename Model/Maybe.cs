using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents an optional value that may or may not be present, with an associated error message if absent.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
public readonly struct Maybe<T> : IEquatable<Maybe<T>> where T : class
{
    /// <summary>
    /// Gets a Maybe instance representing no value.
    /// </summary>
    public static Maybe<T> None => new(Strings.NoValue);

    /// <summary>
    /// Creates a Maybe instance representing no value with a specific reason.
    /// </summary>
    /// <param name="reason">The reason for the absence of a value.</param>
    /// <returns>A Maybe instance with no value.</returns>
    public static Maybe<T> NoneWithReason(string reason) => new(reason);

    /// <summary>
    /// Initializes a new instance of <see cref="Maybe{T}"/> with a value.
    /// </summary>
    /// <param name="value">The value, or null if absent.</param>
    public Maybe(T? value) { _Value = value; Message = string.Empty; }

    /// <summary>
    /// Initializes a new instance of <see cref="Maybe{T}"/> with no value and an error message.
    /// </summary>
    /// <param name="message">The error message explaining the absence.</param>
    public Maybe(string message) { _Value = null; Message = message; }

    /// <summary>
    /// Initializes a new instance of <see cref="Maybe{T}"/> with no value and multiple error messages.
    /// </summary>
    /// <param name="messages">The error messages explaining the absence.</param>
    public Maybe(IEnumerable<string> messages) { _Value = null; Message = string.Join(", ", messages); }

    /// <summary>
    /// Initializes a new instance of <see cref="Maybe{T}"/> with no value and a formatted error message.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    public Maybe(string format, params object[] args) : this(string.Format(CultureInfo.InvariantCulture, format, args)) { }

    /// <summary>
    /// Initializes a new instance of <see cref="Maybe{T}"/> with a value and error message.
    /// </summary>
    /// <param name="value">The value, or null if absent.</param>
    /// <param name="message">The error message if value is null.</param>
    public Maybe(T? value, string message) { _Value = value; Message = message; }

    /// <summary>
    /// Initializes a new instance of <see cref="Maybe{T}"/> with a value and formatted error message.
    /// </summary>
    /// <param name="value">The value, or null if absent.</param>
    /// <param name="format">The format string for the error message.</param>
    /// <param name="args">The format arguments.</param>
    public Maybe(T? value, string format, params object[] args) : this(value, string.Format(CultureInfo.InvariantCulture, format, args)) { }

    /// <summary>
    /// Initializes a new instance of <see cref="Maybe{T}"/> from a collection, taking the first value.
    /// </summary>
    /// <param name="values">The collection of values.</param>
    /// <param name="message">The error message if no values exist.</param>
    public Maybe(IEnumerable<T>? values, string message) { _Value = values?.FirstOrDefault(); Message = _Value is null ? message : string.Empty; }

    private readonly T? _Value;

    /// <summary>
    /// Gets the value, throwing an exception if absent.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no value is present.</exception>
    public readonly T Value => _Value ?? throw new InvalidOperationException(Strings.NoValue);

    /// <summary>
    /// Gets a value indicating whether a value is present.
    /// </summary>
    public readonly bool HasValue => _Value != null;

    /// <summary>
    /// Gets a value indicating whether no value is present.
    /// </summary>
    public readonly bool IsNone => !HasValue;

    /// <summary>
    /// Gets the error message explaining why no value is present.
    /// </summary>
    public string Message { get; }

    /// <inheritdoc/>
    public readonly bool Equals(Maybe<T> other) => _Value?.Equals(other._Value) ?? false;

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) => (obj is Maybe<T> other && Equals(other)) || (obj is T instance && instance.Equals(_Value));

    /// <inheritdoc/>
    public override readonly int GetHashCode() => _Value?.GetHashCode() ?? Message.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <summary>Determines whether two Maybe instances are equal.</summary>
    public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);

    /// <summary>Determines whether two Maybe instances are not equal.</summary>
    public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);

    /// <inheritdoc/>
    public override string ToString() =>
        HasValue ? Value.ToString() ?? string.Empty : Message;
}
