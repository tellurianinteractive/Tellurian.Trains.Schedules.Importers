using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Provides general-purpose extension methods for strings.
/// </summary>
public static class StringExtensions
{
    extension([NotNullWhen(true)] string? text)
    {
        /// <summary>
        /// Returns the string value if it has content, otherwise throws ArgumentNullException with custom message.
        /// </summary>
        /// <param name="parameterName">The name of the parameter for the exception.</param>
        /// <param name="exceptionMessage">An optional custom exception message.</param>
        /// <returns>The string value if it has content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the string is null or empty.</exception>
        public string TextOrException(string parameterName, string? exceptionMessage = null) =>
            text.HasValue ? text : throw new ArgumentNullException(parameterName, exceptionMessage);


    }


}
