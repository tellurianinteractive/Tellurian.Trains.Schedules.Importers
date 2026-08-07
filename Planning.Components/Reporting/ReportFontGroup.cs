namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// How a report font is filed in the list offered to the planner. The group also decides which generic
/// family a report falls back to when the chosen font is missing on the machine it is printed from.
/// </summary>
public enum ReportFontGroup
{
    /// <summary>
    /// A railway or transport signage face. Listed first because it is what this application is for:
    /// a timetable set in the lettering of the railway it belongs to. They are all grotesques, so they
    /// fall back to sans-serif like the group below.
    /// </summary>
    Railway,

    /// <summary>A face without serifs. The default group for anything unrecognised.</summary>
    SansSerif,

    /// <summary>A face with serifs.</summary>
    Serif,

    /// <summary>A face whose characters all take the same width.</summary>
    Monospace
}
