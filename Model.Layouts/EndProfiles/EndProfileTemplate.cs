using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>
/// Creates an end profile from a template (spec EP-2): a rectangular end plate with tracks,
/// screw holes and a flat ground line along the top of the plate.
/// </summary>
public static class EndProfileTemplate
{
    /// <summary>
    /// Screw holes whose rim comes closer than this to the plate's edges are left out, e.g. holes copied from a
    /// higher plate that would end up outside a lower one, mm.
    /// </summary>
    public const double MinimumScrewHoleEdgeDistance = 10;

    /// <summary>Creates the end profile; it gets a new id when <see cref="EndProfileTemplateParameters.Id"/> is null.</summary>
    public static EndProfile Create(EndProfileTemplateParameters parameters)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(parameters.PlateWidth);
        if (parameters.PlateBottom >= parameters.PlateTop)
            throw new ArgumentException("The plate bottom must be below the plate top.", nameof(parameters));

        var half = parameters.PlateWidth / 2;
        var top = parameters.PlateTop;
        var bottom = parameters.PlateBottom;
        return new()
        {
            Id = parameters.Id ?? Guid.NewGuid(),
            Designation = parameters.Designation,
            Scale = parameters.Scale,
            Standards = [.. parameters.Standards],
            Outline = PathDrawing.Polyline([new(-half, bottom), new(half, bottom), new(half, top), new(-half, top)], isClosed: true),
            GroundLine = PathDrawing.Polyline([new(-half, top), new(half, top)]),
            TrackHeight = parameters.TrackHeight,
            Tracks = [.. parameters.TrackOffsets.Select(offset => new EndProfileTrack { Offset = offset, Gauges = [parameters.Gauge], RailSystem = parameters.RailSystem })],
            ScrewHoles = [.. parameters.ScrewHoles
                .Select(p => new Circle(p, parameters.ScrewHoleDiameter))
                .Where(h => FitsInPlate(h, half, top, bottom))],
            ScrewHolePattern = parameters.ScrewHolePattern,
            Source = parameters.Source,
        };
    }

    /// <summary>True if the hole's rim is at least <see cref="MinimumScrewHoleEdgeDistance"/> inside the rectangular plate.</summary>
    public static bool FitsInPlate(Circle hole, double halfWidth, double top, double bottom)
    {
        var margin = MinimumScrewHoleEdgeDistance + hole.Diameter / 2;
        return Math.Abs(hole.Center.X) <= halfWidth - margin && hole.Center.Y <= top - margin && hole.Center.Y >= bottom + margin;
    }

    /// <summary>Offsets of <paramref name="count"/> tracks centred on the centre line, <paramref name="centreDistance"/> apart.</summary>
    public static IReadOnlyList<double> CentredOffsets(int count, double centreDistance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return [.. Enumerable.Range(0, count).Select(i => (i - (count - 1) / 2.0) * centreDistance)];
    }
}
