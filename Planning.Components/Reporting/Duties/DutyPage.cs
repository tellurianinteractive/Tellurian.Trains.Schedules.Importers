namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

/// <summary>The four kinds of page a duty booklet is built from.</summary>
public enum DutyPageKind
{
    /// <summary>Padding, so the overview page falls last and the count is a multiple of four.</summary>
    Blank,

    /// <summary>Page 1: identifies the booklet and carries the duty notes.</summary>
    Front,

    /// <summary>One or more train parts, in working order.</summary>
    Part,

    /// <summary>The layout topology and shunting yards — always the last page.</summary>
    Overview,
}

/// <summary>
/// One A5 page of a duty booklet, numbered as the driver reads it.
/// </summary>
/// <remarks>
/// These are <em>logical</em> pages, 1…N in reading order. Imposition reorders them into the physical
/// sequence the printer needs as a separate final step, which is what keeps page building testable
/// without a browser.
/// </remarks>
public sealed class DutyPage
{
    private DutyPage(int pageNumber, DutyPageKind kind, DriverDuty? duty)
    {
        PageNumber = pageNumber;
        Kind = kind;
        Duty = duty;
    }

    /// <summary>The page's position in the booklet, from 1.</summary>
    public int PageNumber { get; }

    /// <summary>What the page holds.</summary>
    public DutyPageKind Kind { get; }

    /// <summary>The duty this page belongs to. Null only on a booklet-less blank.</summary>
    public DriverDuty? Duty { get; }

    /// <summary>The train parts on this page, in working order. Empty except on a part page.</summary>
    public List<DriverDutyPart> Parts { get; } = [];

    /// <summary>
    /// When true this page shows only the timetable of <see cref="Parts"/>' single part, whose header and
    /// tables are on the facing page. Set on the second page of a split part.
    /// </summary>
    public bool IsTimetableContinuation { get; private init; }

    /// <summary>
    /// When true the part on this page continues onto the next, so no continuation marker is printed:
    /// the remainder is already in view on the facing page, and there is nothing to warn about.
    /// </summary>
    public bool IsSplitFirstHalf { get; private init; }

    /// <summary>Creates a blank padding page.</summary>
    public static DutyPage Blank(int number, DriverDuty? duty = null) => new(number, DutyPageKind.Blank, duty);

    /// <summary>Creates the booklet's front page.</summary>
    public static DutyPage Front(int number, DriverDuty duty) => new(number, DutyPageKind.Front, duty);

    /// <summary>Creates the layout overview page.</summary>
    public static DutyPage Overview(int number, DriverDuty duty) => new(number, DutyPageKind.Overview, duty);

    /// <summary>Creates a page holding one or more whole train parts.</summary>
    public static DutyPage Part(int number, DriverDuty duty, IEnumerable<DriverDutyPart> parts)
    {
        var page = new DutyPage(number, DutyPageKind.Part, duty);
        page.Parts.AddRange(parts);
        return page;
    }

    /// <summary>Creates the first page of a split part: its header and the three tables.</summary>
    public static DutyPage SplitTables(int number, DriverDuty duty, DriverDutyPart part)
    {
        var page = new DutyPage(number, DutyPageKind.Part, duty) { IsSplitFirstHalf = true };
        page.Parts.Add(part);
        return page;
    }

    /// <summary>Creates the facing page of a split part: its train timetable alone.</summary>
    public static DutyPage SplitTimetable(int number, DriverDuty duty, DriverDutyPart part)
    {
        var page = new DutyPage(number, DutyPageKind.Part, duty) { IsTimetableContinuation = true };
        page.Parts.Add(part);
        return page;
    }
}
