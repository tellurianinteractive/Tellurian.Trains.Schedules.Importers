namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

public class DriverDutyPart
{
    public required ScheduledTrainPart TrainPart { get; init; }
    public required DriverDuty Duty { get; init; }
}

public static class DriverDutyExtensions
{
    extension(DriverDutyPart dutyPart)
    {
        public TrainPartTractionData TractionData => // TODO: Implement mapping from DriverDutyPart to TrainPartTractionData
            throw new NotImplementedException();
    }
}
