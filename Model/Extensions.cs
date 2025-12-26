using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Tellurian.Trains.Schedules.Model;

public static class StringExtensions
{
    public static bool HasText([NotNullWhen(true)] this string? text) => !string.IsNullOrWhiteSpace(text);

    extension(string? textOrNullOrWhiteSpace)
    {
        public string TextOrEmpty() =>
            textOrNullOrWhiteSpace.TextOrDefault(string.Empty);

        public string TextOrDefault(string defaultText) =>
            HasText(textOrNullOrWhiteSpace) ? textOrNullOrWhiteSpace : defaultText;

        public string TextOrException(string parameterName, string? exceptionMessage = null) =>
            HasText(textOrNullOrWhiteSpace) ? textOrNullOrWhiteSpace : throw new ArgumentNullException(parameterName, exceptionMessage);

        public bool EqualsCaseInsensitive(string? text) =>
            textOrNullOrWhiteSpace is not null && text is not null && textOrNullOrWhiteSpace.Equals(text, StringComparison.OrdinalIgnoreCase);

        public string UntilOrEmpty(char[] stopAt)
        {
            if (stopAt.Length > 0 && textOrNullOrWhiteSpace.HasText())
            {
                int endIndex = textOrNullOrWhiteSpace.IndexOf(stopAt, StringComparison.Ordinal);
                if (endIndex > 0)
                {
                    return textOrNullOrWhiteSpace[..endIndex];
                }
                else if (endIndex == -1)
                {
                    return textOrNullOrWhiteSpace;
                }
            }
            return string.Empty;
        }
    }
}
public static class ObjectExtensions
{
    public static bool HasValue([NotNullWhen(true)] this object? value) => value is not null;
    public static bool HasValueExcept([NotNullWhen(true)] this object? value, object except) => value is not null && !value.Equals(except);
    public static T ValueOrException<T>([AllowNull] this T? value, string parameterName, string? nullMessage = null) where T : class =>
        value ?? throw new ArgumentNullException(parameterName, nullMessage);

    public static void IfNotEqualsThrow<T>(this T one, T other, string notEqualsMessage)
    {
        if (one is null || !one.Equals(other)) throw new ArgumentOutOfRangeException(notEqualsMessage);
    }
}
public static class BoolExtensions
{
    public static void IfTrueThrows(this bool condition, string parameterName, string? exceptionMessage = null)
    {
        if (condition) throw new ArgumentOutOfRangeException(parameterName, exceptionMessage);
    }
}

public static class TimeSpanExtensions
{
    extension(TimeSpan value)
    {
        public string HHMM => string.Format(CultureInfo.InvariantCulture, "{0:hh\\:mm}", value);
    }
}
