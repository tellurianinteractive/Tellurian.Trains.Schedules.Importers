using Microsoft.AspNetCore.Components;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

public static class OperationLocationGraphExtensions
{
    public static MarkupString NameLabel(this OperationLocation me) => new(me.Signature);

    public static MarkupString KmLabel(this OperationLocation me, TimetableStretch stretch)
    {
        var distance = stretch.DisplayedDistanceToStation(me);
        return new($"{distance:F0} km");
    }
}
