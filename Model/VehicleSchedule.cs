using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

public sealed class VehicleSchedule : IEquatable<VehicleSchedule>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private VehicleSchedule()
    {
        Parts = [];
    }

    public VehicleSchedule(int id)
    {
        Id = id;
        Parts = [];
    }

    public int Id { get; set; }

    // FK property for EF Core - owning Schedule (required)
    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = default!;

    public ICollection<TrainPart> Parts { get; set; }

    public bool Equals(VehicleSchedule? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is VehicleSchedule other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"VehicleSchedule {Id}";
}

public static class VehicleScheduleExtensions
{
    public static TrainPart? Add(this VehicleSchedule me, TrainPart? part)
    {
        if (me == null || part is null) throw new ArgumentNullException(nameof(part));
        part.Schedule = me;
        part.ScheduleId = me.Id;
        if (!me.Parts.Contains(part))
        {
            me.Parts.Add(part);
        }
        return part;
    }
}
