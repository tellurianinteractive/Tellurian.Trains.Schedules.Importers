namespace Tellurian.Trains.Schedules.Model.Dispatching;

/// <summary>Which clearance a dispatcher gives a train at a call.</summary>
public enum ClearanceKind
{
    /// <summary>A train pulling in, cleared into the station.</summary>
    Arrival,

    /// <summary>A train leaving, cleared on to the next station.</summary>
    Departure,

    /// <summary>
    /// A train running past without standing: one moment, and so one clearance rather than two.
    /// </summary>
    PassThrough,

    /// <summary>
    /// A shunting task: work at the station over a span of time rather than a movement through it, so
    /// one clearance at the time the work starts.
    /// </summary>
    ShuntingTask,
}

/// <summary>
/// A single clearance a dispatcher gives a train at one of its calls.
/// </summary>
/// <remarks>
/// A standing train has two clearances, because clearing a train <em>in</em> and clearing it <em>onward</em> are
/// separate actions taken minutes apart — and between them other trains may pass.
/// </remarks>
/// <param name="Call">The call this is a clearance of.</param>
/// <param name="Kind">Which clearance it is.</param>
/// <param name="Time">The time the clearance is given: the arrival for an arrival, otherwise the departure.</param>
/// <param name="Sessions">
/// The sessions or days the clearance is given on. A clearance after midnight belongs to the session or day after
/// the one the train started on, so these are the train's own moved on by the days its time has passed.
/// </param>
public sealed record Clearance(StationCall Call, ClearanceKind Kind, Time Time, Sessions Sessions)
{
    /// <summary>
    /// True when this is the train's only clearance at the call, so it stands for everything done with the train
    /// there rather than for one half of it.
    /// </summary>
    public bool IsSole { get; init; }

    /// <summary>
    /// True when the call is at a location the dispatcher works from afar rather than at their own station.
    /// </summary>
    public bool IsRemote { get; init; }

    /// <summary>
    /// True when the clearance is not given in the first session or day: the meeting begins later than the
    /// operating window does, and either the train does not run in the first session (it or its driver duty starts
    /// before the meeting begins), or the clearance falls after midnight, reached on the first session only by the
    /// period wrapping round. See <see cref="Schedules.FirstSessionExtensions"/>.
    /// </summary>
    public bool IsOutsideFirstSession { get; init; }

    /// <summary>
    /// The clock time of the clearance, without the day it falls on. Clearances are worked through by the clock,
    /// so this is what orders them: a clearance after midnight sorts among the early hours.
    /// </summary>
    public Time TimeOfDay => Time.FromHourAndMinute(Time.Value.Hours, Time.Value.Minutes);
}
