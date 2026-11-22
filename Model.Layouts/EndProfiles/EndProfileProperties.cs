using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>Properties derived from an end profile drawing (spec EP-7).</summary>
public sealed record EndProfileProperties
{
    /// <summary>The bounds of the end-plate outline; empty when there is no outline.</summary>
    public required BoundingBox PlateBounds { get; init; }

    /// <summary>The width of the end plate, mm.</summary>
    public double PlateWidth => PlateBounds.Width;
    /// <summary>The height of the end plate, mm.</summary>
    public double PlateHeight => PlateBounds.Height;

    /// <summary>The track offsets from left to right.</summary>
    public required IReadOnlyList<double> TrackOffsets { get; init; }

    /// <summary>The centre distances (c/c) between neighbouring tracks, from left to right.</summary>
    public required IReadOnlyList<double> TrackCentreDistances { get; init; }

    /// <summary>The centres of the screw holes.</summary>
    public required IReadOnlyList<Point> ScrewHolePositions { get; init; }

    /// <summary>The elements that differ from their mirror image; <see cref="FitDifferences.None"/> when symmetric.</summary>
    public required FitDifferences AsymmetricElements { get; init; }

    /// <summary>The number of tracks.</summary>
    public int TrackCount => TrackOffsets.Count;

    /// <summary>
    /// True when the elements that decide fit (tracks, screw holes, ground line) equal their mirror image.
    /// Then the profile orientation at an exit doesn't matter.
    /// </summary>
    public bool IsSymmetric => (AsymmetricElements & ~(FitDifferences.Outline | FitDifferences.Dowels)) == FitDifferences.None;
}
