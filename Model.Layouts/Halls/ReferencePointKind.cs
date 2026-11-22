namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>Where on the anchor module its dimensions are measured from (HP-21).</summary>
public enum ReferencePointKind
{
    /// <summary>The centre of an exit's end plate.</summary>
    ExitCentre,

    /// <summary>A corner of an exit's end plate.</summary>
    PlateCorner,

    /// <summary>A point marked by the planner, in module coordinates.</summary>
    MarkedPoint,
}
