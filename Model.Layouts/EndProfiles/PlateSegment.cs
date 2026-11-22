using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>
/// A segment of a plate's upper edge, as an end profile drawing gives it: a line to a point, an arc around a
/// centre to a point, or a fillet with only its radius (EP-3). Stored in end profile files as entered.
/// </summary>
public sealed record PlateSegment : ExtensibleRecord
{
    /// <summary>Whether the segment is a line, an arc or a fillet.</summary>
    public required PlateSegmentKind Kind { get; init; }

    /// <summary>The end point of a line or arc; a fillet's end is worked out.</summary>
    public Point? End { get; init; }

    /// <summary>The centre of an arc.</summary>
    public Point? Center { get; init; }

    /// <summary>The radius of a fillet.</summary>
    public double? Radius { get; init; }

    /// <summary>The side of the edge a fillet's centre is on; when not given, the side that fits the next segment best.</summary>
    public FilletSide? Side { get; init; }

    /// <summary>The radius: a fillet's, or an arc's distance from its centre to its end.</summary>
    [JsonIgnore]
    public double EffectiveRadius => Kind switch
    {
        PlateSegmentKind.Fillet => Radius ?? 0,
        PlateSegmentKind.Arc when Center is { } center && End is { } end => center.DistanceTo(end),
        _ => 0,
    };

    /// <summary>A straight line to <paramref name="end"/>.</summary>
    public static PlateSegment Line(Point end) => new() { Kind = PlateSegmentKind.Line, End = end };

    /// <summary>An arc around <paramref name="center"/> to <paramref name="end"/>.</summary>
    public static PlateSegment Arc(Point center, Point end) => new() { Kind = PlateSegmentKind.Arc, End = end, Center = center };

    /// <summary>A fillet of <paramref name="radius"/>, with its centre on <paramref name="side"/> of the edge if given.</summary>
    public static PlateSegment Fillet(double radius, FilletSide? side = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);
        return new() { Kind = PlateSegmentKind.Fillet, Radius = radius, Side = side };
    }

    /// <summary>The same segment moved by <paramref name="offset"/>; a fillet has nothing to move.</summary>
    public PlateSegment Moved(Vector offset) => this with
    {
        End = End is { } end ? end + offset : null,
        Center = Center is { } center ? center + offset : null,
    };

    /// <summary>The same kind of segment with another end point; a fillet is unchanged.</summary>
    public PlateSegment WithEnd(Point end) => Kind == PlateSegmentKind.Fillet ? this : this with { End = end };
}
