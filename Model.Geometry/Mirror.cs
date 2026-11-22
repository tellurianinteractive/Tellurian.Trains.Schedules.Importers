namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// Mirroring about the centre line (X → −X), used <b>only</b> to compare end profiles: two facing
/// ends are each drawn as seen from outside their own module (spec §3.1). It is deliberately kept
/// apart from <see cref="Transform"/> so that a module placement can never be mirrored.
/// </summary>
public static class Mirror
{
    /// <summary>The point mirrored about the Y axis.</summary>
    public static Point AboutCentreLine(Point point) => point.MirroredX();

    /// <summary>The segment mirrored about the Y axis; the direction of travel is kept.</summary>
    public static Segment AboutCentreLine(Segment segment) => segment.MirroredX();

    /// <summary>The path mirrored about the Y axis; the direction of travel is kept.</summary>
    public static SegmentPath AboutCentreLine(SegmentPath path) => path.MirroredX();

    /// <summary>The polyline mirrored about the Y axis, keeping the point order (so a closed polyline changes its winding).</summary>
    public static Polyline AboutCentreLine(Polyline polyline) =>
        new(polyline.Points.Select(p => p.MirroredX()), polyline.IsClosed);

    /// <summary>The points mirrored about the Y axis, in the same order.</summary>
    public static IReadOnlyList<Point> AboutCentreLine(IEnumerable<Point> points) =>
        [.. points.Select(p => p.MirroredX())];
}
