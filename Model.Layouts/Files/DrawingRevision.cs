using Tellurian.Trains.Schedules.Model.Layouts.Drawings;

namespace Tellurian.Trains.Schedules.Model.Files;

/// <summary>An entry in a drawing's revision log: who changed the drawing, and the last date they saved it.</summary>
public sealed record DrawingRevision : ExtensibleRecord
{
    /// <summary>The user name from the settings of the person who saved the drawing.</summary>
    public required string Name { get; init; }

    /// <summary>The last date this person saved the drawing.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// <paramref name="log"/> with <paramref name="name"/> saving on <paramref name="date"/>: one entry per person,
    /// newest first, the person's earlier entry replaced. A blank name leaves the log as it is. Names are compared
    /// ignoring case and surrounding white space.
    /// </summary>
    public static IReadOnlyList<DrawingRevision> Record(IEnumerable<DrawingRevision> log, string? name, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(log);
        var entries = log.ToList();
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0) return entries;
        return
        [
            new DrawingRevision { Name = trimmed, Date = date },
            .. entries.Where(r => !string.Equals(r.Name.Trim(), trimmed, StringComparison.OrdinalIgnoreCase)).OrderByDescending(r => r.Date),
        ];
    }
}
