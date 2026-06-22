using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

/// <summary>
/// A view model that combines a <see cref="TimetableStretch"/> with trains from a <see cref="Timetable"/>
/// for rendering in the graphical schedule editor.
/// </summary>
public class GraphSchedule
{
    public GraphSchedule(TimetableStretch timetableStretch, Timetable timetable, GraphSettings? graphSettings = null, GraphHalf half = GraphHalf.Whole)
    {
        TimetableStretch = timetableStretch;
        Timetable = timetable;
        GraphSettings = graphSettings ?? GraphSettings.Default;
        Half = half;

        var stretchStations = new HashSet<OperationLocation>(timetableStretch.Stations);
        Stations = [.. timetableStretch.Stations];
        TrackStretches = [.. timetableStretch.Stretches];

        // Select trains that have at least one call at a station on this stretch.
        Trains = [.. timetable.Trains.Where(t => t.Calls.Any(c => stretchStations.Contains(c.Station)))];

        // A train may start before midnight and continue past it (calls at 24:00 or later). When any
        // call shown on this stretch runs to or past 24:00, the axis spans the full day 00:00–24:00 so
        // the after-midnight part can wrap to the start. See GraphScheduleDrawingExtensions wrapping.
        _crossesMidnight = Trains
            .SelectMany(t => t.Calls.Where(c => stretchStations.Contains(c.Station)))
            .Any(c => c.Arrival.Value >= OneDay || c.Departure.Value >= OneDay);
    }

    private static readonly TimeSpan OneDay = TimeSpan.FromHours(24);
    private readonly bool _crossesMidnight;

    public TimetableStretch TimetableStretch { get; }
    public Timetable Timetable { get; }
    public GraphSettings GraphSettings { get; }
    public GraphHalf Half { get; }
    public TimeAxisDirection AxisDirection => GraphSettings.AxisDirection;
    public string Description => TimetableStretch.FullDescription;
    public OperationLocation[] Stations { get; }
    public TrackStretch[] TrackStretches { get; }
    public Train[] Trains { get; }

    // The time axis comes from the layout's operating window (General settings), which the importer
    // initialises from the timetable's driver service span (earliest arrival = first start of service,
    // latest departure = last end of service) and the user can then override. See Timetable.OperatingWindow.
    // When the stretch has trains running past midnight, the axis is forced to the full day 00:00–24:00
    // so the after-midnight portions can wrap back to the start.
    private TimeSpan WindowStart => _crossesMidnight ? TimeSpan.Zero : GraphSettings.DefaultStartTime;
    private TimeSpan WindowEnd => _crossesMidnight ? OneDay : GraphSettings.DefaultEndTime;

    // The break splits the axis only when it is set and falls strictly inside the operating window;
    // otherwise the whole window is used regardless of the selected half.
    private TimeSpan? EffectiveBreak =>
        GraphSettings.BreakTime is { } b && b > WindowStart && b < WindowEnd ? b : null;

    /// <summary>Whether a break is configured for this schedule, so first/last half can be selected.</summary>
    public bool HasBreak => EffectiveBreak is not null;

    public TimeSpan StartTime => Half == GraphHalf.Last && EffectiveBreak is { } b ? b : WindowStart;
    public TimeSpan EndTime => Half == GraphHalf.First && EffectiveBreak is { } b ? b : WindowEnd;
}

/// <summary>
/// Represents a train's use of a stretch between two consecutive station calls.
/// </summary>
public record StretchUse(Train Train, int FromCallIndex)
{
    public StationCall From => Train.Calls[FromCallIndex];
    public StationCall To => Train.Calls[FromCallIndex + 1];
}
