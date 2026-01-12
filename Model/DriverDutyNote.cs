using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a note or instruction associated with a driver duty.
/// </summary>
public class DriverDutyNote : IEquatable<DriverDutyNote>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private DriverDutyNote() => Text = string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="DriverDutyNote"/> with the specified text.
    /// </summary>
    /// <param name="text">The note text.</param>
    public DriverDutyNote(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Gets or sets the unique identifier for this note.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the text content of this note.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the driver duty.
    /// </summary>
    public int DriverDutyId { get; set; }

    /// <summary>
    /// Gets or sets the driver duty this note belongs to.
    /// </summary>
    public DriverDuty DriverDuty { get; set; } = default!;

    /// <inheritdoc/>
    public bool Equals(DriverDutyNote? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DriverDutyNote other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Text;
}
