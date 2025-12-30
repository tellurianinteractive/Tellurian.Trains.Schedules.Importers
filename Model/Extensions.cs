using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Schedules.Model;

public static class StringExtensions
{
    extension([NotNullWhen(true)] string? text)
    {
        /// <summary>
        /// Returns the string value if it has content, otherwise throws ArgumentNullException with custom message.
        /// </summary>
        public string TextOrException(string parameterName, string? exceptionMessage = null) =>
            text.HasValue ? text : throw new ArgumentNullException(parameterName, exceptionMessage);
    }
}
