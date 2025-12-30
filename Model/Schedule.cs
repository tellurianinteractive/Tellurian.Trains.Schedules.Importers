namespace Tellurian.Trains.Schedules.Model;

public class Schedule : IEquatable<Schedule>
{
    // Private parameterless constructor for EF Core
    private Schedule()
    {
        Name = string.Empty;
        Timetable = default!;
        LocoSchedules = [];
        TrainsetSchedules = [];
        DriverDuties = [];
    }

    public Schedule(string name, Timetable timetable)
    {
        Name = name;
        Timetable = timetable;
        TimetableId = timetable.Id;
        LocoSchedules = [];
        TrainsetSchedules = [];
        DriverDuties = [];
    }

    public static Schedule Create(string name, Timetable timetable) =>
        new(name, timetable);

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // FK property for EF Core
    public int TimetableId { get; set; }
    public Timetable Timetable { get; set; }

    public ICollection<LocoSchedule> LocoSchedules { get; set; }
    public ICollection<TrainsetSchedule> TrainsetSchedules { get; set; }
    public ICollection<DriverDuty> DriverDuties { get; set; }

    public bool Equals(Schedule? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is Schedule other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Name;
}

public static class ScheduleExtensions
{
    public static VehicleSchedule AddLocoSchedule(this Schedule me, LocoSchedule locoSchedule)
    {
        me = me.ValueOrException(nameof(me));
        locoSchedule = locoSchedule.ValueOrException(nameof(locoSchedule));
        if (!me.LocoSchedules.Contains(locoSchedule))
        {
            locoSchedule.Schedule = me;
            locoSchedule.ScheduleId = me.Id;
            me.LocoSchedules.Add(locoSchedule);
        }
        return locoSchedule;
    }

    public static VehicleSchedule AddTrainsetSchedule(this Schedule me, TrainsetSchedule trainsetSchedule)
    {
        me = me.ValueOrException(nameof(me));
        trainsetSchedule = trainsetSchedule.ValueOrException(nameof(trainsetSchedule));
        if (!me.TrainsetSchedules.Contains(trainsetSchedule))
        {
            trainsetSchedule.Schedule = me;
            trainsetSchedule.ScheduleId = me.Id;
            me.TrainsetSchedules.Add(trainsetSchedule);
        }
        return trainsetSchedule;
    }

    public static DriverDuty AddDriverDuty(this Schedule schedule, DriverDuty driverDuty)
    {
        schedule = schedule.ValueOrException(nameof(schedule));
        driverDuty = driverDuty.ValueOrException(nameof(driverDuty));
        if (!schedule.DriverDuties.Contains(driverDuty))
        {
            driverDuty.Schedule = schedule;
            driverDuty.ScheduleId = schedule.Id;
            schedule.DriverDuties.Add(driverDuty);
        }
        return driverDuty;
    }
}
