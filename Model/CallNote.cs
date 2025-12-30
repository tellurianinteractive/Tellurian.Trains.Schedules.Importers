namespace Tellurian.Trains.Schedules.Model;

public abstract class CallNote : IEquatable<CallNote>
{
    // Protected parameterless constructor for EF Core
    protected CallNote() { }

    public int Id { get; set; }
    public bool IsDriverNote { get; set; }
    public bool IsStationNote { get; set; }
    public bool IsShuntingNote { get; set; }
    public bool IsForArrival { get; set; }
    public bool IsForDeparture { get; set; }

    // FK property for EF Core
    public int StationCallId { get; set; }
    public StationCall StationCall { get; set; } = default!;

    public bool Equals(CallNote? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is CallNote other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
}
