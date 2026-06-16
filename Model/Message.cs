using System.Globalization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a validation or information message with an associated severity level.
/// </summary>
public sealed record Message : IEquatable<Message>
{
    /// <summary>
    /// Creates an information message.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <returns>A new information message.</returns>
    public static Message Information(string text) => new(text, Severity.Information);

    /// <summary>
    /// Creates an information message with format arguments.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    /// <returns>A new information message.</returns>
    public static Message Information(string format, params object[] args) => new(string.Format(CultureInfo.CurrentCulture, format, args), Severity.Information);

    /// <summary>
    /// Creates a warning message.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <returns>A new warning message.</returns>
    public static Message Warning(string text) => new(text, Severity.Warning);

    /// <summary>
    /// Creates a warning message with format arguments.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    /// <returns>A new warning message.</returns>
    public static Message Warning(string format, params object[] args) => new(string.Format(CultureInfo.CurrentCulture, format, args), Severity.Warning);

    /// <summary>
    /// Creates an error message.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <returns>A new error message.</returns>
    public static Message Error(string text) => new(text, Severity.Error);

    /// <summary>
    /// Creates an error message with format arguments.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    /// <returns>A new error message.</returns>
    public static Message Error(string format, params object[] args) => new(string.Format(CultureInfo.CurrentCulture, format, args), Severity.Error);

    /// <summary>
    /// Creates a system message.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <returns>A new system message.</returns>
    public static Message System(string text) => new(text, Severity.System);

    /// <summary>
    /// Creates a copy message (no severity).
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <returns>A new message with no severity.</returns>
    public static Message Copy(string text) => new(text, Severity.None);

    /// <summary>
    /// Initializes a new instance of <see cref="Message"/> with the specified text and severity.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <param name="severity">The message severity.</param>
    public Message(string text, Severity severity) { Text = text; Severity = severity; }

    /// <summary>
    /// Gets the message text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the message severity.
    /// </summary>
    public Severity Severity { get; }

    /// <inheritdoc/>
    public override string ToString() =>
        Severity == Severity.None ? Text :
        $"{Severity.ToLanguageString(CultureInfo.CurrentCulture)}: {Text}";

    /// <inheritdoc/>
    public bool Equals(Message? other) =>
        Severity.Equals(other?.Severity) && Text.Equals(other?.Text, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Text, Severity);
}

/// <summary>
/// Provides extension methods for collections of <see cref="Message"/>.
/// </summary>
public static class ErrorMessageExtensions
{
    /// <summary>
    /// Determines whether the messages contain no stopping errors.
    /// </summary>
    /// <param name="me">The messages to check.</param>
    /// <returns><c>true</c> if there are no error or system messages; otherwise, <c>false</c>.</returns>
    public static bool HasNoStoppingErrors(this IEnumerable<Message> me) => !me.Any() || me.Any(m => m.Severity < Severity.Error);

    /// <summary>
    /// Determines whether the messages contain stopping errors.
    /// </summary>
    /// <param name="me">The messages to check.</param>
    /// <returns><c>true</c> if there are error or system messages; otherwise, <c>false</c>.</returns>
    public static bool HasStoppingErrors(this IEnumerable<Message> me) => me.Any(m => m.Severity >= Severity.Error);

    /// <summary>
    /// Determines whether any message contains the specified text.
    /// </summary>
    /// <param name="me">The messages to search.</param>
    /// <param name="text">The text to search for.</param>
    /// <returns><c>true</c> if a message contains the text; otherwise, <c>false</c>.</returns>
    public static bool Contains(this IEnumerable<Message> me, string text) => me.Any(m => m.Text.Contains(text, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Converts the messages to their string representations.
    /// </summary>
    /// <param name="me">The messages to convert.</param>
    /// <returns>A collection of message strings.</returns>
    public static IEnumerable<string> ToStrings(this IEnumerable<Message> me) =>
        me is null ? [] : me.Select(m => m.ToString());
}
