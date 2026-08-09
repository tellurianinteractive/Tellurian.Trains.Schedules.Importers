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
    /// Gets or sets the catalogue of train categories available in this timetable. A <see cref="Train"/>'s
    /// <see cref="Train.Category"/> references an entry here. Seeded with default Passenger and Freight
    /// categories for a new timetable (see <c>TrainCategory.DefaultsFor</c>), and reconciled with the
    /// categories the trains actually use whenever a plan is read (see
    /// <see cref="TimetableExtensions.RebuildTrainCategories"/>).
    /// </summary>
    /// <remarks>
    /// Declared before <see cref="Trains"/> so that it is also written before them: a category is
    /// written where it is first met, and this is the one place it belongs.
    /// </remarks>
    public ICollection<TrainCategory> TrainCategories { get; set; }

    /// <summary>
    /// Gets or sets the collection of trains in this timetable.
    /// </summary>
    public ICollection<Train> Trains { get; set; }

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
    void IJsonOnDeserialized.OnDeserialized()
    {
        this.RebuildStationCalls();
        this.ResolveCatalogueReferences();
        this.RebuildTrainCategories();
        Layout.EnsurePlatforms();
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
    /// Re-establishes the catalogue entries a timetable is read without: a <see cref="Train"/>'s
    /// <see cref="Train.Category"/> and <see cref="Train.Company"/>, and a
    /// <see cref="TrainCategory"/>'s <see cref="TrainCategory.Company"/>, are each looked up by their
    /// foreign key in the catalogue that owns them. Call this whenever a plan is read, before
    /// <see cref="RebuildTrainCategories"/>.
    /// </summary>
    /// <remarks>
    /// These are the only place a category or a company belongs, so they are the only place either is
    /// written (see <c>PlanJson</c>) and everything that uses one keeps just its id. A reference that
    /// survived the reading — a plan written by an earlier version, which stored the whole category or
    /// company on the train — is left alone, so nothing is lost by resolving against a catalogue that
    /// version never filled in.
    /// </remarks>
    /// <param name="timetable">The timetable whose catalogue references to re-establish.</param>
    public static void ResolveCatalogueReferences(this Timetable timetable)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        var categories = timetable.TrainCategories;
        var companies = timetable.Layout.Companies;

        foreach (var category in categories)
            category.Company ??= companies.FirstOrDefault(c => c.Id == category.CompanyId);
        foreach (var train in timetable.Trains)
        {
            train.Category ??= categories.FirstOrDefault(c => c.Id == train.CategoryId);
            train.Company ??= companies.FirstOrDefault(c => c.Id == train.CompanyId);
        }
    }

    /// <summary>
    /// Reconciles the train category catalogue (<see cref="Timetable.TrainCategories"/>) with the
    /// categories the trains actually use: a category a train holds but the catalogue does not know is
    /// added to it, and every category is then given an id that is unique and greater than zero.
    /// Call this whenever a plan is loaded or imported, before it is displayed.
    /// </summary>
    /// <remarks>
    /// A plan written by an earlier version stored the whole category on the train rather than in the
    /// catalogue, so it reads back with trains that have categories and a catalogue that has none. The
    /// Trains tab then groups the trains under categories its own category drop-down cannot offer, and
    /// no train can be moved to another category.
    /// <para>
    /// The ids matter as much as the membership, because a category is picked out by
    /// <see cref="Train.CategoryId"/> rather than by the object: categories left sharing id zero would be
    /// shown as a single category holding all their trains, be taken for the same category when trains
    /// are checked for duplicate numbers, and be confused with the trains that have no category at all.
    /// Now that a plan stores only the id, they would also all read back as the same category.
    /// </para>
    /// </remarks>
    /// <param name="timetable">The timetable whose category catalogue to reconcile.</param>
    public static void RebuildTrainCategories(this Timetable timetable)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        Catalogue.Reconcile(
            timetable.TrainCategories,
            timetable.Trains.Select(t => t.Category),
            category => category.Id,
            (category, id) => category.Id = id);
    }

    /// <summary>
    /// Gets the timetable's trains of <paramref name="category"/>, matched on
    /// <see cref="Train.CategoryId"/> so that a train reading back before its category has been resolved
    /// from the catalogue is found too.
    /// </summary>
    /// <param name="timetable">The timetable whose trains to look through.</param>
    /// <param name="category">The category the trains must belong to.</param>
    public static IEnumerable<Train> TrainsIn(this Timetable timetable, TrainCategory category)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        ArgumentNullException.ThrowIfNull(category);
        return timetable.Trains.Where(t => t.CategoryId == category.Id);
    }

    /// <summary>
    /// Reapplies <see cref="TrainCategory.DefaultPreparationMinutes"/> to every train of
    /// <paramref name="category"/>, moving each train's origin arrival that many minutes before its
    /// departure (see <c>Train.SetPreparationMinutes</c>). Only the preparation time changes: no train
    /// departs or arrives anywhere at another time than it did.
    /// </summary>
    /// <param name="timetable">The timetable whose trains to reapply the default to.</param>
    /// <param name="category">The category whose default to reapply.</param>
    /// <returns>The number of trains changed. A train already prepared for that long is not counted.</returns>
    public static int ApplyDefaultPreparationMinutes(this Timetable timetable, TrainCategory category)
    {
        var changed = 0;
        foreach (var train in timetable.TrainsIn(category))
            if (train.SetPreparationMinutes(category.DefaultPreparationMinutes)) changed++;
        return changed;
    }

    /// <summary>
    /// Reapplies <see cref="TrainCategory.DefaultFinishingMinutes"/> to every train of
    /// <paramref name="category"/>, moving each train's destination departure that many minutes after its
    /// arrival (see <c>Train.SetFinishingMinutes</c>). Only the finishing-up time changes: no train
    /// departs or arrives anywhere at another time than it did.
    /// </summary>
    /// <param name="timetable">The timetable whose trains to reapply the default to.</param>
    /// <param name="category">The category whose default to reapply.</param>
    /// <returns>The number of trains changed. A train already finished in that time is not counted.</returns>
    public static int ApplyDefaultFinishingMinutes(this Timetable timetable, TrainCategory category)
    {
        var changed = 0;
        foreach (var train in timetable.TrainsIn(category))
            if (train.SetFinishingMinutes(category.DefaultFinishingMinutes)) changed++;
        return changed;
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
