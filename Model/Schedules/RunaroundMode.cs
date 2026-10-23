namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Whether the locomotive is run round to the other end of the train where the train arrives, so it can
/// leave the way it came.
/// </summary>
/// <remarks>
/// A runaround is almost certain wherever a train changes direction without changing locomotive and the
/// locomotive cannot reverse as it stands, so it is worked out from the route and the planner only says
/// where that is wrong. Whatever is said, a train whose traction changes direction without runaround (a
/// trainset, a push-pull train) is never told to run round.
/// </remarks>
public enum RunaroundMode
{
    /// <summary>Decided from the route: a runaround where the train leaves in the opposite direction to the one it arrived from.</summary>
    Automatic = 0,

    /// <summary>No runaround here, whatever the route says.</summary>
    Never = 1,

    /// <summary>A runaround here, even where the route does not call for one.</summary>
    Always = 2,
}
