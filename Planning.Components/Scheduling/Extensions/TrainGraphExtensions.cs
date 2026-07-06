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
    /// category prefix/suffix when the corresponding settings are enabled. Unless suppressed by
    /// <see cref="GraphSettings.HideSessionsOrDays"/>, the operating sessions/days are appended at the end
    /// (see <see cref="GraphSessionsLabel"/>).</summary>
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
        if (train.GraphSessionsLabel(settings) is { } sessions)
            parts.Add(sessions);
        return string.Join(" ", parts);
    }

    /// <summary>Formats the train's operating sessions/days for the graph label, or <c>null</c> when nothing
    /// should be shown. Suppressed entirely when <see cref="GraphSettings.HideSessionsOrDays"/> is set, and
    /// for trains that run every session/day (a full pattern carries no information). When
    /// <see cref="GraphSettings.UseDays"/> is set the pattern is shown as short weekday names with
    /// consecutive runs collapsed (e.g. <c>Mo-Fr</c>); otherwise as session numbers (e.g. <c>1-5</c>,
    /// <c>1 3 5</c>).</summary>
    public static string? GraphSessionsLabel(this Train train, GraphSettings settings)
    {
        if (settings.HideSessionsOrDays) return null;
        // Ignore session bits beyond the layout's operating period, so a full pattern within that period is
        // recognised as "every session/day" and suppressed.
        var sessions = train.Sessions.CappedForDisplay(settings.UseDays, settings.MaxSessions);
        if (settings.UseDays)
        {
            var dayCount = sessions.DayCount;
            return dayCount == 0 || dayCount == Math.Min(settings.MaxSessions, 7) ? null : sessions.DaysShort(settings.StartDay);
        }
        var numberCount = sessions.Numbers.Length;
        return numberCount == 0 || numberCount == settings.MaxSessions ? null : sessions.SessionsNumbers;
    }
}
