using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Drawings;

/// <summary>
/// A drawn path of line and arc steps, as stored in files: outlines, ground lines, sides and centre lines.
/// Each step starts where the previous one ends, so the path is connected by construction.
/// A closed path's last step ends at <see cref="Start"/>.
/// </summary>
public sealed record PathDrawing : ExtensibleRecord
{
    /// <summary>Where the first step starts.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Point Start { get; init; }
    /// <summary>The steps in order, each starting where the previous one ends.</summary>
    public required IReadOnlyList<PathStep> Steps { get; init; }
    /// <summary>True when the path ends at <see cref="Start"/>.</summary>
    public bool IsClosed { get; init; }

    /// <summary>A polyline path through <paramref name="points"/>.</summary>
    public static PathDrawing Polyline(IEnumerable<Point> points, bool isClosed = false)
    {
        var list = points.ToList();
        if (list.Count < 2) throw new ArgumentException("A path needs at least two points.", nameof(points));
        var steps = list.Skip(1).Select(p => (PathStep)new LineStep(p)).ToList();
        if (isClosed && !list[^1].IsCloseTo(list[0])) steps.Add(new LineStep(list[0]));
        return new() { Start = list[0], Steps = steps, IsClosed = isClosed };
    }

    /// <summary>
    /// A closed rectangle of <paramref name="width"/> along the X axis and <paramref name="depth"/> along Y,
    /// with its lower left corner at the origin, placed by <paramref name="transform"/>.
    /// </summary>
    public static PathDrawing Rectangle(double width, double depth, Transform transform)
    {
        if (width <= 0 || depth <= 0) throw new ArgumentOutOfRangeException(nameof(width), "A rectangle needs a positive width and depth.");
        Point[] corners = [new(0, 0), new(width, 0), new(width, depth), new(0, depth)];
        return Polyline(corners.Select(transform.Apply), isClosed: true);
    }

    /// <summary>A closed rectangle centred on <paramref name="center"/>, turned <paramref name="rotation"/>.</summary>
    public static PathDrawing Rectangle(Point center, double width, double depth, Angle rotation) =>
        Rectangle(width, depth, new Transform(Angle.Zero, new(-width / 2, -depth / 2)).Then(new(rotation, center.AsVector)));

    /// <summary>A closed circle of two half arcs.</summary>
    public static PathDrawing Circle(Point center, double diameter)
    {
        if (diameter <= 0) throw new ArgumentOutOfRangeException(nameof(diameter), "A circle needs a positive diameter.");
        return new()
        {
            Start = center + new Vector(diameter / 2, 0),
            Steps = [new ArcStep(center, Angle.Half.Degrees), new ArcStep(center, Angle.Half.Degrees)],
            IsClosed = true,
        };
    }

    /// <summary>A drawing of <paramref name="path"/>.</summary>
    public static PathDrawing From(SegmentPath path) => new()
    {
        Start = path.Start,
        Steps = [.. path.Segments.Select(PathStep.From)],
        IsClosed = path.IsClosed,
    };

    /// <summary>The geometry of the path. A closed path's last step is snapped to the start.</summary>
    public SegmentPath ToPath()
    {
        var segments = new List<Segment>(Steps.Count);
        var current = Start;
        for (var i = 0; i < Steps.Count; i++)
        {
            var segment = Steps[i].ToSegment(current);
            if (IsClosed && i == Steps.Count - 1 && segment is LineSegment && !segment.End.Equals(Start))
            {
                segment = new LineSegment(current, Start);
            }
            segments.Add(segment);
            current = segment.End;
        }
        return new(segments, IsClosed);
    }

    /// <summary>The path as points, with arcs approximated within <paramref name="chordTolerance"/>, mm.</summary>
    public Polyline Tessellate(double chordTolerance = Tolerance.Chord) => ToPath().Tessellate(chordTolerance);

    /// <summary>
    /// The centre, size and rotation if the path is a closed rectangle of four straight steps, e.g. a rectangular
    /// pillar or hall: the width runs along the first step, turned <paramref name="rotation"/> from the X axis, and the
    /// depth along the last step back to the start.
    /// </summary>
    public bool TryGetRectangle(out Point center, out double width, out double depth, out Angle rotation)
    {
        center = default;
        width = depth = 0;
        rotation = default;
        if (!IsClosed || Steps.Count != 4 || Steps.Any(s => s is not LineStep)) return false;
        var corners = ToPath().Segments.Select(s => s.Start).ToList();
        Vector along = corners[1] - corners[0], across = corners[3] - corners[0];
        if (along.Length < Tolerance.Length || across.Length < Tolerance.Length) return false;
        // A rectangle: the opposite corner closes the parallelogram and the sides are perpendicular.
        if (!corners[2].IsCloseTo(corners[0] + along + across, Tolerance.Length) || Math.Abs(along.Normalized.Dot(across.Normalized)) > Tolerance.Length) return false;
        center = Point.Lerp(corners[0], corners[2], 0.5);
        width = along.Length;
        depth = across.Length;
        rotation = along.Direction;
        return true;
    }

    /// <summary>The same path mirrored about the centre line; only for comparing end profiles.</summary>
    public PathDrawing MirroredX() => From(Mirror.AboutCentreLine(ToPath()));
}
