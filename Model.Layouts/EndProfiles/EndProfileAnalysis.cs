using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>Derives properties from an end profile drawing, including symmetry (spec EP-7).</summary>
public static class EndProfileAnalysis
{
    /// <summary>Derives the properties of <paramref name="profile"/>; <paramref name="tolerances"/> default to <see cref="FitTolerances.Default"/>.</summary>
    public static EndProfileProperties Analyze(EndProfile profile, FitTolerances? tolerances = null)
    {
        tolerances ??= FitTolerances.Default;
        var offsets = profile.Tracks.Select(t => t.Offset).Order().ToList();
        return new()
        {
            PlateBounds = profile.Outline?.ToPath().Bounds ?? BoundingBox.Empty,
            TrackOffsets = offsets,
            TrackCentreDistances = [.. offsets.Skip(1).Select((offset, i) => offset - offsets[i])],
            ScrewHolePositions = [.. profile.ScrewHoles.Select(h => h.Center)],
            AsymmetricElements = AsymmetricElements(profile, tolerances),
        };
    }

    /// <summary>
    /// The width of the end plate, as in <see cref="Analyze"/> but without its symmetry check, which compares the
    /// outline and ground line with their mirror images and is far slower.
    /// </summary>
    public static double PlateWidth(EndProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return (profile.Outline?.ToPath().Bounds ?? BoundingBox.Empty).Width;
    }

    /// <summary>The elements of <paramref name="profile"/> that differ from their mirror image.</summary>
    public static FitDifferences AsymmetricElements(EndProfile profile, FitTolerances? tolerances = null)
    {
        tolerances ??= FitTolerances.Default;
        var mirrored = profile.MirroredX();
        var result = FitDifferences.None;
        if (EndProfileComparison.CompareTracks(profile.Tracks, mirrored.Tracks, tolerances.Joint) != FitDifferences.None)
            result |= FitDifferences.TrackPositions;
        if (!EndProfileComparison.MatchCircles(profile.ScrewHoles, mirrored.ScrewHoles, tolerances.Joint).IsComplete)
            result |= FitDifferences.ScrewHoles;
        if (!EndProfileComparison.MatchCircles(profile.Dowels, mirrored.Dowels, tolerances.Joint).IsComplete)
            result |= FitDifferences.Dowels;
        if (profile.GroundLine is not null && mirrored.GroundLine is not null &&
            !ShapeComparison.IsWithin(profile.GroundLine.Tessellate(), mirrored.GroundLine.Tessellate(), tolerances.Landscape, tolerances.Landscape / 4))
            result |= FitDifferences.GroundLine;
        if (profile.Outline is not null && mirrored.Outline is not null &&
            !ShapeComparison.IsWithin(profile.Outline.Tessellate(), mirrored.Outline.Tessellate(), tolerances.Joint, tolerances.Joint / 2))
            result |= FitDifferences.Outline;
        return result;
    }
}
