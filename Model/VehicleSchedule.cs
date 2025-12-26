namespace Tellurian.Trains.Schedules.Model;

public abstract record VehicleSchedule
{
    public int Id { get; init; }
    public int Number { get; init; }
    public Sessions Sessions { get; set; } = Sessions.All;

    public string Class { get; init; } = string.Empty;
    public OperatingCompany Company { get; init; } = OperatingCompany.None;
    public string? Remark { get; init; }
    public ICollection<TrainPart> Parts { get; }

    protected VehicleSchedule(OperatingCompany company, int number, string? remark = null)
    {
        Company = company;
        Number = number;
        Remark = remark;
        Parts = [];
    }

    public override string ToString() => $"{Company.Signature} {Number}";
}

public sealed record LocoSchedule : VehicleSchedule
{
    public LocoSchedule(int number, string? remark = null) : this(OperatingCompany.None, number, remark) { }
    public LocoSchedule(OperatingCompany company, int number, string? remark = null) : base(company, number, remark) { }
}
public sealed record TrainsetSchedule : VehicleSchedule
{
    public TrainsetSchedule(int number, string? remark = null) : this(OperatingCompany.None, number, remark) { }

    public TrainsetSchedule(OperatingCompany company, int number, string? remark = null) : base(company, number, remark) { }
}

public static class VehicleScheduleExtensions
{
    public static TrainPart? Add(this VehicleSchedule me, TrainPart? part)
    {
        if (me == null || part is null) throw new ArgumentNullException(nameof(part));
        part.Schedule = me;
        if (!me.Parts.Contains(part))
        {
            me.Parts.Add(part);
        }
        return part;
    }
}
