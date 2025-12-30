namespace Tellurian.Trains.Schedules.Model;

public abstract class VehicleSchedule : IEquatable<VehicleSchedule>
{
    // Protected parameterless constructor for EF Core
    protected VehicleSchedule()
    {
        Parts = [];
    }

    protected VehicleSchedule(Company? company, int number, string? remark = null)
    {
        Company = company;
        CompanyId = company?.Id;
        Number = number;
        Remark = remark;
        Parts = [];
    }

    public int Id { get; set; }
    public int Number { get; set; }
    public Sessions Sessions { get; set; } = Sessions.All;

    public string Class { get; set; } = string.Empty;

    // FK property for EF Core
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    // FK property for EF Core - owning Schedule
    public int? ScheduleId { get; set; }
    public Schedule? Schedule { get; set; }

    public string? Remark { get; set; }
    public ICollection<TrainPart> Parts { get; set; }

    public bool Equals(VehicleSchedule? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is VehicleSchedule other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"{Company?.Signature} {Number}".Trim();
}

public sealed class LocoSchedule : VehicleSchedule
{
    // Private parameterless constructor for EF Core
    private LocoSchedule() : base() { }

    public LocoSchedule(int number, string? remark = null) : this(null, number, remark) { }
    public LocoSchedule(Company? company, int number, string? remark = null) : base(company, number, remark) { }
}

public sealed class TrainsetSchedule : VehicleSchedule
{
    // Private parameterless constructor for EF Core
    private TrainsetSchedule() : base() { }

    public TrainsetSchedule(int number, string? remark = null) : this(null, number, remark) { }
    public TrainsetSchedule(Company? company, int number, string? remark = null) : base(company, number, remark) { }
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
