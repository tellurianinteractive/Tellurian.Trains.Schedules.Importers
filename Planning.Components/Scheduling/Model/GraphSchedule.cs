using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.Schedules;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

/// <summary>
/// A view model that combines a <see cref="TimetableStretch"/> with trains from a <see cref="Timetable"/>
/// for rendering in the graphical schedule editor.
/// </summary>
public class GraphSchedule
{
    public GraphSchedule(TimetableStretch timetableStretch, Timetable timetable, GraphSettings? graphSettings = null)
    {
        TimetableStretch = timetableStretch;
        Timetable = timetable;
        GraphSettings = graphSettings ?? GraphSettings.Default;

        var stretchStations = new HashSet<OperationLocation>(timetableStretch.Stations);
        Stations = [.. timetableStretch.Stations];
        TrackStretches = [.. timetableStretch.Stretches];

        // Select trains that have at least one call at a station on this stretch.
        Trains = [.. timetable.Trains.Where(t => t.Calls.Any(c => stretchStations.Contains(c.Station)))];

        StartTime = GetStartTime();
        EndTime = GetEndTime();
    }

    public TimetableStretch TimetableStretch { get; }
    public Timetable Timetable { get; }
    public GraphSettings GraphSettings { get; }
    public TimeAxisDirection AxisDirection => GraphSettings.AxisDirection;
    public string Description => TimetableStretch.GetDescription();
    public OperationLocation[] Stations { get; }
    public TrackStretch[] TrackStretches { get; }
    public Train[] Trains { get; }

    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }

    private TimeSpan GetStartTime()
    {
        if (Trains.Length > 0)
        {
            var time = Trains
                .SelectMany(t => t.Calls)
                .Select(c => c.Arrival.Value)
                .Min();
            return new TimeSpan(time.Days, time.Hours, 0, 0);
        }
        return GraphSettings.DefaultStartTime;
    }

    private TimeSpan GetEndTime()
    {
        if (Trains.Length > 0)
        {
            var time = Trains
                .SelectMany(t => t.Calls)
                .Select(c => c.Departure.Value)
                .Max();
            return new TimeSpan(time.Days, time.Hours + 1, 0, 0);
        }
        return GraphSettings.DefaultEndTime;
    }
}

/// <summary>
/// Represents a train's use of a stretch between two consecutive station calls.
/// </summary>
public record StretchUse(Train Train, int FromCallIndex)
{
    public StationCall From => Train.Calls[FromCallIndex];
    public StationCall To => Train.Calls[FromCallIndex + 1];
}
