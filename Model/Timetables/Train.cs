using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Settings;

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
        Groups = [];
        Calls = [];
        WagonGroups = [];
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
        Groups = [];
        Calls = [];
        WagonGroups = [];
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
    public TrainLenght Length { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the operating company. Optional.
    /// </summary>
    public int? CompanyId { get; set; }

    /// <summary>
    /// Gets or sets the company operating this train.
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the train category. Optional.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category of this train.
    /// </summary>
    public TrainCategory? Category { get; set; }

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
    /// Gets or sets the groups this train belongs to.
    /// </summary>
    public IList<string> Groups { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the timetable. Required.
    /// </summary>
    public int TimetableId { get; set; }

    /// <summary>
    /// Gets or sets the timetable this train belongs to.
    /// </summary>
    public Timetable Timetable { get; set; }

    /// <summary>
    /// Gets or sets the train that this train continues as at its final destination.
    /// Used when a train reverses direction, e.g. an odd-numbered train arriving at a terminus
    /// continues as an even-numbered train in the opposite direction.
    /// </summary>
    public Train? ContinuesAs { get; set; }

    /// <summary>
    /// Gets or sets the train that this train is a continuation of.
    /// </summary>
    public Train? ContinuesFrom { get; set; }

    /// <summary>
    /// Gets or sets the collection of station calls for this train.
    /// </summary>
    public IList<StationCall> Calls { get; set; }

    /// <summary>
    /// Gets or sets the collection of wagon groups in this train.
    /// </summary>
    public IList<WagonGroup> WagonGroups { get; set; }

    /// <summary>
    /// Gets the driver's start time (arrival time of first call).
    /// </summary>
    public Time DriverStartTime => this[0].Arrival;

    /// <summary>
    /// Gets the driver's end time (departure time of last call).
    /// </summary>
    public Time DriverEndTime => this[^1].Departure;

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
    /// Gets the layout from the first station call.
    /// </summary>
    public Layout Layout => Calls[0].Station.Layout;

    /// <summary>
    /// Gets this train as a train part covering all station calls.
    /// </summary>
    public TrainPart AsTrainPart => this.AsTrainPart(0, Calls.Count - 1);

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
        string.Format(CultureInfo.CurrentCulture, "{0} {1} {2} {3}", Company?.Signature, Category?.Prefix, Number, Category?.Suffix).Trim();
}

/// <summary>
/// Provides extension methods for <see cref="Train"/>.
/// </summary>
public static class TrainExtensions
{
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
        /// <param name="fromCallIndex">The index of the departure call.</param>
        /// <param name="toCallIndex">The index of the arrival call.</param>
        /// <returns>A new train part.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the indices are invalid.</exception>
        public TrainPart AsTrainPart(int fromCallIndex, int toCallIndex)
        {
            var t = train.ValueOrException(nameof(train));
            var c = t.Calls.Count;
            (fromCallIndex < 0 || fromCallIndex > c - 2).IfTrueThrows(nameof(fromCallIndex));
            (toCallIndex <= fromCallIndex || toCallIndex > c - 1).IfTrueThrows(nameof(toCallIndex));
            var calls = t.Calls.ToArray();
            return new TrainPart(calls[fromCallIndex], calls[toCallIndex]);
        }
        /// <summary>
        /// Gets the full identity string for the train (category prefix + number + suffix).
        /// </summary>
        public string Identity =>
            train.Category?.TrainIdentity(train.Number) ?? train.Number.ToString();

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
                    if (previous is not null && call.Station.Id == previous.Station.Id)
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
        /// Creates a wagon group for this train between two times.
        /// </summary>
        /// <param name="id">The unique identifier for the wagon group.</param>
        /// <param name="from">The departure time.</param>
        /// <param name="to">The arrival time.</param>
        /// <param name="positionInTrain">The position of the wagon group in the train.</param>
        /// <param name="remark">An optional remark.</param>
        /// <returns>The created wagon group.</returns>
        public WagonGroup CreateWagonGroup(int id, Time from, Time to, int positionInTrain, string? remark = null)
        {
            var fromCall = train.StationCall(from).ValueOrException(nameof(from));
            var toCall = train.StationCall(to).ValueOrException(nameof(to));
            return new()
            {
                Id = id,
                FromStationCall = fromCall,
                FromStationCallId = fromCall.Id,
                ToStationCall = toCall,
                ToStationCallId = toCall.Id,
                PositionInTrain = positionInTrain,
                Remark = remark
            };
        }

        /// <summary>
        /// Adds a wagon group to the train.
        /// </summary>
        /// <param name="wagonGroup">The wagon group to add.</param>
        /// <returns>The added wagon group, or null if input was null.</returns>
        public WagonGroup? Add(WagonGroup? wagonGroup)
        {
            if (wagonGroup is not null && !train.WagonGroups.Contains(wagonGroup))
            {
                wagonGroup.Train = train;
                wagonGroup.TrainId = train.Id;
                train.WagonGroups.Add(wagonGroup);
            }
            return wagonGroup;
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
            }
            return call;
        }

        /// <summary>
        /// Fixes a train that has only one call by duplicating it as arrival and departure.
        /// </summary>
        /// <returns>The train with fixed calls.</returns>
        public Train WithFixedSingleCallTrain()
        {
            if (train.Calls.Count == 1)
            {
                var departure = train.Calls[0];
                departure.Track.Calls.Remove(departure);
                var arrival = new StationCall(departure.Id, departure.Track, departure.Arrival, departure.Arrival);
                departure = new StationCall(departure.Id + 1, departure.Track, departure.Departure, departure.Departure);
                train.Calls.Clear();
                train.Add(arrival);
                train.Add(departure);
            }
            return train;
        }

        /// <summary>
        /// Sets the first call as departure only and the last call as arrival only.
        /// </summary>
        /// <returns>The train with adjusted call flags.</returns>
        public Train WithFirstCallDepartureOnlyAndLastCallArrivalOnly()
        {
            train.SetFirstCallDepartureOnly();
            train.SetLastCallArrivalOnly();
            return train;
        }

        /// <summary>
        /// Sets the first call to be departure only (no arrival).
        /// </summary>
        public void SetFirstCallDepartureOnly() => train.Calls.First().IsArrival = false;

        /// <summary>
        /// Sets the last call to be arrival only (no departure).
        /// </summary>
        public void SetLastCallArrivalOnly() => train.Calls.Last().IsDeparture = false;
    }

    /// <summary>
    /// Determines whether the train is null or has no station calls.
    /// </summary>
    /// <param name="train">The train to check.</param>
    /// <returns><c>true</c> if the train is null or has no calls; otherwise, <c>false</c>.</returns>
    public static bool IsNullOrHasNoCalls([NotNullWhen(false)] this Train? train)
        => train is null || train.Calls.Count == 0;
}
