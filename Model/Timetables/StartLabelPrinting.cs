namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Which trains get a <em>start label</em> — the folded card stood on the start track at a train's first
/// departure. Chosen on a <see cref="TrainCategory"/> (for its trains) and on a manned
/// <see cref="Layouts.Station"/> (for the trains starting there); a train gets a label only where both
/// allow it, the narrower of the two deciding (see <c>StartLabelExtensions.Narrower</c>).
/// </summary>
/// <remarks>
/// The values are ordered from least to most, so <c>None</c> is the default: nobody is given labels
/// they did not ask for, and a plan saved before start labels existed prints none.
/// </remarks>
public enum StartLabelPrinting
{
    /// <summary>No start label.</summary>
    None = 0,

    /// <summary>
    /// A start label only for the train that starts the day for a traction unit — the first train in its
    /// schedule, which is where the vehicle is put out before the meeting begins.
    /// </summary>
    First = 1,

    /// <summary>A start label for every train that starts here.</summary>
    All = 2,
}
