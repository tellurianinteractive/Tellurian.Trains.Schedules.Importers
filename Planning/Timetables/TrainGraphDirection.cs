namespace Tellurian.Trains.Schedules.Planning.Timetables;

/// <summary>The direction a train travels along a timetable stretch in the graphical timetable.</summary>
public enum TrainGraphDirection
{
    /// <summary>Train travels from the stretch start towards the stretch end (ascending position).</summary>
    Upward,
    /// <summary>Train travels from the stretch end towards the stretch start (descending position).</summary>
    Downward
}
