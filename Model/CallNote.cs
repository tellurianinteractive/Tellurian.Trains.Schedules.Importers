using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Base class for notes associated with a station call.
/// </summary>
/// <remarks>
/// Call notes provide additional information or instructions for drivers, station staff, or shunting personnel.
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TextCallNote), typeDiscriminator: "TextCallNote")]
public abstract class CallNote : IEquatable<CallNote>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CallNote"/> class.
    /// Parameterless constructor for EF Core and JSON deserialization.
    /// </summary>
    [JsonConstructor]
    protected CallNote() { }

    /// <summary>
    /// Gets or sets the unique identifier for this note.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this note is for the driver.
    /// </summary>
    public bool IsDriverNote { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this note is for station staff.
    /// </summary>
    public bool IsStationNote { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this note is for shunting personnel.
    /// </summary>
    public bool IsShuntingNote { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this note applies to the arrival.
    /// </summary>
    public bool IsForArrival { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this note applies to the departure.
    /// </summary>
    public bool IsForDeparture { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the station call.
    /// </summary>
    public int StationCallId { get; set; }

    /// <summary>
    /// Gets or sets the station call this note belongs to.
    /// </summary>
    public StationCall StationCall { get; set; } = default!;

    /// <inheritdoc/>
    public bool Equals(CallNote? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CallNote other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();
}
