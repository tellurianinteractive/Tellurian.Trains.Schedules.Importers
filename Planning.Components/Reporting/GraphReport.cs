using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// Everything a printed graphical timetable needs, worked out before anything is rendered: the print scale, how
/// tall each stretch is on paper, how the time axis divides into sheets, and which stretches share each sheet.
/// The report page itself is then only a view over this.
/// <para>
/// It lives outside the component so the whole pipeline — measure, squeeze, slice, pack — can be run against a
/// real plan in a test and checked against the paper, which is the only way to know that what is laid out here
/// actually fits what comes out of the printer.
/// </para>
/// </summary>
public sealed class GraphReport
{
    /// <summary>
    /// How far the minimum station spacing may be squeezed for a stretch that is otherwise taller than a sheet.
    /// Below this the track fan-out and the station labels collide whatever the scale, so a stretch that still
    /// does not fit is printed slightly clipped rather than shrunk into illegibility.
    /// </summary>
    public const double MinimumStationSpacingMm = 5;

    private readonly Timetable? _timetable;
    private readonly LayoutSettings _settings;
    private readonly int _longestSignature;
    private readonly Dictionary<double, GraphSettings> _byStationSpacing = [];

    /// <summary>A report with nothing to print, for when no plan is loaded.</summary>
    public static GraphReport Empty { get; } = new(null, new LayoutSettings(), 1);

    private GraphReport(Timetable? timetable, LayoutSettings settings, int longestSignature)
    {
        _timetable = timetable;
        _settings = settings;
        _longestSignature = longestSignature;
        Pages = [];
    }

    /// <summary>The sheets to print, in order.</summary>
    public IReadOnlyList<GraphPage> Pages { get; private set; }

    /// <summary>Whether the graphs have a vertical time axis, and so print on A4 portrait stacked sideways
    /// rather than on A4 landscape stacked downward.</summary>
    public bool IsVertical { get; private set; }

    /// <summary>The print-scaled settings the report's own measurements were made with.</summary>
    public GraphSettings PrintSettings { get; private set; } = GraphSettings.Default;

    /// <summary>Whether train lines are printed black rather than in their category colour.</summary>
    public bool Monochrome => _settings.GraphicTimetable.PrintMonochrome;

    /// <summary>Builds the report. Returns a report with no pages when there is nothing to print.</summary>
    public static GraphReport Create(Timetable timetable, IReadOnlyList<TimetableStretch> stretches, LayoutSettings settings, GraphPageGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(timetable);
        ArgumentNullException.ThrowIfNull(stretches);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(geometry);

        if (stretches.Count == 0) return new GraphReport(timetable, settings, 1);

        // One gutter for the whole report, sized from the longest signature printed anywhere in it, so every
        // sheet holds the same number of minutes and the sheets of one stretch line up with the next stretch's.
        var longestSignature = stretches
            .SelectMany(stretch => stretch.Stations)
            .Max(station => station.Signature?.Length ?? 0);

        var report = new GraphReport(timetable, settings, longestSignature);
        var print = report.SettingsFor(settings.GraphicTimetable.PrintStationSpacingMm);

        var items = stretches.Select(stretch => report.Fit(stretch, geometry)).ToList();

        // A stretch running past midnight widens the axis to the whole day for every sheet, so the graphs
        // stacked on a sheet keep sharing one time axis.
        var crossesMidnight = GraphSchedule.CrossesMidnight(timetable.Trains.SelectMany(train => train.Calls));
        var (start, end) = GraphSchedule.TimeWindow(print, crossesMidnight, GraphHalf.Whole);

        report.PrintSettings = print;
        report.IsVertical = print.AxisDirection == TimeAxisDirection.Vertical;
        report.Pages = GraphPaginator.BuildPages(items, start, end, settings.General.BreakTime, print.MinutesPerPage(geometry), geometry);
        return report;
    }

    /// <summary>The graph to render for one stretch on one sheet: the stretch drawn at its own print scale,
    /// over that sheet's slice of the time axis.</summary>
    public GraphSchedule Graph(GraphPageItem item, GraphPage page)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(page);
        if (_timetable is null) throw new InvalidOperationException("An empty report has no graphs to draw.");
        return new GraphSchedule(item.Stretch, _timetable, SettingsFor(item.StationSpacingMm), GraphHalf.Whole, (page.From, page.To));
    }

    /// <summary>
    /// Measures a stretch across the time axis and, when it is taller than a sheet, squeezes its minimum station
    /// spacing millimetre by millimetre until it fits. A graph cannot be split along the distance axis, and only
    /// this legibility floor is reduced — never the millimetres-per-kilometre scale — so the distances stay
    /// comparable with every other sheet.
    /// </summary>
    private GraphPageItem Fit(TimetableStretch stretch, GraphPageGeometry geometry)
    {
        var available = geometry.UsableCrossLengthMm - geometry.HeadingHeightMm;
        var spacing = _settings.GraphicTimetable.PrintStationSpacingMm;
        var cross = Measure(stretch, spacing);
        while (cross > available && spacing > MinimumStationSpacingMm)
        {
            spacing = Math.Max(MinimumStationSpacingMm, spacing - 1);
            cross = Measure(stretch, spacing);
        }
        return new GraphPageItem(stretch, cross, spacing);
    }

    private double Measure(TimetableStretch stretch, double spacingMm) =>
        new GraphSchedule(stretch, _timetable!, SettingsFor(spacingMm)).CrossAxisLengthMm();

    // One GraphSettings per minimum station spacing in use: all stretches normally share the setting's value,
    // and only a stretch too tall for a sheet gets its own.
    private GraphSettings SettingsFor(double spacingMm)
    {
        if (_byStationSpacing.TryGetValue(spacingMm, out var cached)) return cached;
        var built = _settings.ToPrintGraphSettings(_longestSignature, spacingMm);
        _byStationSpacing[spacingMm] = built;
        return built;
    }
}
