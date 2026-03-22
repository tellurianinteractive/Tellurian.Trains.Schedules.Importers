using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Layouts;

/// <summary>
/// The direction of travel along a <see cref="TrackStretch"/>.
/// Forward means travelling from <see cref="TrackStretch.Start"/> to <see cref="TrackStretch.End"/>;
/// Backward means travelling from <see cref="TrackStretch.End"/> to <see cref="TrackStretch.Start"/>.
/// </summary>
public enum TrainPathDirection
{
    Forward,
    Backward
}

/// <summary>
/// A single segment of a train path, representing travel along a track stretch in a specific direction.
/// </summary>
/// <param name="TrackStretch">The track stretch being traversed.</param>
/// <param name="Direction">The direction of travel along the stretch.</param>
public record TrainPathSegment(TrackStretch TrackStretch, TrainPathDirection Direction)
{
    /// <summary>
    /// The location where this segment begins.
    /// </summary>
    public OperationLocation From => Direction == TrainPathDirection.Forward
        ? TrackStretch.Start
        : TrackStretch.End;

    /// <summary>
    /// The location where this segment ends.
    /// </summary>
    public OperationLocation To => Direction == TrainPathDirection.Forward
        ? TrackStretch.End
        : TrackStretch.Start;

    /// <summary>
    /// The distance in metres for this segment.
    /// </summary>
    public double Distance => TrackStretch.Distance;
}

/// <summary>
/// A complete path through the layout, consisting of an ordered sequence of <see cref="TrainPathSegment"/>s.
/// </summary>
public class TrainPath
{
    private readonly List<TrainPathSegment> _segments;

    internal TrainPath(List<TrainPathSegment> segments)
    {
        _segments = segments;
    }

    /// <summary>
    /// The ordered segments making up this path.
    /// </summary>
    public IReadOnlyList<TrainPathSegment> Segments => _segments;

    /// <summary>
    /// The origin of the path.
    /// </summary>
    public OperationLocation From => _segments[0].From;

    /// <summary>
    /// The destination of the path.
    /// </summary>
    public OperationLocation To => _segments[^1].To;

    /// <summary>
    /// The total distance of the path in metres.
    /// </summary>
    public double TotalDistance => _segments.Sum(s => s.Distance);

    /// <summary>
    /// All locations visited along the path, in order.
    /// </summary>
    public IEnumerable<OperationLocation> Locations
    {
        get
        {
            yield return _segments[0].From;
            foreach (var segment in _segments)
            {
                yield return segment.To;
            }
        }
    }

    /// <summary>
    /// The locations where the direction of travel changes.
    /// </summary>
    public IEnumerable<OperationLocation> DirectionChanges
    {
        get
        {
            for (var i = 1; i < _segments.Count; i++)
            {
                if (_segments[i].Direction != _segments[i - 1].Direction)
                {
                    yield return _segments[i].From;
                }
            }
        }
    }
}
