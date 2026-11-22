using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;

namespace Tellurian.Trains.Schedules.Model.Layouts.Measures;

/// <summary>Derives the measures of a module drawing from its drawing (MD-10, ModuleDrawing §5.4).</summary>
public static class ModuleMeasurement
{
    private const double PerMille = 1000;

    /// <exception cref="KeyNotFoundException">An exit's end profile isn't available.</exception>
    /// <exception cref="InvalidOperationException">The sides don't meet the end plates.</exception>
    public static ModuleMeasures Derive(ModuleDrawing drawing, Func<Guid, EndProfile?> endProfiles)
    {
        ArgumentNullException.ThrowIfNull(drawing);
        ArgumentNullException.ThrowIfNull(endProfiles);

        var main = drawing.MainPath?.ToPath();
        var lines = main?.Segments.OfType<LineSegment>().ToList() ?? [];
        var arcs = main?.Segments.OfType<ArcSegment>().ToList() ?? [];
        var (heightDifference, gradient) = Ramp(drawing.Exits, main);
        return new()
        {
            Length = main?.Length,
            Straight = main is null ? null : lines.Sum(l => l.Length),
            Angle = main is null ? null : arcs.Sum(a => Math.Abs(a.SweepDegrees)),
            Radius = arcs.Count > 0 && arcs.All(a => Math.Abs(a.Radius - arcs[0].Radius) < Tolerance.Length) ? arcs[0].Radius : null,
            Width = main is null ? null : Width(main, ModuleGeometry.Outline(drawing, endProfiles)),
            TrackLength = drawing.TrackElements.Sum(t => t.TrackLength),
            NumberOfSections = drawing.Sections.Count,
            RampHeightDifference = heightDifference,
            RampGradient = gradient,
        };
    }

    /// <summary>The vertices of a tessellated outline lie on its arcs, so the width is exact for sides concentric with the main path.</summary>
    private static double? Width(SegmentPath main, SegmentPath? outline)
    {
        if (outline is null) return null;
        var offsets = outline.Tessellate().Points.Select(p => main.Project(p).Offset).ToList();
        return offsets.Max() - offsets.Min();
    }

    private static (double? HeightDifference, double? Gradient) Ramp(IReadOnlyList<ModuleExit> exits, SegmentPath? main)
    {
        if (exits.Count < 2) return (null, null);
        var difference = exits.Max(e => e.RailTopOffset) - exits.Min(e => e.RailTopOffset);
        if (difference < Tolerance.Length) return (null, null);
        var gradient = main is null ? (double?)null : (exits[1].RailTopOffset - exits[0].RailTopOffset) / main.Length * PerMille;
        return (difference, gradient);
    }
}
