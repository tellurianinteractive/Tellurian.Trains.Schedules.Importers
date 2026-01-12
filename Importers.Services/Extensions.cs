using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Schedules.Importers.Services;

/// <summary>
/// Provides extension methods for common collection operations.
/// </summary>
public static class Extensions
{
    extension<T>(List<T> list)
    {
        /// <summary>
        /// Attempts to find the first element in the list that matches the specified predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="result">When this method returns, contains the first matching element if found; otherwise, the default value for the type.</param>
        /// <returns><c>true</c> if a matching element was found; otherwise, <c>false</c>.</returns>
        public bool TryGetFirstValue(Func<T, bool> predicate, [NotNullWhen(true)] out T? result)
        {
            result = list.FirstOrDefault(item => predicate(item));
            return result is not null;
        }
    }
}
