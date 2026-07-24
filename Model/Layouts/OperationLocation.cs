using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Represents a railway station or other operation location where trains can stop or pass.
/// </summary>
/// <remarks>
/// The location type governs whether a train may stop at all, on top of the per-call
/// <see cref="Timetables.StationCall.IsStop"/> flags:
/// <list type="bullet">
/// <item><see cref="Station"/> — a train stops when the call says so (<see cref="Timetables.StationCall.IsStop"/>).</item>
/// <item><see cref="OtherLocation"/> — a train may stop when the call says so (e.g. an unstaffed halt with passenger exchange).</item>
/// <item><see cref="SignalControlledLocation"/> — a train <b>never</b> stops; it always passes through, whatever the call flags say.</item>
/// </list>
/// So the effective rule is: the train stops at a call when
/// <c>call.IsStop &amp;&amp; call.Station is not SignalControlledLocation</c>.
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Station), "Station")]
[JsonDerivedType(typeof(SignalControlledLocation), "SignalControlled")]
[JsonDerivedType(typeof(IndustrialArea), "IndustrialArea")]
[JsonDerivedType(typeof(OtherLocation), "Other")]
public abstract class OperationLocation : IEquatable<OperationLocation>
{
    /// <summary>
    /// Gets or sets the foreign key to the associated layout.
    /// </summary>
    public int LayoutId { get; set; }

    /// <summary>
    /// Gets or sets the layout this station belongs to.
    /// </summary>
    public Layout Layout { get; set; } = default!;

    /// <summary>
    /// Gets or sets the unique identifier for this operation location.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this operation location.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the short signature or abbreviation for this operation location.
    /// </summary>
    public string Signature { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Country.Id"/> of the country this operation location is in, or
    /// <c>null</c> when not specified. A new location defaults to the layout's default country
    /// (see <c>IdentitySettings.DefaultCountryId</c>); the country is resolved through the layout's
    /// saved catalogue (see <c>Layout.CountryById</c>). Not set by the XPLN import, which has no such
    /// concept.
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Gets or sets the module owner of this operation location. Used in FREMO meetings
    /// where stations are owned by the members who bring the modules. Optional.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Gets or sets the contact phone number for the station operator. Optional.
    /// </summary>
    public int? PhoneNumber { get; set; }

    /// <summary>
    /// TODO: Reevaluate this property. This could instead indicae a <see cref="SignalControlledLocation"/>.
    /// </summary>
    public bool IsSignal { get; set; }

    /// <summary>
    /// Supress creating notes about trains meeding at this location.
    /// </summary>
    public bool HideMeets { get; set; }

    /// <summary>
    /// Supresses the display of trains not stopping at this location (used in some reports).
    /// </summary>
    public virtual bool HidePassings { get; set; }

    /// <summary>
    /// Gets whether a train can change direction at this location,
    /// which typically requires a loco runaround.
    /// Always false for <see cref="OtherLocation"/>; configurable for
    /// <see cref="Station"/> and <see cref="SignalControlledLocation"/>.
    /// </summary>
    public virtual bool IsChangingTrainDirectionPossible { get; set; }

    /// <summary>
    /// Gets whether passenger trains can stop here to exchange passengers, i.e. tickets can be issued
    /// from and to this location. Always false for <see cref="SignalControlledLocation"/>; configurable
    /// for <see cref="Station"/> and <see cref="OtherLocation"/>.
    /// </summary>
    public virtual bool HasPassengerExchange { get; set; } = true;

    /// <summary>
    /// Gets whether freight trains can exchange cargo wagons here. Always false for
    /// <see cref="SignalControlledLocation"/> and <see cref="OtherLocation"/>; configurable for
    /// <see cref="Station"/>.
    /// </summary>
    public virtual bool HasCargoExchange { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of tracks at this operation location.
    /// </summary>
    public ICollection<StationTrack> Tracks { get; set; }

    /// <summary>
    /// Gets or sets the per-station operational time overrides.
    /// Null-valued members inherit the layout-wide defaults from
    /// <see cref="LayoutSettings.TimeAndSpeed"/>.
    /// </summary>
    public StationTimings Timings { get; set; } = new();

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected OperationLocation()
    {
        Name = string.Empty;
        Signature = string.Empty;
        Tracks = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="OperationLocation"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the operation location.</param>
    /// <param name="name">The name of the operation location.</param>
    /// <param name="signature">The short signature or abbreviation.</param>
    public OperationLocation(int id, string name, string signature)
    {
        Id = id;
        name = name.ValueOrException(nameof(name), string.Format(CultureInfo.CurrentCulture, Strings.NameOfObjectIsRequired, Strings.Station.ToLowerInvariant()));
        Name = name.Replace("_", " ", StringComparison.OrdinalIgnoreCase);
        Signature = signature.ValueOrException(nameof(signature), string.Format(CultureInfo.CurrentCulture, Strings.SignatureOfStationIsRequired));
        Tracks = [];
    }

    /// <summary>
    /// Gets the track with the specified number.
    /// </summary>
    /// <param name="number">The track number to find.</param>
    /// <returns>The station track with the specified number.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no track with the specified number exists.</exception>
    public StationTrack this[string number] => Tracks.SingleOrDefault(t => t.Number == number) ?? throw new InvalidOperationException($"Station {Name} has no track '{number}'");

    /// <inheritdoc/>
    public bool Equals(OperationLocation? other) => Signature.Equals(other?.Signature, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is OperationLocation other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Signature.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>
/// Provides extension methods for <see cref="OperationLocation"/>.
/// </summary>
public static class OperationLocationExtensions
{
    extension(OperationLocation? operationLocation)
    {
        /// <summary>
        /// Gets an example operation location for testing purposes.
        /// </summary>
        public static Station Example => new(1, "Ytterby", "Yb");

        /// <summary>
        /// Gets all trains that call at this station.
        /// </summary>
        /// <returns>A collection of distinct trains calling at this station.</returns>
        public IEnumerable<Train> Trains() =>
            operationLocation is null ? [] :
            operationLocation.Calls()
                .Where(c => c.Train.HasValue)
                .Select(c => c.Train!)
                .Distinct();

        /// <summary>
        /// Gets all station calls at this operation location.
        /// </summary>
        /// <returns>A collection of all station calls at this location.</returns>
        public IEnumerable<StationCall> Calls() =>
            operationLocation is null ? [] :
            operationLocation.Tracks.SelectMany(t => t.Calls);

        /// <summary>
        /// Finds a track by its number.
        /// </summary>
        /// <param name="number">The track number to find.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the track if found.</returns>
        public Maybe<StationTrack> Track(string number) =>
            new(operationLocation?.Tracks.SingleOrDefault(t => t.Number == number),
                string.Format(CultureInfo.CurrentCulture, Strings.StationHasNotTrackNumber, operationLocation?.Name, number));

        /// <summary>
        /// Determines whether the station has a track with the specified number.
        /// </summary>
        /// <param name="number">The track number to look for.</param>
        /// <returns><c>true</c> if a track with the number exists; otherwise, <c>false</c>.</returns>
        public bool HasTrack(string number)
            => operationLocation?.Tracks.Any(t => t.Number == number) ?? false;

        /// <summary>
        /// Adds a track to the station.
        /// </summary>
        /// <param name="stationTrack">The track to add.</param>
        /// <returns>The added track.</returns>
        public StationTrack Add(StationTrack stationTrack)
        {
            stationTrack = stationTrack.ValueOrException(nameof(stationTrack));
            ArgumentNullException.ThrowIfNull(stationTrack);
            operationLocation = operationLocation.ValueOrException(nameof(operationLocation));
            stationTrack.Station = operationLocation;
            stationTrack.StationId = operationLocation.Id;
            operationLocation.Tracks.Add(stationTrack);
            return stationTrack;
        }

        /// <summary>
        /// Decides whether trains can meet on an <see cref="Station"/>
        /// </summary>
        public bool CanHaveTrainsMeets =>
            operationLocation is Station station &&
            station.Tracks.Count(t => t.IsScheduled) > 1;
    }
}
