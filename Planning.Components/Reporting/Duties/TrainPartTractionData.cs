namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

public class TrainPartTractionData
{
    public IEnumerable<TrainPartTractionUnit> Units { get; init; } = [];
    public required SessionsSettings SessionsSettings { get; init; }
}

public class TrainPartTractionUnit()
{
    public Sessions Sessions { get; init; }
    public required ScheduledTrainPart TrainPart { get; init; }
}

