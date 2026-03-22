using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling.Extensions;

internal static class TrainGraphExtensions
{
    public static IEnumerable<StretchUse> StretchUses(this Train train)
    {
        for (var callIndex = 0; callIndex < train.Calls.Count - 1; callIndex++)
        {
            yield return new StretchUse(train, callIndex);
        }
    }
}
