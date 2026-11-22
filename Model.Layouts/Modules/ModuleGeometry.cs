using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Modules;

/// <summary>
/// Plan-view geometry of a module drawing that depends on the exits' end profiles:
/// end plates, track positions at exits and the assembled outline.
/// </summary>
public static class ModuleGeometry
{
    /// <summary>How far apart a side's end and its end-plate corner may be, mm (NF-8).</summary>
    public const double CornerTolerance = 0.01;

    /// <summary>
    /// The plan position of a point at <paramref name="endViewX"/> on the exit's end view X axis.
    /// Seen from outside, the end view's right is the exit's left-hand side in plan.
    /// </summary>
    public static Point ToPlan(ModuleExit exit, double endViewX) => exit.Pose.Position + exit.Pose.Left * endViewX;

    /// <summary>The end plate at <paramref name="exit"/>, <paramref name="plateWidth"/> mm wide and centred on the exit.</summary>
    public static EndPlate EndPlate(ModuleExit exit, double plateWidth) =>
        new(exit, plateWidth, ToPlan(exit, -plateWidth / 2), ToPlan(exit, plateWidth / 2));

    /// <summary>The end plate at <paramref name="exit"/>, as wide as the plate of <paramref name="profile"/>.</summary>
    public static EndPlate EndPlate(ModuleExit exit, EndProfile profile) =>
        EndPlate(exit, EndProfileAnalysis.PlateWidth(profile));

    /// <summary>
    /// The track centre positions where tracks cross the exit's end plate, from the end profile as built
    /// at the exit (its orientation), with the exit's heading.
    /// </summary>
    public static IReadOnlyList<Pose> TrackPositions(ModuleExit exit, EndProfile profile) =>
        [.. profile.Oriented(exit.Orientation).Tracks
            .OrderBy(t => t.Offset)
            .Select(t => new Pose(ToPlan(exit, t.Offset), exit.Pose.Heading))];

    /// <summary>
    /// The outline of the drawing: the end plates and the sides between them, counter-clockwise.
    /// Null when the drawing has no sides.
    /// </summary>
    /// <exception cref="InvalidOperationException">A side refers to an unknown exit or doesn't meet its end-plate corners.</exception>
    /// <exception cref="KeyNotFoundException">An exit's end profile isn't available.</exception>
    public static SegmentPath? Outline(ModuleDrawing drawing, Func<Guid, EndProfile?> endProfiles)
    {
        if (drawing.Sides.Count == 0) return null;
        var plates = EndPlates(drawing, endProfiles);
        var sides = drawing.Sides.Select(s => s.Path.ToPath()).ToList();
        var segments = new List<Segment>();
        for (var i = 0; i < drawing.Sides.Count; i++)
        {
            var side = drawing.Sides[i];
            var from = Plate(drawing, plates, side.FromExit);
            var to = Plate(drawing, plates, side.ToExit);
            CheckCorner(sides[i].Start, from.RightCorner, $"Side {i} doesn't start at the right corner of exit {side.FromExit}.");
            CheckCorner(sides[i].End, to.LeftCorner, $"Side {i} doesn't end at the left corner of exit {side.ToExit}.");

            var previousEnd = sides[(i + sides.Count - 1) % sides.Count].End;
            segments.Add(new LineSegment(previousEnd, sides[i].Start));
            segments.AddRange(sides[i].Segments);
        }
        return new SegmentPath(segments, isClosed: true);
    }

    /// <summary>The end plates of all exits of the drawing, by exit label.</summary>
    /// <exception cref="KeyNotFoundException">An exit's end profile isn't available.</exception>
    public static IReadOnlyDictionary<string, EndPlate> EndPlates(ModuleDrawing drawing, Func<Guid, EndProfile?> endProfiles) =>
        drawing.Exits.ToDictionary(
            e => e.Label,
            e => EndPlate(e, endProfiles(e.EndProfile.Id) ?? throw new KeyNotFoundException($"End profile {e.EndProfile.Name} ({e.EndProfile.Id}) of exit {e.Label} isn't available.")));

    private static EndPlate Plate(ModuleDrawing drawing, IReadOnlyDictionary<string, EndPlate> plates, ExitPlace place) =>
        drawing.Exit(place) is { } exit && plates.TryGetValue(exit.Label, out var plate) ? plate : throw new InvalidOperationException($"A side refers to the exit {place}, which the module doesn't have.");

    private static void CheckCorner(Point actual, Point expected, string message)
    {
        if (!actual.IsCloseTo(expected, CornerTolerance)) throw new InvalidOperationException($"{message} Gap {actual.DistanceTo(expected):0.###} mm.");
    }
}
