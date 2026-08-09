namespace Tellurian.Trains.Schedules.Planning.Timetables;

/// <summary>Which way a cloned train runs compared with the train it was copied from.</summary>
public enum CloneDirection
{
    /// <summary>The clone runs the same route the same way round: another working of the same service.</summary>
    Same,
    /// <summary>The clone runs the same route backwards, from the copied train's terminus to its origin.</summary>
    Opposite
}
