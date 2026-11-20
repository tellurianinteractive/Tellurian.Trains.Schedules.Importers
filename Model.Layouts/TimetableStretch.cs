using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>A route through the layout, of track stretches in order, shown as one graphical timetable.</summary>
public sealed class TimetableStretch : IEquatable<TimetableStretch>
{
    /// <summary>For JSON and EF Core.</summary>
    [JsonConstructor]
    private TimetableStretch()
    {
        Number = string.Empty;
        Description = string.Empty;
        Stretches = [];
    }

    /// <summary>Creates a timetable stretch.</summary>
    /// <exception cref="ArgumentException">The number is empty.</exception>
    public TimetableStretch(int id, string? number)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        Id = id;
        Number = number;
        Description = string.Empty;
        Stretches = [];
    }

    /// <summary>Creates a timetable stretch with a description.</summary>
    public TimetableStretch(int id, string? number, string description) : this(id, number)
    {
        Description = description;
    }

    /// <summary>The identity of the timetable stretch within the layout.</summary>
    public int Id { get; set; }

    /// <summary>The number the timetable stretch is known by, e.g. "1".</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>What the route is, e.g. "Main line".</summary>
    public string Description { get; set; }

    /// <summary>The colour of the route in schematics, as a CSS hex colour.</summary>
    public string Color { get; set; } = "#000000";

    /// <summary>The track stretches of the route, in order. Written by their ids, <see cref="StretchIds"/>.</summary>
    [JsonIgnore]
    public ICollection<TrackStretch> Stretches
    {
        get;
        set { field = value; _stretchIds = null; }
    }

    /// <summary>
    /// The ids of <see cref="Stretches"/>, in order, the form they are written in; the ids read until the stretches are
    /// found. Null, and so not written, when there are none.
    /// </summary>
    public IList<int>? StretchIds
    {
        get => Stretches is { Count: > 0 } ? [.. Stretches.Select(s => s.Id)] : _stretchIds;
        set => _stretchIds = value;
    }
    private IList<int>? _stretchIds;

    /// <summary>The locations of the route, in order.</summary>
    [JsonIgnore]
    public IEnumerable<OperationLocation> Stations =>
        Stretches.Count == 0 ? [] : Stretches.Select(s => s.Start).Concat([Stretches.Last().End]);

    /// <inheritdoc/>
    public bool Equals(TimetableStretch? other) => other != null && Number.Equals(other.Number, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TimetableStretch other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Number.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <summary>The number, the description and, when it has stretches, where the route starts and ends.</summary>
    public override string ToString() => Stretches.Count == 0
        ? $"{Number}: {Description}".Trim()
        : $"{Number}: {Description} {Stretches.First().Start}-{Stretches.Last().End}".Trim();
}
