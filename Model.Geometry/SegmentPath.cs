namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// A connected sequence of segments: each segment starts where the previous one ends.
/// A closed path also ends where it starts (outlines, hall walls); an open path doesn't
/// (main paths, track centre lines, ground lines).
/// </summary>
public sealed class SegmentPath
{
    /// <summary>A path of <paramref name="segments"/>, each starting where the previous one ends; a closed path must also end where it starts.</summary>
    /// <exception cref="ArgumentException">There are no segments, they aren't connected, or a closed path doesn't end where it starts.</exception>
    public SegmentPath(IEnumerable<Segment> segments, bool isClosed = false)
    {
        Segments = [.. segments];
        if (Segments.Count == 0) throw new ArgumentException("A path must have at least one segment.", nameof(segments));
        for (var i = 1; i < Segments.Count; i++)
        {
            if (!Segments[i - 1].End.IsCloseTo(Segments[i].Start, Tolerance.Length))
                throw new ArgumentException($"Segment {i} doesn't start where segment {i - 1} ends.", nameof(segments));
        }
        if (isClosed && !End.IsCloseTo(Start, Tolerance.Length))
            throw new ArgumentException("A closed path must end where it starts.", nameof(isClosed));
        IsClosed = isClosed;
    }

    /// <summary>The segments in order of travel.</summary>
    public IReadOnlyList<Segment> Segments { get; }
    /// <summary>True if the path ends where it starts and encloses an area.</summary>
    public bool IsClosed { get; }
    /// <summary>The start point of the first segment.</summary>
    public Point Start => Segments[0].Start;
    /// <summary>The end point of the last segment.</summary>
    public Point End => Segments[^1].End;
    /// <summary>The start point and direction of travel of the first segment.</summary>
    public Pose StartPose => Segments[0].StartPose;
    /// <summary>The end point and direction of travel of the last segment.</summary>
    public Pose EndPose => Segments[^1].EndPose;
    /// <summary>The total length along the path in millimetres.</summary>
    public double Length => Segments.Sum(s => s.Length);
    /// <summary>The exact bounding box of all segments.</summary>
    public BoundingBox Bounds => Segments.Aggregate(BoundingBox.Empty, (box, s) => box.Union(s.Bounds));

    /// <summary>True if the direction of travel doesn't jump at any joint between segments.</summary>
    public bool IsTangentContinuous(double angleTolerance = Tolerance.Angle)
    {
        for (var i = 1; i < Segments.Count; i++)
        {
            if (!Segments[i - 1].EndHeading.IsCloseTo(Segments[i].StartHeading, angleTolerance)) return false;
        }
        return true;
    }

    /// <summary>The pose at <paramref name="distance"/> from the start, clamped to the path.</summary>
    public Pose PoseAt(double distance)
    {
        var remaining = Math.Max(0, distance);
        foreach (var segment in Segments)
        {
            if (remaining <= segment.Length) return segment.PoseAt(remaining);
            remaining -= segment.Length;
        }
        return EndPose;
    }

    /// <summary>
    /// The open path from <paramref name="from"/> to <paramref name="to"/> along this one, with both clamped to the path
    /// (0 to <see cref="Length"/>). Parts of segments shorter than <see cref="Tolerance.Length"/> are left out.
    /// </summary>
    /// <exception cref="ArgumentException">The clamped part is shorter than <see cref="Tolerance.Length"/>, e.g. when both
    /// values are beyond the same end of the path.</exception>
    public SegmentPath Between(double from, double to)
    {
        if (to - from < Tolerance.Length) throw new ArgumentException("The part must have a length.", nameof(to));
        from = Math.Clamp(from, 0, Length);
        to = Math.Clamp(to, 0, Length);
        if (to - from < Tolerance.Length) throw new ArgumentException($"The part from {from} to {to} mm has no length within the path of {Length} mm.", nameof(to));
        var parts = new List<Segment>();
        var start = 0.0;
        foreach (var segment in Segments)
        {
            var end = start + segment.Length;
            var partFrom = Math.Max(from, start);
            var partTo = Math.Min(to, end);
            if (partTo - partFrom >= Tolerance.Length) parts.Add(segment.Between(partFrom - start, partTo - start));
            start = end;
        }
        return new(parts);
    }

    /// <summary>
    /// Projects <paramref name="point"/> onto the path: the closest point on it, the distance along the
    /// path to there, and the lateral offset of the point (positive to the left of the direction of travel).
    /// Beyond the ends of an open path the offset is measured across the end pose.
    /// </summary>
    public PathProjection Project(Point point)
    {
        var best = default(PathProjection);
        var bestDistance = double.PositiveInfinity;
        var start = 0.0;
        foreach (var segment in Segments)
        {
            var along = segment.ClosestDistance(point);
            var pose = segment.PoseAt(along);
            var distance = point.DistanceTo(pose.Position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = new(start + along, (point - pose.Position).Dot(pose.Left), pose);
            }
            start += segment.Length;
        }
        return best;
    }

    /// <summary>The same path, travelled in the opposite direction.</summary>
    public SegmentPath Reversed() => new(Segments.Reverse().Select(s => s.Reversed()), IsClosed);

    /// <summary>The path rotated and translated by <paramref name="transform"/>.</summary>
    public SegmentPath Transformed(Transform transform) => new(Segments.Select(s => s.Transformed(transform)), IsClosed);

    /// <summary>The path mirrored about the Y axis. Only for comparing end profiles.</summary>
    public SegmentPath MirroredX() => new(Segments.Select(s => s.MirroredX()), IsClosed);

    /// <summary>
    /// A parallel path at <paramref name="distance"/> to the left (negative: right).
    /// Exact for tangent-continuous paths, such as module main paths and track centre lines.
    /// </summary>
    public SegmentPath Offset(double distance)
    {
        if (!IsTangentContinuous())
            throw new InvalidOperationException("Only tangent-continuous paths can be offset exactly.");
        return new(Segments.Select(s => s.Offset(distance)), IsClosed);
    }

    /// <summary>The path as a polyline whose distance from the true path is at most <paramref name="chordTolerance"/> millimetres. A closed path gives a closed polyline without a repeated end point.</summary>
    public Polyline Tessellate(double chordTolerance = Tolerance.Chord)
    {
        var points = new List<Point> { Start };
        foreach (var segment in Segments) segment.AppendTessellation(points, chordTolerance);
        if (IsClosed) points.RemoveAt(points.Count - 1);
        return new(points, IsClosed);
    }
}
