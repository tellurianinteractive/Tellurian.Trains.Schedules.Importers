using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Represents a timetable containing a collection of trains operating on a specific layout.
/// </summary>
/// <remarks>
/// A timetable defines all scheduled trains for a particular track layout during a specific operating period.
/// </remarks>
public sealed class Timetable : IEquatable<Timetable>, IJsonOnDeserialized
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

    /// <summary>
    /// Gets or sets the catalogue of train categories available in this timetable. A <see cref="Train"/>'s
    /// <see cref="Train.Category"/> references an entry here. Seeded with default Passenger and Freight
    /// categories for a new timetable (see <c>TrainCategory.DefaultsFor</c>).
    /// </summary>
    public ICollection<TrainCategory> TrainCategories { get; set; }

    /// <summary>
    /// Gets or sets the catalogue of session (or day) patterns available in this timetable. A
    /// <see cref="Train"/>'s <see cref="Train.Sessions"/> is chosen from this catalogue. Seeded with
    /// the standard patterns for a new timetable (see <see cref="CommonSessionPatterns.DefaultCatalogue"/>);
    /// the user adds custom patterns (e.g. <c>1-3</c>, <c>4-7</c>) from the settings.
    /// </summary>
    public IList<Sessions> Sessions { get; set; }

    /// <summary>
    /// Gets or sets the catalogue of reusable cargo flow descriptions available in this timetable. A
    /// cargo flow references an entry here (editing the entry updates every cargo flow that uses it).
    /// Managed on the Cargo Flow tab; empty for a new timetable until the planner adds descriptions.
    /// </summary>
    public IList<CargoFlowOptions> CargoFlowOptions { get; set; }

    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private Timetable()
    {
        Layout = default!;
        Trains = [];
        TrainCategories = [];
        Sessions = [];
        CargoFlowOptions = [];
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
        TrainCategories = [];
        Sessions = [];
        CargoFlowOptions = [];
    }

    /// <summary>
    /// Rebuilds the per-track call index as soon as a timetable has been read, so a plan is never
    /// looked at through a stale one.
    /// </summary>
    /// <remarks>
    /// <see cref="Layouts.StationTrack.Calls"/> is derived from <see cref="Train.Calls"/> and is no
    /// longer written to the document (see <c>PlanJson</c>), so it has to be built after every read —
    /// and a document written by an earlier version may carry an index holding calls of trains that
    /// are no longer in the timetable. Doing it here means no reading path can forget.
    /// </remarks>
    void IJsonOnDeserialized.OnDeserialized() => this.RebuildStationCalls();

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
    /// Ensures the timetable has a session catalogue to choose from: when <see cref="Timetable.Sessions"/>
    /// is empty (an older or imported timetable created before the catalogue existed), it is seeded with
    /// the standard patterns (see <see cref="CommonSessionPatterns.DefaultCatalogue"/>). Idempotent while
    /// the catalogue is non-empty. Returns <c>true</c> when patterns were added.
    /// </summary>
    /// <param name="timetable">The timetable whose catalogue to ensure.</param>
    public static bool EnsureSessions(this Timetable timetable)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        if (timetable.Sessions.Count > 0) return false;
        foreach (var sessions in CommonSessionPatterns.DefaultCatalogue) timetable.Sessions.Add(sessions);
        return true;
    }

    /// <summary>
    /// Rebuilds the per-track call index (<see cref="StationTrack.Calls"/>) from the single source of
    /// truth, <see cref="Train.Calls"/>: every track's call collection is cleared and then repopulated
    /// from the calls of the timetable's trains. A call whose train is not in <see cref="Timetable.Trains"/>
    /// — an orphan left by an earlier import or a persisted plan — is thereby dropped and can never surface
    /// as a phantom conflict on a track or stretch. Call this whenever a plan is loaded or restored, before
    /// it is validated or displayed.
    /// </summary>
    /// <param name="timetable">The timetable whose per-track call index to rebuild.</param>
    public static void RebuildStationCalls(this Timetable timetable)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        foreach (var track in timetable.Layout.OperationLocations.SelectMany(l => l.Tracks)) track.Calls.Clear();
        foreach (var call in timetable.Trains.SelectMany(t => t.Calls)) call.Track.Add(call);
    }

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

    /// <summary>
    /// Adds a cargo flow description to the timetable's catalogue, assigning the next free id when it
    /// has none. A description already present is left untouched.
    /// </summary>
    /// <param name="timetable">The timetable whose catalogue to add to.</param>
    /// <param name="options">The cargo flow description to add.</param>
    /// <returns>The added (or already present) cargo flow description.</returns>
    public static CargoFlowOptions Add(this Timetable timetable, CargoFlowOptions options)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        options = options.ValueOrException(nameof(options));
        if (!timetable.CargoFlowOptions.Contains(options))
        {
            if (options.Id == 0) options.Id = timetable.NextCargoFlowOptionsId();
            timetable.CargoFlowOptions.Add(options);
        }
        return options;
    }

    /// <summary>
    /// Gets the next free cargo flow description id for this timetable's catalogue.
    /// </summary>
    /// <param name="timetable">The timetable.</param>
    public static int NextCargoFlowOptionsId(this Timetable timetable) =>
        (timetable.CargoFlowOptions.Select(o => o.Id).DefaultIfEmpty(0).Max()) + 1;

    /// <summary>
    /// Gets the next free train number for a train of <paramref name="category"/> travelling in the given
    /// direction: the lowest number at or above the category's <see cref="TrainCategory.StartNumber"/> that
    /// has the required parity (odd when <paramref name="odd"/>, even otherwise) and is not already used by
    /// another train of the same category. By convention trains running downwards are numbered odd and those
    /// running upwards even, so opposing directions never share a number and every category keeps its own band.
    /// </summary>
    /// <param name="timetable">The timetable whose trains define which numbers are taken.</param>
    /// <param name="category">The category whose start number and existing trains bound the search.</param>
    /// <param name="odd"><c>true</c> for an odd number (downwards), <c>false</c> for an even number (upwards).</param>
    /// <returns>The next free number of the requested parity.</returns>
    public static int NextTrainNumber(this Timetable timetable, TrainCategory category, bool odd)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        ArgumentNullException.ThrowIfNull(category);
        var used = timetable.Trains
            .Where(t => t.CategoryId == category.Id)
            .Select(t => t.Number)
            .ToHashSet();
        var number = Math.Max(category.StartNumber, 1);
        if (number % 2 == 0 == odd) number++; // step up one to reach the required parity
        while (used.Contains(number)) number += 2;
        return number;
    }
}
