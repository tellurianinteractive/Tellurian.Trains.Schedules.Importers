namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

public class TrainPartWagonsetData
{
    public required DriverDuty Duty { get; init; }
    public IEnumerable<ScheduledTrainPart> Parts { get; init; } = [];

}

public class TrainPartWagonset
{
    public required ScheduledTrainPart TrainPart { get; init; }
}


