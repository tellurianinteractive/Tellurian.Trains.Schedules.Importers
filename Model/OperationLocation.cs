using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a railway station or other operation location where trains can stop or pass.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Station), "Station")]
[JsonDerivedType(typeof(SignalControlledLocation), "SignalControlled")]
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
    ///
    /// </summary>
    public bool IsSignal { get; set; }

    /// <summary>
    /// Gets whether a train can change direction at this location,
    /// which typically requires a loco runaround.
    /// Always false for <see cref="OtherLocation"/>; configurable for
    /// <see cref="Station"/> and <see cref="SignalControlledLocation"/>.
    /// </summary>
    public virtual bool IsChangingTrainDirectionPossible { get; set; }

    /// <summary>
    /// Gets or sets the collection of tracks at this operation location.
    /// </summary>
    public ICollection<StationTrack> Tracks { get; set; }

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
        name = name.TextOrException(nameof(name), string.Format(CultureInfo.CurrentCulture, Strings.NameOfObjectIsRequired, Strings.Station.ToLowerInvariant()));
        Name = name.Replace("_", " ", StringComparison.OrdinalIgnoreCase);
        Signature = signature.TextOrException(nameof(signature), string.Format(CultureInfo.CurrentCulture, Strings.SignatureOfStationIsRequired));
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
/// An <see cref="OperationLocation"/> that is unmanned 
/// </summary>
public class SignalControlledLocation : OperationLocation
{
    /// <summary>
    /// The <see cref="Station"/> that controls this location.
    /// </summary>
    public Station? ControlledBy { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="signature"></param>
    public SignalControlledLocation(int id, string name, string signature) : base(id, name, signature) { }

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected SignalControlledLocation() : base() { }
}

/// <summary>
/// An unmanned and not signal controlled <see cref="OperationLocation"/>.
/// </summary>
public class OtherLocation : OperationLocation
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="signature"></param>
    public OtherLocation(int id, string name, string signature) : base(id, name, signature) { }

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected OtherLocation() : base() { }

    /// <summary>
    /// Always false for other locations, as they lack the infrastructure for loco runaround.
    /// </summary>
    public override bool IsChangingTrainDirectionPossible
    {
        get => false;
        set { } // Ignore attempts to set
    }
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
    }
}

