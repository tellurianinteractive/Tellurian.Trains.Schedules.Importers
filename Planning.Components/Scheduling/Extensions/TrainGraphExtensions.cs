using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

internal static class TrainGraphExtensions
{
    public static IEnumerable<StretchUse> StretchUses(this Train train)
    {
        for (var callIndex = 0; callIndex < train.Calls.Count - 1; callIndex++)
        {
            yield return new StretchUse(train, callIndex);
        }
    }

    /// <summary>Formats the train identity label for display on the graph according to the given settings.
    /// Always includes the train number; optionally prepends the company signature and wraps with the
    /// category prefix/suffix when the corresponding settings are enabled.</summary>
    public static string GraphLabel(this Train train, GraphSettings settings)
    {
        var parts = new List<string>();
        if (settings.ShowCompany && (train.Company ?? train.Category?.Company)?.Signature is { Length: > 0 } sig)
            parts.Add(sig);
        if (settings.ShowTrainCategory && train.Category?.Prefix is { Length: > 0 } prefix)
            parts.Add(prefix);
        parts.Add(train.Number.ToString());
        if (settings.ShowTrainCategory && train.Category?.Suffix is { Length: > 0 } suffix)
            parts.Add(suffix);
        return string.Join(" ", parts);
    }
}
