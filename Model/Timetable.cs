using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a timetable containing a collection of trains operating on a specific layout.
/// </summary>
/// <remarks>
/// A timetable defines all scheduled trains for a particular track layout during a specific operating period.
/// </remarks>
public sealed class Timetable : IEquatable<Timetable>
{
    /// <summary>
    /// Gets or sets the foreign key to the associated layout.
    /// </summary>
    public int LayoutId { get; set; }

    /// <summary>
    /// Gets or sets the layout on which this timetable operates.
    /// </summary>
    public Layout Layout { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this timetable.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this timetable.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of trains in this timetable.
    /// </summary>
    public ICollection<Train> Trains { get; set; }

    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private Timetable()
    {
        Layout = default!;
        Trains = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Timetable"/> with the specified name and layout.
    /// </summary>
    /// <param name="name">The name of the timetable.</param>
    /// <param name="layout">The layout on which this timetable operates.</param>
    public Timetable(string name, Layout layout)
    {
        Name = name;
        Layout = layout;
        LayoutId = layout.Id;
        Trains = [];
    }

    /// <inheritdoc/>
    public bool Equals(Timetable? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Timetable other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>
/// Provides extension methods for <see cref="Timetable"/>.
/// </summary>
public static class TimetableExtensions
{
    /// <summary>
    /// Gets the earliest hour from all train arrivals in the timetable.
    /// </summary>
    /// <param name="me">The timetable.</param>
    /// <returns>The start hour of the timetable, or 0 if no trains exist.</returns>
    public static int StartHour(this Timetable me) =>
        (me?.Trains.Select(t => t.Calls.Min(c => c.Arrival)).Min(tt => tt).Hours()) ?? 0;

    /// <summary>
    /// Gets the latest hour from all train arrivals in the timetable, plus one.
    /// </summary>
    /// <param name="me">The timetable.</param>
    /// <returns>The end hour of the timetable, or 24 if no trains exist.</returns>
    public static int EndHour(this Timetable me) =>
        (me?.Trains.Select(t => t.Calls.Max(c => c.Arrival)).Max(tt => tt).Hours() + 1) ?? 24;

    /// <summary>
    /// Computes the operating time window for the timetable: the earliest arrival floored to the hour
    /// (the first driver's start of service) through the latest departure ceiled to the next whole hour
    /// (the last driver's end of service). Returns 06:00–20:00 when the timetable has no calls, and the
    /// full day 00:00–24:00 when any call runs to or past midnight (so after-midnight times can wrap).
    /// </summary>
    /// <param name="me">The timetable.</param>
    /// <returns>The (start, end) operating window.</returns>
    public static (TimeSpan Start, TimeSpan End) OperatingWindow(this Timetable me)
    {
        var calls = me?.Trains.SelectMany(t => t.Calls).ToList() ?? [];
        if (calls.Count == 0) return (TimeSpan.FromHours(6), TimeSpan.FromHours(20));
        // A train may start before midnight and continue past it. When any call runs to or past 24:00,
        // the operating window spans the whole day so the after-midnight part can wrap to the start.
        var oneDay = TimeSpan.FromHours(24);
        if (calls.Any(c => c.Arrival.Value >= oneDay || c.Departure.Value >= oneDay)) return (TimeSpan.Zero, oneDay);
        var min = calls.Min(c => c.Arrival.Value);
        var max = calls.Max(c => c.Departure.Value);
        var start = new TimeSpan(min.Days, min.Hours, 0, 0);
        var end = max.Minutes == 0 && max.Seconds == 0
            ? new TimeSpan(max.Days, max.Hours, 0, 0)
            : new TimeSpan(max.Days, max.Hours + 1, 0, 0);
        return (start, end);
    }

    /// <summary>
    /// Gets all stations from the timetable's layout.
    /// </summary>
    /// <param name="me">The timetable.</param>
    /// <returns>A collection of operation locations (stations).</returns>
    public static IEnumerable<OperationLocation> Stations(this Timetable me) =>
        me is null ? Array.Empty<OperationLocation>() : me.Layout.OperationLocations;

    /// <summary>
    /// Finds a train by its external identifier.
    /// </summary>
    /// <param name="me">The timetable.</param>
    /// <param name="externalId">The external identifier to search for.</param>
    /// <returns>A <see cref="Maybe{T}"/> containing the train if found.</returns>
    public static Maybe<Train> Train(this Timetable me, string externalId) =>
        new(me?.Trains.Where(t => t.ExternalId == externalId), $"Train with external id '{externalId}' not found.");

    /// <summary>
    /// Adds a train to the timetable.
    /// </summary>
    /// <param name="timetable">The timetable to add the train to.</param>
    /// <param name="train">The train to add.</param>
    /// <returns>The added train.</returns>
    public static Train Add(this Timetable timetable, Train train)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        train = train.ValueOrException(nameof(train));
        if (!timetable.Trains.Contains(train))
        {
            train.Timetable = timetable;
            train.TimetableId = timetable.Id;
            timetable.Trains.Add(train);
        }
        return train;
    }
}
