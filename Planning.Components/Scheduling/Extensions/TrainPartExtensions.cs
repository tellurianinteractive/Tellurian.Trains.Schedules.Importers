using Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling.Extensions;

public static class TrainPartExtensions
{
    extension(ScheduledTrainPart trainPart)
    {
        public TrainPartTractionData TractionData =>
            throw new NotImplementedException(); // TODO: Implement extracting train part traction data from train part
    }
}
