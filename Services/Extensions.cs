using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Schedules.Importers.Services;

public static class Extensions
{
    extension<T>(List<T> list)
    {
        public bool TryGetFirstValue(Func<T, bool> predicate, [NotNullWhen(true)] out T? result)
        {
            result = list.FirstOrDefault(item => predicate(item));
            return result is not null;
        }
    }
}
