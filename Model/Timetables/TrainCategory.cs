using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Represents a category or type of train with associated display properties.
/// </summary>
public record TrainCategory
{
    /// <summary>
    /// Gets or sets the unique identifier for this train category. Unique within the timetable's
    /// catalogue and greater than zero; a category read from a plan that breaks either is renumbered
    /// (see <c>Timetable.RebuildTrainCategories</c>).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the prefix shown before train number (e.g., "P" for passenger, "G" for goods).
    /// Used in train identity display.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional suffix shown after train number.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets what trains of this category exchange where they stop — passengers, cargo, both, or
    /// nothing at all. This is what every stop rule tests (see <c>Train.CanStopAt</c>), so it is what
    /// decides where trains of this category are able to stop.
    /// </summary>
    /// <remarks>
    /// <see cref="TrainContent.None"/> makes it a service category, whose trains hand nothing over: a
    /// construction train, or a locomotive or trainset moved out of service. The category
    /// <see cref="Name"/> says which of those it is. A shunting category always includes
    /// <see cref="TrainContent.Cargo"/>, because what it handles is cargo wagons; see
    /// <see cref="IsShunting"/>.
    /// </remarks>
    public TrainContent Content { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether trains of this category are <em>shunting tasks</em> rather
    /// than trains that travel: work done at one operating location over a span of time, typically making
    /// up the wagons of a departing train or taking the wagons of an arrived one out to the cargo
    /// customers. A shunting task has exactly one station call, whose arrival is the time the work starts
    /// and whose departure is the time it ends — the same two times that bound any train's working.
    /// </summary>
    /// <remarks>
    /// A shunting category is a freight category too — its <see cref="Content"/> includes
    /// <see cref="TrainContent.Cargo"/> — because what it handles is cargo wagons: that is what lets it
    /// stop where cargo is exchanged and carry the cargo flows that say
    /// which wagons to shunt. What it does <em>not</em> do is travel, so its trains are left out of the
    /// reports that draw or tabulate a run over a stretch. See <c>TrainExtensions.IsShuntingTask</c>.
    /// </remarks>
    public bool IsShunting { get; set; }

    /// <summary>
    /// Gets or sets the name of this train category.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the color used when drawing timetable graphs and schematic train lines.
    /// Format: CSS color string (e.g., "#FF0000", "red", "rgb(255,0,0)")
    /// </summary>
    public string Color { get; set; } = "#000000";

    /// <summary>
    /// Gets or sets the default scale speed in km/h for trains of this category.
    /// Used when a train does not set its own <see cref="Train.MaxSpeed"/>.
    /// </summary>
    public int DefaultSpeed { get; set; } = 100;

    /// <summary>
    /// Gets or sets the lowest train number allotted to this category. New trains of this category are
    /// numbered from here upwards, picking the next free number of the parity that matches their travel
    /// direction (odd running downwards, even running upwards). A category can therefore be given its own
    /// number band (for example freight from 5000), keeping each category's numbers distinct. Defaults to 1.
    /// </summary>
    public int StartNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the default preparation time in minutes for trains of this category: how long before
    /// its first departure the train is made ready, which is where the vehicles and the loco driver are
    /// first tied up. A new train of this category is created with it, and it can be reapplied to the
    /// trains that already exist (see <c>TimetableExtensions.ApplyDefaultPreparationMinutes</c>).
    /// Defaults to 10.
    /// </summary>
    public int DefaultPreparationMinutes { get; set; } = 10;

    /// <summary>
    /// Gets or sets the default finishing-up time in minutes for trains of this category: how long after
    /// its last arrival the train is put away, which is where the vehicles and the loco driver are freed
    /// again. A new train of this category is created with it, and it can be reapplied to the trains that
    /// already exist (see <c>TimetableExtensions.ApplyDefaultFinishingMinutes</c>). Defaults to 10.
    /// </summary>
    public int DefaultFinishingMinutes { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether trains of this category are excluded from automatic schedule building.
    /// When <c>true</c>, the automatic builder never seeds or chains these trains; they can still be
    /// added to a schedule manually. Defaults to <c>false</c>.
    /// </summary>
    public bool ExcludeFromAutomaticScheduling { get; set; }

    /// <summary>
    /// Gets or sets this category's <em>stop pattern</em>: the ids of the operating locations where
    /// trains of the category stop on their way. A positive list — what is named here is stopped at, and
    /// what is not named is run through — so it is what tells the route creator where to plan a stop
    /// (see <c>Plan.Create</c>), and a train stopping anywhere else is reported (see
    /// <c>Train.CheckStopPattern</c>).
    /// </summary>
    /// <remarks>
    /// An empty list is no pattern at all rather than a pattern of nowhere: the route creator then stops
    /// wherever the category can exchange what it carries, exactly as it did before there were stop
    /// patterns, and no stop is reported. A category that runs non-stop is said so by naming only the
    /// locations it does stop at, which for a through train is its two ends.
    /// <para>
    /// A pattern binds nothing that already exists. It says where a <em>new</em> route is given stops;
    /// the trains already planned keep the stops they have, and the ones outside the pattern are reported
    /// so the planner can decide between the train and the pattern. Reading a plan fills the pattern in
    /// from the trains the category already has, where it has none (see <c>Plan.DeduceStopPatterns</c>).
    /// </para>
    /// <para>
    /// The locations are stored by id, as a station's regions are, because they belong to the layout and
    /// are stored there. Nothing resolves them into objects: both the route creator and the check have
    /// the location in hand and ask only whether it is named.
    /// </para>
    /// </remarks>
    [JsonConverter(typeof(Schedules.IdListConverter))]
    public IList<int> StopLocationIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the foreign key to the company that operates trains in this category. Optional.
    /// Follows <see cref="Company"/> while one is set, so assigning the company is enough to keep the
    /// two in step; the stored value is what a plan is read with, before the company itself has been
    /// resolved from the catalogue.
    /// </summary>
    public int? CompanyId { get => Company?.Id ?? field; set; }

    /// <summary>
    /// Optional company that operates trains in this category. Written to a plan as
    /// <see cref="CompanyId"/> alone; the company itself is stored once, in the layout's company
    /// catalogue. Setting no company clears the key too, so the category is not given the old one back
    /// the next time the plan is read.
    /// </summary>
    public Company? Company { get => field; set { field = value; if (value is null) CompanyId = null; } }

    /// <summary>
    /// Two categories are the same category when they have the same <see cref="Id"/>, which is what the
    /// whole plan picks one out by (a train keeps only <see cref="Train.CategoryId"/>). The catalogue
    /// keeps the ids unique and greater than zero (see <c>Timetable.RebuildTrainCategories</c>), so this
    /// is identity within a timetable, as it is for a <see cref="Layouts.Layout"/>, a
    /// <see cref="Schedules.Plan"/> or a <see cref="Timetable"/>.
    /// </summary>
    /// <remarks>
    /// Stated rather than left to the record's own field-by-field equality, which a collection member
    /// silently breaks: two categories holding equal but separate stop patterns would compare unequal,
    /// because a list is compared by reference.
    /// </remarks>
    public virtual bool Equals(TrainCategory? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Name;

}

/// <summary>
/// Provides extension methods for <see cref="TrainCategory"/>.
/// </summary>
public static class TrainCategoryExtensions
{
    extension(TrainCategory category)
    {
        /// <summary>
        /// Gets whether trains of this category exchange passengers where they stop, from
        /// <see cref="TrainCategory.Content"/>.
        /// </summary>
        public bool IsPassenger => category.Content.HasFlag(TrainContent.Passenger);

        /// <summary>
        /// Gets whether trains of this category exchange cargo where they stop, from
        /// <see cref="TrainCategory.Content"/>. A shunting category always does, whatever its content
        /// says: what it handles is cargo wagons.
        /// </summary>
        public bool IsFreight => category.Content.HasFlag(TrainContent.Cargo) || category.IsShunting;

        /// <summary>
        /// Gets whether this is a service category: its trains exchange nothing at all and are not
        /// shunting tasks either. A construction train or a vehicle moved out of service; the category
        /// <see cref="TrainCategory.Name"/> says which.
        /// </summary>
        public bool IsService => category.Content is TrainContent.None && !category.IsShunting;

        /// <summary>
        /// Gets whether this category has a stop pattern: at least one operating location is named in
        /// <see cref="TrainCategory.StopLocationIds"/>. Where it has none, nothing is constrained — see
        /// <see cref="TrainCategory.StopLocationIds"/> for why an empty list is no pattern.
        /// </summary>
        public bool HasStopPattern => category.StopLocationIds.Count > 0;

        /// <summary>
        /// Gets whether a train of this category is able to stop at <paramref name="location"/> at all,
        /// whatever a call there says and whatever the stop pattern names. A train stops to exchange
        /// something, so it needs somewhere to exchange it: a passenger category needs
        /// <see cref="Layouts.OperationLocation.HasPassengerExchange"/>, a freight category
        /// <see cref="Layouts.OperationLocation.HasCargoExchange"/>, and one that is both needs either.
        /// Nowhere can a train stop at a <see cref="SignalControlledLocation"/>, which exists for block and
        /// dispatch boundaries only.
        /// </summary>
        /// <remarks>
        /// A service category exchanges nothing, so no exchange is demanded of the location and only the
        /// location type restricts it. A shadow station always exchanges both (see
        /// <see cref="Station.HasPassengerExchange"/>), so it needs no case of its own here. This is the
        /// rule <c>Train.CanStopAt</c> answers by, asked of the category alone — which is what the stop
        /// pattern is chosen from, since a pattern can only name locations its trains could stop at.
        /// </remarks>
        /// <param name="location">The operating location to test.</param>
        public bool CanStopAt(OperationLocation? location) =>
            location is not null and not SignalControlledLocation &&
            ((category.IsPassenger && location.HasPassengerExchange) ||
             (category.IsFreight && location.HasCargoExchange) ||
             (!category.IsPassenger && !category.IsFreight));

        /// <summary>
        /// Gets whether the stop pattern lets a train of this category stop at <paramref name="location"/>:
        /// the location is named in <see cref="TrainCategory.StopLocationIds"/>, or the category has no
        /// pattern, which constrains nothing.
        /// </summary>
        /// <remarks>
        /// This answers the pattern alone. Whether the train can stop there at all is a separate question,
        /// asked of the location and what the train carries (see <c>Train.CanStopAt</c>): a pattern can
        /// only narrow where a route stops, never open a stop the location does not allow.
        /// </remarks>
        /// <param name="location">The operating location to test.</param>
        public bool AllowsStopAt(OperationLocation? location) =>
            location is not null &&
            (!category.HasStopPattern || category.StopLocationIds.Contains(location.Id));

        /// <summary>
        /// The operating locations of <paramref name="layout"/> that this category's stop pattern names,
        /// in the layout's own order. Empty where the category has no pattern, and where a pattern names
        /// a location the layout no longer has.
        /// </summary>
        /// <param name="layout">The layout whose locations the pattern is read against.</param>
        public IEnumerable<OperationLocation> StopLocations(Layout layout) =>
            layout is null ? [] : layout.OperationLocations.Where(location => category.StopLocationIds.Contains(location.Id));

        /// <summary>
        /// Gets the full train identity string for a given train number.
        /// </summary>
        /// <param name="trainNumber">The train number.</param>
        /// <returns>A formatted string combining prefix, number, and suffix.</returns>
        public string TrainIdentity(int trainNumber) =>
            $"{category.Prefix} {trainNumber} {category.Suffix}".Trim();

        /// <summary>
        /// The standard pair of train categories — Passenger (prefix <c>P</c>) and Freight (prefix
        /// <c>G</c>) — named in the given two-letter <paramref name="language"/> (falling back to
        /// English), with no operating company. Used to seed a new timetable.
        /// </summary>
        /// <param name="language">The two-letter language code of the layout's default language.</param>
        public static IEnumerable<TrainCategory> DefaultsFor(string language) =>
            Defaults.Select(d => new TrainCategory
            {
                Id = d.Id,
                Name = d.NameFor(language),
                Prefix = d.Prefix,
                Color = d.Color,
                Content = d.Content,
                DefaultSpeed = d.DefaultSpeed,
                StartNumber = d.StartNumber,
            });
    }

    // The localised names of the standard categories; used only when seeding a new timetable, after
    // which each category keeps a single Name in the layout's default language. Passenger and freight
    // start in separate number bands so their trains never collide.
    private static readonly TrainCategoryDefault[] Defaults =
    [
        new(1, "P", "#CC0000", TrainContent.Passenger, DefaultSpeed: 100, StartNumber: 1,    EN: "Passenger", DA: "Persontog", DE: "Reisezug",  NB: "Persontog", SV: "Persontåg"),
        new(2, "G", "#000000", TrainContent.Cargo,     DefaultSpeed: 80,  StartNumber: 5000, EN: "Freight",   DA: "Godstog",   DE: "Güterzug",  NB: "Godstog",   SV: "Godståg"),
    ];

    private sealed record TrainCategoryDefault(int Id, string Prefix, string Color, TrainContent Content, int DefaultSpeed, int StartNumber, string EN, string DA, string DE, string NB, string SV)
    {
        public string NameFor(string language) => language switch
        {
            "sv" => SV,
            "da" => DA,
            "de" => DE,
            "nb" or "nn" => NB,
            _ => EN,
        };
    }
}
