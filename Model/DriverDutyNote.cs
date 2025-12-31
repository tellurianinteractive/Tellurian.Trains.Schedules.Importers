using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

public class DriverDutyNote : IEquatable<DriverDutyNote>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private DriverDutyNote() => Text = string.Empty;

    public DriverDutyNote(string text)
    {
        Text = text;
    }

    public int Id { get; set; }
    public string Text { get; set; }

    // FK property for EF Core
    public int DriverDutyId { get; set; }
    public DriverDuty DriverDuty { get; set; } = default!;

    public bool Equals(DriverDutyNote? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is DriverDutyNote other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Text;
}
