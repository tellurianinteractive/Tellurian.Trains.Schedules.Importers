using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.Components.Scheduling;
using Tellurian.Utilities.Web;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Allocation;

/// <summary>
/// One train part on a vehicle's line: the box of its running time, with the preparation before it and the
/// finishing-up after it, which are drawn grey.
/// </summary>
/// <param name="Identity">The train as the Trains tab names it: category prefix, number and suffix.</param>
/// <param name="Color">The train category's colour, which fills the running box.</param>
/// <param name="TextColor">A text colour that reads on <paramref name="Color"/>.</param>
/// <param name="WorkFrom">Where the vehicle is first tied up: the preparation starts here at the train's origin.</param>
/// <param name="RunFrom">The departure.</param>
/// <param name="RunTo">The arrival.</param>
/// <param name="WorkTo">Where the vehicle is free again: the finishing-up ends here at the train's destination.</param>
/// <param name="Title">The part written out, for a tooltip on screen.</param>
public sealed record AllocationBar(
    string Identity,
    string Color,
    string TextColor,
    TimeSpan WorkFrom,
    TimeSpan RunFrom,
    TimeSpan RunTo,
    TimeSpan WorkTo,
    string Title)
{
    /// <summary>The minute the running starts at, as the two digits the graph writes a departure with.</summary>
    public string StartMinute => $"{RunFrom.Minutes:00}";

    /// <summary>The minute the running ends at.</summary>
    public string EndMinute => $"{RunTo.Minutes:00}";

    /// <summary>The same bar a number of whole days earlier, which keeps its minutes.</summary>
    internal AllocationBar ShiftedBack(int days)
    {
        var shift = TimeSpan.FromDays(days);
        return this with { WorkFrom = WorkFrom - shift, RunFrom = RunFrom - shift, RunTo = RunTo - shift, WorkTo = WorkTo - shift };
    }
}

/// <summary>
/// One line of the report: a vehicle on the sessions it works one set of train parts, labelled as the Schedules
/// tab labels it. A vehicle that works different parts on different sessions has a line for each set.
/// </summary>
/// <param name="Vehicle">The vehicle.</param>
/// <param name="Role">The place in the train the schedules give the vehicle; <see cref="TractionRole.Front"/> is not said.</param>
/// <param name="Sessions">The sessions or days this line is for.</param>
/// <param name="Lanes">The bars, in as many lanes as it takes to keep bars that overlap in time apart. Overlapping bars are
/// a scheduling conflict, and showing them side by side is how the report lets it be seen.</param>
public sealed record AllocationRow(
    ScheduledObject Vehicle,
    TractionRole Role,
    Sessions Sessions,
    IReadOnlyList<IReadOnlyList<AllocationBar>> Lanes);

/// <summary>The vehicles of one company, or those of none.</summary>
public sealed record AllocationGroup(Company? Company, IReadOnlyList<AllocationRow> Rows);

/// <summary>One sheet: a span of the time axis and the lines that fit under it.</summary>
public sealed record AllocationPage(TimeSpan From, TimeSpan To, bool IsContinued, IReadOnlyList<AllocationGroup> Groups);

/// <summary>
/// Paper geometry for the vehicle allocation report. A4 landscape, time along the long side. Millimetres, and
/// deliberately plain arithmetic, so pagination is testable and never measures the page. The heights are pinned
/// in <c>VehicleAllocationReport.razor.css</c> and must be changed together with the ones here: a constant that
/// no longer matches does not print short, it prints a row past the foot of the sheet.
/// </summary>
public sealed record AllocationGeometry
{
    /// <summary>The sheet the graphical timetable is printed on, whose margins and time axis this report shares.</summary>
    public GraphPageGeometry Page { get; init; } = GraphPageGeometry.A4;

    /// <summary>Width of the first column, with the vehicle labels.</summary>
    public double LabelWidthMm { get; init; } = 48;

    /// <summary>The heading above the sheet: the report, and the span of time it covers.</summary>
    public double HeadingHeightMm { get; init; } = 7;

    /// <summary>The row with the hour labels, repeated on every sheet.</summary>
    public double AxisHeightMm { get; init; } = 5;

    /// <summary>The header above the vehicles of one company.</summary>
    public double GroupHeaderHeightMm { get; init; } = 6;

    /// <summary>Height of one lane of boxes.</summary>
    public double LaneHeightMm { get; init; } = 6;

    /// <summary>What a line adds to its lanes: the space between it and the next.</summary>
    public double RowPaddingMm { get; init; } = 1;

    /// <summary>Length of the time axis, once the label column and the margins are taken.</summary>
    public double PlotWidthMm => Page.UsableTimeLengthMm - LabelWidthMm;

    /// <summary>What a sheet holds of lines and headers, once the heading and the hour labels are taken.</summary>
    public double AvailableHeightMm => Page.UsableCrossLengthMm - HeadingHeightMm - AxisHeightMm;

    /// <summary>The height of a line.</summary>
    public double RowHeightMm(AllocationRow row) => (Math.Max(1, row.Lanes.Count) * LaneHeightMm) + RowPaddingMm;

    /// <summary>Default A4 landscape geometry.</summary>
    public static AllocationGeometry A4 { get; } = new();
}

/// <summary>
/// The vehicle allocation report, worked out before anything is rendered: for each vehicle the train parts it works,
/// laid out on a time scale, divided into sheets the way the graphical timetable is.
/// </summary>
/// <remarks>
/// <para>
/// The time axis takes everything from the graphical timetable's print settings: the millimetres to the hour, the
/// operating window (the whole day when a train runs past midnight) and the break time. The break time cuts the
/// window in two, and a sheet never straddles it. The graphical timetable's orientation is not followed: the
/// lines are rows, so time always runs along the sheet.
/// </para>
/// <para>
/// It lives outside the component so the whole of it can be run against a real plan in a test.
/// </para>
/// </remarks>
public sealed class VehicleAllocation
{
    private static readonly TimeSpan OneDay = TimeSpan.FromHours(24);

    private VehicleAllocation(IReadOnlyList<AllocationPage> pages, double millimetresPerMinute, bool hasCompanies)
    {
        Pages = pages;
        MillimetresPerMinute = millimetresPerMinute;
        HasCompanies = hasCompanies;
    }

    /// <summary>The sheets to print, in order.</summary>
    public IReadOnlyList<AllocationPage> Pages { get; }

    /// <summary>The print scale of the time axis.</summary>
    public double MillimetresPerMinute { get; }

    /// <summary>Whether any vehicle has a company. Only then do the lines come under company headers.</summary>
    public bool HasCompanies { get; }

    /// <summary>A report with nothing to print.</summary>
    public static VehicleAllocation Empty { get; } = new([], 1, false);

    /// <summary>Builds the report. Returns a report with no sheets when no vehicle works a train.</summary>
    /// <param name="vehicles">The plan's vehicles.</param>
    /// <param name="settings">The layout's settings.</param>
    /// <param name="geometry">The paper.</param>
    public static VehicleAllocation Create(IEnumerable<ScheduledObject> vehicles, LayoutSettings settings, AllocationGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(vehicles);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(geometry);

        // The same identity the Schedules tab writes on its boxes, whatever the graph is set to draw: the boxes
        // have to be recognisable against the Trains tab, which always lists trains with prefix and suffix.
        var graph = settings.ToGraphSettings() with { ShowTrainCategory = true, ShowCompany = false, HideSessionsOrDays = true };
        var general = settings.General;

        var candidates = vehicles
            .Where(vehicle => vehicle.HasTurnusCard)
            .SelectMany(vehicle => vehicle.SessionCombinations(general.UseDays, general.MaxSessions)
                .Select(combination => (Vehicle: vehicle, Combination: combination)))
            .ToList();

        var crossesMidnight = candidates.Any(c => c.Combination.Parts.Any(p => Bar(p, graph).WorkTo >= OneDay));
        var rows = candidates
            .Select(c => new AllocationRow(
                c.Vehicle,
                RoleOf(c.Vehicle, c.Combination),
                c.Combination.Sessions,
                Lanes(c.Combination.Parts.SelectMany(part => Placed(Bar(part, graph), crossesMidnight)))))
            .ToList();
        if (rows.Count == 0) return Empty;

        var groups = rows
            .GroupBy(row => row.Vehicle.Company)
            .OrderBy(group => group.Key is null)
            .ThenBy(group => group.Key?.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(group => new AllocationGroup(
                group.Key,
                [.. group
                    .OrderBy(row => row.Vehicle.ObjectType)
                    .ThenBy(row => row.Vehicle.Class, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(row => row.Vehicle.Number)
                    .ThenBy(row => row.Vehicle.ExternalId, StringComparer.CurrentCultureIgnoreCase)]))
            .ToList();

        var millimetresPerMinute = Math.Max(0.01, settings.GraphicTimetable.PrintHourSpacingMm) / 60.0;
        var minutesPerPage = Math.Max(1, (int)Math.Floor(geometry.PlotWidthMm / millimetresPerMinute));
        var (start, end) = GraphSchedule.TimeWindow(graph, crossesMidnight, GraphHalf.Whole);

        var pages = new List<AllocationPage>();
        if (end > start)
        {
            foreach (var (from, to, isContinued) in GraphPaginator.TimeSlices(start, end, settings.General.BreakTime, minutesPerPage, geometry.Page.SeamOverlapMinutes))
                foreach (var onSheet in Pack(groups, geometry))
                    pages.Add(new AllocationPage(from, to, isContinued, onSheet));
        }
        return new VehicleAllocation(pages, millimetresPerMinute, groups.Any(group => group.Company is not null));
    }

    // The part as a bar. A shunting task is worked at one call whose times read the other way round, which is
    // what StartTime and EndTime already allow for; the working span only ever reaches further out than the run.
    private static AllocationBar Bar(ScheduledTrainPart part, GraphSettings graph)
    {
        var (workFrom, workTo) = part.WorkingSpan;
        var runFrom = part.StartTime.Value;
        var runTo = part.EndTime.Value;
        var color = part.Train.Category?.Color ?? "#000000";
        var identity = part.Train.GraphLabel(graph);
        return new AllocationBar(
            identity,
            color,
            color.TextColor,
            workFrom.Value < runFrom ? workFrom.Value : runFrom,
            runFrom,
            runTo,
            workTo.Value > runTo ? workTo.Value : runTo,
            $"{identity} {part.From.OperationLocation.Signature} {part.StartTime.HHMM()} – {part.To.OperationLocation.Signature} {part.EndTime.HHMM()}");
    }

    // A time past midnight is drawn at the start of the day, as the graphical timetable draws it. A bar that
    // runs across midnight is drawn at both ends of the axis.
    private static IEnumerable<AllocationBar> Placed(AllocationBar bar, bool crossesMidnight)
    {
        if (!crossesMidnight)
        {
            yield return bar;
            yield break;
        }
        var days = (int)Math.Floor(bar.WorkFrom / OneDay);
        var shifted = days > 0 ? bar.ShiftedBack(days) : bar;
        yield return shifted;
        if (shifted.WorkTo > OneDay) yield return shifted.ShiftedBack(1);
    }

    // As few lanes as keep the bars apart, filled from the earliest.
    private static IReadOnlyList<IReadOnlyList<AllocationBar>> Lanes(IEnumerable<AllocationBar> bars)
    {
        var lanes = new List<List<AllocationBar>>();
        foreach (var bar in bars.OrderBy(b => b.WorkFrom).ThenBy(b => b.RunFrom))
        {
            var lane = lanes.FirstOrDefault(l => l[^1].WorkTo <= bar.WorkFrom);
            if (lane is null) lanes.Add(lane = []);
            lane.Add(bar);
        }
        return [.. lanes.Select(l => (IReadOnlyList<AllocationBar>)l)];
    }

    // The role the schedules that hold these parts give the vehicle. Front is the ordinary one and goes unsaid,
    // so any other role is what the vehicle's label should carry.
    private static TractionRole RoleOf(ScheduledObject vehicle, SessionCombination combination) =>
        vehicle.ScheduleAssignments
            .Where(assignment => assignment.Schedule.Parts.Any(combination.Parts.Contains))
            .Select(assignment => assignment.Role)
            .FirstOrDefault(role => role != TractionRole.Front);

    // Divides the lines into sheets. A company that goes on over the page repeats its header on the next, so a
    // sheet torn out of the pile still says whose vehicles it shows.
    private static IEnumerable<IReadOnlyList<AllocationGroup>> Pack(IReadOnlyList<AllocationGroup> groups, AllocationGeometry geometry)
    {
        var available = geometry.AvailableHeightMm;
        var sheet = new List<AllocationGroup>();
        var used = 0.0;

        foreach (var group in groups)
        {
            var rows = new List<AllocationRow>();
            foreach (var row in group.Rows)
            {
                var needed = (rows.Count == 0 ? geometry.GroupHeaderHeightMm : 0) + geometry.RowHeightMm(row);
                if (used + needed > available && (rows.Count > 0 || sheet.Count > 0))
                {
                    if (rows.Count > 0) sheet.Add(group with { Rows = rows });
                    yield return sheet;
                    sheet = [];
                    rows = [];
                    used = 0;
                    needed = geometry.GroupHeaderHeightMm + geometry.RowHeightMm(row);
                }
                rows.Add(row);
                used += needed;
            }
            if (rows.Count > 0) sheet.Add(group with { Rows = rows });
        }

        if (sheet.Count > 0) yield return sheet;
    }
}
