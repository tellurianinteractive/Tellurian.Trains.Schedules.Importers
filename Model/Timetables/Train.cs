using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Represents a train, including its identification, category, company, timetable, and operational details.
/// </summary>
/// <remarks>
/// The Train class models a scheduled train and its associated data, such as its number, operator,
/// timetable, and route calls. It supports equality comparison based on company, number, and external identifier.
/// This class is designed for use with Entity Framework Core and includes properties for related entities such as Company
/// and Timetable. Some properties are required for correct instantiation and operation.
/// </remarks>
public class Train : IEquatable<Train>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private Train()
    {
        ExternalId = string.Empty;
        Timetable = default!;
        Calls = [];
        CargoFlows = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Train"/> with the specified id, number, and optional external identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the train.</param>
    /// <param name="number">The train number.</param>
    /// <param name="externalId">An optional external identifier for the train.</param>
    [SetsRequiredMembers]
    public Train(int id, int number, string externalId = "")
    {
        Id = id;
        Number = number;
        ExternalId = externalId;
        Timetable = default!;
        Calls = [];
        CargoFlows = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Train"/> with the specified id, category, number, and optional external identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the train.</param>
    /// <param name="category">The category of the train.</param>
    /// <param name="number">The train number.</param>
    /// <param name="externalId">An optional external identifier for the train.</param>
    [SetsRequiredMembers]
    public Train(int id, TrainCategory category, int number, string externalId = "")
        : this(id, number, externalId)
    {
        Category = category;
        CategoryId = category.Id;
    }

    /// <summary>
    /// Gets or sets the unique identifier for this train.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Gets or sets the train number.
    /// </summary>
    public required int Number { get; set; }

    /// <summary>
    /// Gets or sets the external identifier for this train.
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// Gets or sets an optional remark about this train.
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets the length restrictions for this train.
    /// </summary>
    public TrainCapacity Length { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the operating company. Optional. Follows <see cref="Company"/>
    /// while one is set, so assigning the company is enough to keep the two in step; the stored value
    /// is what a plan is read with, before the company itself has been resolved from the catalogue.
    /// </summary>
    public int? CompanyId { get => Company?.Id ?? field; set; }

    /// <summary>
    /// Gets or sets the company operating this train. Written to a plan as <see cref="CompanyId"/>
    /// alone; the company itself is stored once, in the layout's company catalogue. Setting no company
    /// clears the key too, so the train is not given the old one back the next time the plan is read.
    /// </summary>
    public Company? Company { get => field; set { field = value; if (value is null) CompanyId = null; } }

    /// <summary>
    /// Gets or sets the foreign key to the train category. Optional. Follows <see cref="Category"/>
    /// while one is set, in the same way as <see cref="CompanyId"/>.
    /// </summary>
    public int? CategoryId { get => Category?.Id ?? field; set; }

    /// <summary>
    /// Gets or sets the category of this train. Written to a plan as <see cref="CategoryId"/> alone;
    /// the category itself is stored once, in the timetable's category catalogue. Setting no category
    /// clears the key too, so the train is not given the old one back the next time the plan is read.
    /// </summary>
    public TrainCategory? Category { get => field; set { field = value; if (value is null) CategoryId = null; } }

    /// <summary>
    /// Gets or sets this train's maximum scale speed in km/h. Optional; when unset,
    /// the speed calculation falls back to <see cref="TrainCategory.DefaultSpeed"/>.
    /// </summary>
    public int? MaxSpeed { get; set; }

    /// <summary>
    /// Gets or sets the sessions during which this train operates.
    /// </summary>
    public required Sessions Sessions { get; set; } = Sessions.All;

    /// <summary>
    /// Gets or sets the foreign key to the timetable. Required.
    /// </summary>
    public int TimetableId { get; set; }

    /// <summary>
    /// Gets or sets the timetable this train belongs to.
    /// </summary>
    public Timetable Timetable { get; set; }

    /// <summary>
    /// Gets or sets whether a note is shown at this train's final arrival stating which train it
    /// continues as and when (for example "Continues as G 1234 at 15:30"). The continuation train and
    /// time are derived from the schedule (the vehicle's next working departing the arrival station),
    /// so no explicit link is stored. Leave <c>false</c> for trains that terminate.
    /// </summary>
    public bool ShowContinuesAs { get; set; }

    /// <summary>
    /// Gets or sets the collection of station calls for this train.
    /// </summary>
    public IList<StationCall> Calls { get; set; }

    /// <summary>
    /// Gets or sets the collection of cargo flows on this train. Each <see cref="CargoFlowTrainPart"/>
    /// connects wagons at its from-call and disconnects them at its to-call, referencing a reusable
    /// description in the timetable's cargo flow catalogue.
    /// </summary>
    public IList<CargoFlowTrainPart> CargoFlows { get; set; }

    /// <summary>
    /// Gets the driver's start time (arrival time of first call). The first call is the one the train
    /// runs first; <see cref="Calls"/> is in insertion order, which need not be the run order.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the train has no calls.</exception>
    [JsonIgnore]
    public Time DriverStartTime => Calls.MinBy(c => c.SortTime) is { } first ? first.Arrival : throw NoCalls();

    /// <summary>
    /// Gets the driver's end time (departure time of last call). The last call is the one the train runs
    /// last; <see cref="Calls"/> is in insertion order, which need not be the run order.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the train has no calls.</exception>
    [JsonIgnore]
    public Time DriverEndTime => Calls.MaxBy(c => c.SortTime) is { } last ? last.Departure : throw NoCalls();

    /// <summary>
    /// Gets the station call at the specified index.
    /// </summary>
    /// <param name="index">The index of the call to retrieve.</param>
    /// <returns>The station call at the specified index.</returns>
    public StationCall this[Index index] => Calls[index];

    /// <summary>
    /// Gets the distinct tracks used by this train, ordered by arrival time.
    /// </summary>
    internal IEnumerable<StationTrack> Tracks => Calls.OrderBy(c => c.Arrival.Value).Select(c => c.Track).Distinct();

    /// <summary>
    /// Gets the layout from the first station call, or <c>null</c> when the train has no calls yet and
    /// so belongs to no layout.
    /// </summary>
    [JsonIgnore]
    public Layout? Layout => Calls.Count > 0 ? Calls[0].OperationLocation.Layout : null;

    /// <summary>
    /// Gets this train as a train part covering all station calls, from where it starts its run to
    /// where it ends it. A shunting task, which works at one location and travels nowhere, is the part
    /// from its single call to itself.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the train has fewer than two calls,
    /// which is too few to span a part — unless it is a shunting task, whose one call is its whole
    /// working.</exception>
    [JsonIgnore]
    public ScheduledTrainPart AsTrainPart =>
        this.IsShuntingTask && Calls.Count == 1
            ? new ScheduledTrainPart(Calls[0], Calls[0])
            : this.AsTrainPart(0, Calls.Count - 1);

    // A train under construction may have no calls at all. Everything derived from where it runs is
    // then undefined rather than zero, so it says so instead of failing with a null reference.
    private InvalidOperationException NoCalls() =>
        new($"Train {Number} has no calls, so it has no run to derive times from.");

    /// <inheritdoc/>
    public bool Equals(Train? other) =>
        other is not null &&
        (Company?.Equals(other.Company) ?? other.Company is null) &&
        Number == other.Number;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Train other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(CompanyId, Number);

    /// <inheritdoc/>
    public override string ToString() =>
        string.Format(CultureInfo.CurrentCulture, "{0} {1} {2} {3}", this.EffectiveCompany?.Signature, Category?.Prefix, Number, Category?.Suffix).Trim();
}

/// <summary>
/// Provides extension methods for <see cref="Train"/>.
/// </summary>
public static class TrainExtensions
{
    /// <summary>
    /// The time a train loses at one end of a stretch by standing still there: it has to brake into the
    /// location, or get away from it again. Added to the running time of the stretch leading up to a stop
    /// and of the stretch following it (see <c>ScheduledLegMinutes</c>), so a stop costs two minutes in all
    /// on top of its dwell.
    /// </summary>
    public const int StopAllowanceMinutes = 1;

    extension(string value)
    {
        /// <summary>
        /// Gets the letter prefix from a train identifier string.
        /// </summary>
        public string Prefix =>
            string.IsNullOrWhiteSpace(value) ? "" :
            new([.. value.TakeWhile(c => char.IsLetter(c))]);

        /// <summary>
        /// Gets the numeric part from a train identifier string.
        /// </summary>
        public string NumberPart =>
            string.IsNullOrWhiteSpace(value) ? "" :
            new([.. value.SkipWhile(c => char.IsLetter(c) || char.IsWhiteSpace(c)).TakeWhile(c => char.IsDigit(c))]);

        /// <summary>
        /// Parses the number part of the string, returning 0 if parsing fails.
        /// </summary>
        public int NumberOrZero =>
            int.TryParse(value.NumberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : 0;
    }

    extension(Train train)
    {
        /// <summary>
        /// Creates a train part from the train between the specified call indices.
        /// </summary>
        /// <remarks>
        /// The indices are positions in <c>CallsInRunOrder</c> — the order the train works its calls —
        /// so a part always runs forwards along the route. <see cref="Train.Calls"/> is in insertion
        /// order, which on a hand-edited train is not the run order.
        /// </remarks>
        /// <param name="fromCallIndex">The index of the departure call.</param>
        /// <param name="toCallIndex">The index of the arrival call.</param>
        /// <returns>A new train part.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the indices are invalid.</exception>
        public ScheduledTrainPart AsTrainPart(int fromCallIndex, int toCallIndex)
        {
            var t = train.ValueOrException(nameof(train));
            var calls = t.CallsInRunOrder;
            var c = calls.Count;
            // A shunting task spans no route: its one call is both ends of the only part it has, so the
            // rule that a part must reach a later call than it starts from does not apply to it.
            if (t.IsShuntingTask && c == 1 && fromCallIndex == 0 && toCallIndex == 0)
                return new ScheduledTrainPart(calls[0], calls[0]);
            (fromCallIndex < 0 || fromCallIndex > c - 2).IfTrueThrows(nameof(fromCallIndex));
            (toCallIndex <= fromCallIndex || toCallIndex > c - 1).IfTrueThrows(nameof(toCallIndex));
            return new ScheduledTrainPart(calls[fromCallIndex], calls[toCallIndex]);
        }
        /// <summary>
        /// Gets the full identity string for the train (category prefix + number + suffix).
        /// </summary>
        public string Identity =>
            train.Category?.TrainIdentity(train.Number) ?? train.Number.ToString();

        /// <summary>
        /// Gets the company that operates the train: the train's own <see cref="Train.Company"/> when set,
        /// otherwise the company of its <see cref="Train.Category"/> (<see cref="TrainCategory.Company"/>),
        /// or <c>null</c> when neither is set. Mirrors how <see cref="Train.MaxSpeed"/> falls back to the
        /// category's <see cref="TrainCategory.DefaultSpeed"/>. Use this for display and reporting; the
        /// stored <see cref="Train.Company"/> stays <c>null</c> while a train inherits its category's company.
        /// </summary>
        public Company? EffectiveCompany => train.Company ?? train.Category?.Company;

        /// <summary>
        /// Gets a one-line label for the train suitable for a drop-down: its identity (with the
        /// effective company signature) followed by the first and last call as station signatures and
        /// times, e.g. "G 4321  Gbg 12:00→Snu 12:55". A shunting task is written as the one place it is
        /// worked at and the span of the work, e.g. "V 801  RSE 06:00-08:00". The from/to part is
        /// omitted when the train has no calls.
        /// </summary>
        /// <remarks>
        /// The two forms differ because the two things do. A travelling train goes from somewhere to
        /// somewhere, so the arrow reads as the journey and each end names its own place. A shunting task
        /// goes nowhere: naming its one station twice with an arrow between would read as a journey that
        /// ends where it began, and the times would come out backwards — the departure leads on a
        /// travelling train because that is when it sets off, whereas a task's departure is when it
        /// <em>finishes</em>. So the place is said once and the times are given as the span they are.
        /// </remarks>
        public string ListLabel
        {
            get
            {
                var identity = train.EffectiveCompany is { } c ? $"{c.Signature} {train.Identity}".Trim() : train.Identity;
                if (train.Calls.Count == 0) return identity;
                if (train.ShuntingCall is { } task)
                    return $"{identity}  {task.OperationLocation.Signature} {task.Arrival.HHMM()}-{task.Departure.HHMM()}";
                var ordered = train.Calls.OrderBy(call => call.SortTime).ToList();
                var first = ordered[0];
                var last = ordered[^1];
                return $"{identity}  {first.OperationLocation.Signature} {first.Departure.HHMM()}→{last.OperationLocation.Signature} {last.Arrival.HHMM()}";
            }
        }

        /// <summary>
        /// Gets the train's calls in the order it runs them, earliest first.
        /// </summary>
        /// <remarks>
        /// <see cref="Train.Calls"/> is in insertion order, which is not the run order: a call added last
        /// can be timed first, and editing a time reorders the route without touching the collection.
        /// Anything reasoning about the route — where it starts and ends, which legs it travels — must read
        /// the calls through here, ordered by <see cref="StationCall.SortTime"/>.
        /// </remarks>
        public IReadOnlyList<StationCall> CallsInRunOrder => [.. train.Calls.OrderBy(c => c.SortTime)];

        /// <summary>
        /// The train's preparation time in minutes: the gap at its origin call between the arrival, where
        /// the vehicles and the loco driver are first tied up, and the departure. Zero when the train has
        /// no calls.
        /// </summary>
        public int PreparationMinutes =>
            train.Calls.Count == 0 ? 0 : MinutesBetween(train.CallsInRunOrder[0]);

        /// <summary>
        /// The train's finishing-up time in minutes: the gap at its destination call between the arrival
        /// and the departure, where the vehicles and the loco driver are freed again. Zero when the train
        /// has no calls.
        /// </summary>
        public int FinishingMinutes =>
            train.Calls.Count == 0 ? 0 : MinutesBetween(train.CallsInRunOrder[^1]);

        /// <summary>
        /// Sets the train's preparation time by moving its origin call's arrival <paramref name="minutes"/>
        /// before that call's departure. Nothing else moves, so the train runs exactly as it did.
        /// </summary>
        /// <param name="minutes">The preparation time in minutes. Must be non-negative.</param>
        /// <returns><c>true</c> when the origin arrival was changed; <c>false</c> when it already was that
        /// many minutes before the departure, when the train has fewer than two calls (it then has no run
        /// to prepare for), or when the preparation would start before midnight of the operating day.</returns>
        public bool SetPreparationMinutes(int minutes)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(minutes);
            if (train.Calls.Count < 2) return false;
            var origin = train.CallsInRunOrder[0];
            var arrival = origin.Departure.AddMinutes(-minutes);
            if (arrival.Value < TimeSpan.Zero || arrival == origin.Arrival) return false;
            origin.Arrival = arrival;
            return true;
        }

        /// <summary>
        /// Sets the train's finishing-up time by moving its destination call's departure
        /// <paramref name="minutes"/> after that call's arrival. Nothing else moves, so the train runs
        /// exactly as it did.
        /// </summary>
        /// <param name="minutes">The finishing-up time in minutes. Must be non-negative.</param>
        /// <returns><c>true</c> when the destination departure was changed; <c>false</c> when it already
        /// was that many minutes after the arrival, or when the train has fewer than two calls (it then
        /// has no run to finish).</returns>
        public bool SetFinishingMinutes(int minutes)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(minutes);
            if (train.Calls.Count < 2) return false;
            var destination = train.CallsInRunOrder[^1];
            var departure = destination.Arrival.AddMinutes(minutes);
            if (departure == destination.Departure) return false;
            destination.Departure = departure;
            return true;
        }

        /// <summary>
        /// Gets whether this is a passenger train, from its <see cref="Train.Category"/>. A train may be
        /// both a passenger and a cargo train at the same time.
        /// </summary>
        public bool IsPassenger => train.Category is { } category && category.IsPassenger;

        /// <summary>
        /// Gets whether this is a cargo (freight) train, from its <see cref="Train.Category"/>. A train may
        /// be both a passenger and a cargo train at the same time.
        /// </summary>
        public bool IsCargo => train.Category is { } category && category.IsFreight;

        /// <summary>
        /// Gets whether this is a shunting task rather than a train that travels, from its
        /// <see cref="Train.Category"/> (see <see cref="TrainCategory.IsShunting"/>). A shunting task works
        /// at one operating location: it has exactly one call, whose arrival is the time the work starts
        /// and whose departure the time it ends.
        /// </summary>
        public bool IsShuntingTask => train.Category?.IsShunting ?? false;

        /// <summary>
        /// The one call a shunting task works at, or <c>null</c> when this is not a shunting task or it
        /// does not have exactly the one call it should (a task under construction, or one whose category
        /// was changed to shunting while it still had a route).
        /// </summary>
        public StationCall? ShuntingCall =>
            train.IsShuntingTask && train.Calls.Count == 1 ? train.Calls[0] : null;

        /// <summary>
        /// The calls at which a cargo flow on this train may connect its wagons: a departure stop at a
        /// location that exchanges cargo.
        /// </summary>
        public IEnumerable<StationCall> CargoFlowDepartureCalls =>
            train.DepartureCalls.Where(c => c.OperationLocation.HasCargoExchange);

        /// <summary>
        /// The calls at which a cargo flow connected at <paramref name="from"/> may disconnect its wagons:
        /// an arrival stop exchanging cargo later in the run — or, on a shunting task, the very call the
        /// flow connects at, because the task neither travels nor has another call to reach.
        /// </summary>
        /// <param name="from">The call the flow connects its wagons at.</param>
        public IEnumerable<StationCall> CargoFlowArrivalCalls(StationCall from) =>
            train.IsShuntingTask
                ? [from]
                : train.ArrivalCallsAfter(from).Where(c => c.OperationLocation.HasCargoExchange);

        /// <summary>
        /// The time a cargo flow connects its wagons at <paramref name="call"/>: the train's departure
        /// from it — but on a shunting task the arrival, because the task's flow spans its single call
        /// from when the work starts to when it ends, and the work starts at the arrival.
        /// </summary>
        /// <param name="call">The call the flow connects its wagons at.</param>
        public Time CargoFlowConnectTime(StationCall call) =>
            train.IsShuntingTask ? call.Arrival : call.Departure;

        /// <summary>
        /// The time a cargo flow disconnects its wagons at <paramref name="call"/>: the train's arrival
        /// at it — but on a shunting task the departure, which is when the work ends.
        /// </summary>
        /// <param name="call">The call the flow disconnects its wagons at.</param>
        public Time CargoFlowDisconnectTime(StationCall call) =>
            train.IsShuntingTask ? call.Departure : call.Arrival;

        /// <summary>
        /// Whether a cargo flow can be added to this train at all: it carries cargo and has a pair of
        /// calls to span (on a shunting task, its single call twice over).
        /// </summary>
        public bool CanHostCargoFlow =>
            train.IsCargo && train.CargoFlowDepartureCalls.Any(from => train.CargoFlowArrivalCalls(from).Any());

        /// <summary>
        /// Gets whether this train is able to stop at <paramref name="location"/> at all, whatever a call
        /// there says. A train stops to exchange something, so it needs somewhere to exchange it: a
        /// passenger train needs <see cref="Layouts.OperationLocation.HasPassengerExchange"/>, a cargo
        /// train <see cref="Layouts.OperationLocation.HasCargoExchange"/>, and a train that is both needs
        /// either. Nowhere can a train stop at a <see cref="SignalControlledLocation"/>, which exists for
        /// block and dispatch boundaries only.
        /// </summary>
        /// <remarks>
        /// A train whose category is neither passenger nor cargo — empty stock, a light engine, a category
        /// not yet filled in — carries nothing to exchange, so no exchange is demanded of the location and
        /// only the location type restricts it. A shadow station always exchanges both (see
        /// <see cref="Station.HasPassengerExchange"/>), so it needs no case of its own here.
        /// </remarks>
        /// <param name="location">The operation location to test.</param>
        public bool CanStopAt(OperationLocation? location) =>
            location is not null and not SignalControlledLocation &&
            ((train.IsPassenger && location.HasPassengerExchange) ||
             (train.IsCargo && location.HasCargoExchange) ||
             (!train.IsPassenger && !train.IsCargo));

        /// <summary>
        /// Gets the train's calls where it stops to depart (a wagon can be connected here), earliest first.
        /// </summary>
        public IEnumerable<StationCall> DepartureCalls =>
            train.Calls.Where(c => c.IsStop && c.IsDeparture).OrderBy(c => c.SortTime);

        /// <summary>
        /// Gets the train's arrival stops that fall after the given call (a wagon connected at
        /// <paramref name="call"/> can be disconnected here), latest first.
        /// </summary>
        /// <param name="call">The call the later arrivals must follow.</param>
        public IEnumerable<StationCall> ArrivalCallsAfter(StationCall call) =>
            train.Calls.Where(c => c.IsStop && c.IsArrival && c.SortTime > call.SortTime).OrderByDescending(c => c.SortTime);

        /// <summary>
        /// Gets the effective scale speed (km/h) for this train on a track stretch:
        /// the lower of the train speed (its <see cref="Train.MaxSpeed"/>, or its category's
        /// <see cref="TrainCategory.DefaultSpeed"/> when unset) and the stretch's maximum speed.
        /// </summary>
        /// <param name="stretch">The track stretch the train runs on.</param>
        /// <returns>The effective scale speed in km/h.</returns>
        public int EffectiveScaleSpeed(TrackStretch stretch)
        {
            var trainSpeed = train.MaxSpeed ?? train.Category?.DefaultSpeed ?? stretch.Speed;
            return Math.Min(trainSpeed, stretch.Speed);
        }

        /// <summary>
        /// Gets the effective real model speed (m/s) for this train on a track stretch,
        /// by mapping the effective scale speed through the speed curve.
        /// </summary>
        /// <param name="stretch">The track stretch the train runs on.</param>
        /// <param name="settings">The time and speed settings holding the speed mapping curve.</param>
        /// <returns>The effective real model speed in metres per second.</returns>
        public double EffectiveRealSpeedMetersPerSecond(TrackStretch stretch, TimeAndSpeedSettings settings) =>
            settings.RealSpeedMetersPerSecond(train.EffectiveScaleSpeed(stretch));

        /// <summary>
        /// Gets the scheduled (fast-clock) travel time in minutes for this train across a track stretch,
        /// derived from the stretch distance, the effective real speed, and the fast-clock multiplier.
        /// </summary>
        /// <param name="stretch">The track stretch the train runs on.</param>
        /// <param name="settings">The time and speed settings holding the speed curve and fast-clock speed.</param>
        /// <returns>The scheduled travel time in fast-clock minutes; zero when the real speed is not positive.</returns>
        public double ScheduledTravelMinutes(TrackStretch stretch, TimeAndSpeedSettings settings)
        {
            var realSpeed = train.EffectiveRealSpeedMetersPerSecond(stretch, settings);
            if (realSpeed <= 0) return 0;
            var realSeconds = stretch.Distance / realSpeed;
            return realSeconds / 60.0 * settings.FastClockSpeed;
        }

        /// <summary>
        /// Gets the scheduled (fast-clock) run time in whole minutes for this train over one leg of its
        /// journey: the travel time across <paramref name="stretch"/>, plus a minute for each end of the
        /// leg where the train stands still.
        /// </summary>
        /// <remarks>
        /// <see cref="ScheduledTravelMinutes"/> is the time for a train running the stretch at speed. A train
        /// that stands still at one end loses time on top of that — braking into the location, or getting away
        /// from it again — so a stop costs a minute on the stretch leading up to its arrival and a minute on
        /// the stretch following its departure, and a leg between two stops costs both. The allowance is added
        /// to the running time and is quite separate from the dwell at the location itself. A train stands
        /// still at its origin and at its terminus as surely as at any stop in between, so those count too;
        /// a pass-through does not.
        /// </remarks>
        /// <param name="stretch">The track stretch the train runs on.</param>
        /// <param name="settings">The time and speed settings holding the speed curve and fast-clock speed.</param>
        /// <param name="standsAtStart">Whether the train stands still at the call the leg starts from.</param>
        /// <param name="standsAtEnd">Whether the train stands still at the call the leg ends at.</param>
        /// <returns>The run time in fast-clock minutes; the running time is at least one minute before the
        /// allowances are added.</returns>
        public int ScheduledLegMinutes(TrackStretch stretch, TimeAndSpeedSettings settings, bool standsAtStart, bool standsAtEnd)
        {
            var running = Math.Max(1, (int)Math.Round(train.ScheduledTravelMinutes(stretch, settings)));
            return running + (standsAtStart ? StopAllowanceMinutes : 0) + (standsAtEnd ? StopAllowanceMinutes : 0);
        }

        /// <summary>
        /// Finds the station call at the specified time.
        /// </summary>
        /// <param name="time">The time to search for.</param>
        /// <returns>The matching station call.</returns>
        public StationCall StationCall(Time time)
        {
            try
            {
                foreach (var call in train.Calls)
                {
                    if (time == call.Arrival || time == call.Departure) return call;
                }
                foreach (var call in train.Calls)
                {
                    if (time >= call.Arrival && time <= call.Departure) return call;
                }
                StationCall? previous = default;
                foreach (var call in train.Calls)
                {
                    if (previous is not null && call.OperationLocation.Id == previous.OperationLocation.Id)
                    {
                        if (time >= previous.Arrival || time <= call.Departure) return call;
                    }
                    previous = call;
                }
                throw new ArgumentNullException(nameof(time));

            }
            catch (Exception)
            {
                Debugger.Break();
                throw;
            }
        }

        /// <summary>
        /// Creates a cargo flow over a segment of this train and adds it to <see cref="Train.CargoFlows"/>.
        /// </summary>
        /// <param name="id">The unique identifier for the cargo flow train part.</param>
        /// <param name="from">The departure call where wagons are connected.</param>
        /// <param name="to">The arrival call where wagons are disconnected.</param>
        /// <param name="options">The cargo flow description (a timetable catalogue entry) to reference.</param>
        /// <param name="positionInTrain">The position of the cargo flow within the train (default 1).</param>
        /// <returns>The created cargo flow, already added to the train.</returns>
        public CargoFlowTrainPart CreateCargoFlow(int id, StationCall from, StationCall to, CargoFlowOptions options, int positionInTrain = 1)
        {
            var cargoFlow = new CargoFlowTrainPart(from, to)
            {
                Id = id,
                CargoFlowOptions = options,
                CargoFlowOptionsId = options.Id,
                PositionInTrain = positionInTrain,
            };
            return train.Add(cargoFlow);
        }

        /// <summary>
        /// Adds a cargo flow to the train.
        /// </summary>
        /// <param name="cargoFlow">The cargo flow to add.</param>
        /// <returns>The added cargo flow.</returns>
        public CargoFlowTrainPart Add(CargoFlowTrainPart cargoFlow)
        {
            cargoFlow = cargoFlow.ValueOrException(nameof(cargoFlow));
            if (!train.CargoFlows.Contains(cargoFlow)) train.CargoFlows.Add(cargoFlow);
            return cargoFlow;
        }

        /// <summary>
        /// Adds a station call to the train.
        /// </summary>
        /// <param name="call">The station call to add.</param>
        /// <returns>The added station call.</returns>
        public StationCall Add(StationCall call)
        {
            train = train.ValueOrException(nameof(train));
            call = call.ValueOrException(nameof(call));
            if (!train.Calls.Contains(call))
            {
                call.SetTrain(train);
                if (train.Calls.Count == 0)
                {
                    call.IsArrival = false;
                    call.IsDeparture = true;
                }

                train.Calls.Add(call);
                // The train owns the call; keep the per-track call index in step as the call joins the train.
                call.Track.Add(call);
            }
            return call;
        }

    }

    // The minutes a call is held, from its arrival to its departure. At a train's origin that is its
    // preparation time and at its destination its finishing-up time.
    private static int MinutesBetween(StationCall call) =>
        (int)call.Departure.Subtract(call.Arrival).TotalMinutes;

    /// <summary>
    /// Determines whether the train is null or has no station calls.
    /// </summary>
    /// <param name="train">The train to check.</param>
    /// <returns><c>true</c> if the train is null or has no calls; otherwise, <c>false</c>.</returns>
    public static bool IsNullOrHasNoCalls([NotNullWhen(false)] this Train? train)
        => train is null || train.Calls.Count == 0;
}
