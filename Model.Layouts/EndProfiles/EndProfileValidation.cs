using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>What is wrong with an end profile drawing; UIs show a text per code.</summary>
public enum EndProfileProblemCode
{
    /// <summary>
    /// Two tracks overlap. Only a dual-gauge track, such as standard and narrow gauge on three rails, may overlap
    /// another, and then they share one rail.
    /// </summary>
    TracksOverlap,

    /// <summary>Two screw holes overlap.</summary>
    ScrewHolesOverlap,

    /// <summary>A screw hole overlaps a track.</summary>
    ScrewHoleOverlapsTrack,

    /// <summary>
    /// A track doesn't stand on the end plate: the plate's top edge isn't level, at the track's underside (minus its
    /// height), across the whole width of its underside (<see cref="EndProfileTrack.EffectiveBedWidth"/>); nor at rail
    /// top, as in an outline drawn around the track. E.g. after the track's height or width was changed; how to adjust
    /// the plate is up to the one who draws it.
    /// </summary>
    TrackNotOnPlate,
}

/// <summary>
/// A problem with two parts of an end profile, by their index in <see cref="EndProfile.Tracks"/> or
/// <see cref="EndProfile.ScrewHoles"/>: two tracks, two screw holes, or a screw hole (<paramref name="First"/>) and
/// a track (<paramref name="Second"/>). A problem with one track has it as both.
/// </summary>
public sealed record EndProfileProblem(EndProfileProblemCode Code, int First, int Second);

/// <summary>Checks that the tracks and screw holes of an end profile don't overlap, and that the tracks rest on the plate.</summary>
public static class EndProfileValidation
{
    /// <summary>Validates <paramref name="profile"/>; <paramref name="tolerances"/> default to <see cref="FitTolerances.Default"/>.</summary>
    /// <returns>The problems found; empty when there are none.</returns>
    public static IReadOnlyList<EndProfileProblem> Validate(EndProfile profile, FitTolerances? tolerances = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        tolerances ??= FitTolerances.Default;
        var problems = new List<EndProfileProblem>();
        var tracks = profile.Tracks;
        var holes = profile.ScrewHoles;
        for (var i = 0; i < tracks.Count; i++)
            for (var j = i + 1; j < tracks.Count; j++)
                if (Overlap(profile.TrackBounds(tracks[i]), profile.TrackBounds(tracks[j])) && !ShareARail(tracks[i], tracks[j], tolerances.Joint))
                    problems.Add(new(EndProfileProblemCode.TracksOverlap, i, j));
        for (var i = 0; i < holes.Count; i++)
            for (var j = i + 1; j < holes.Count; j++)
                if (holes[i].Center.DistanceTo(holes[j].Center) < (holes[i].Diameter + holes[j].Diameter) / 2 - Tolerance.Length)
                    problems.Add(new(EndProfileProblemCode.ScrewHolesOverlap, i, j));
        for (var i = 0; i < holes.Count; i++)
            for (var j = 0; j < tracks.Count; j++)
                if (Overlaps(holes[i], profile.TrackBounds(tracks[j])))
                    problems.Add(new(EndProfileProblemCode.ScrewHoleOverlapsTrack, i, j));
        for (var i = 0; i < tracks.Count; i++)
            if (!StandsOnPlate(profile, tracks[i], tolerances))
                problems.Add(new(EndProfileProblemCode.TrackNotOnPlate, i, i));
        return problems;
    }

    /// <summary>
    /// Whether two tracks of different gauges form one dual-gauge track: they have one rail in common, the left or
    /// the right, and the narrower track's other rail lies between the wider track's rails.
    /// </summary>
    public static bool ShareARail(EndProfileTrack first, EndProfileTrack second, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        if (Math.Abs(first.WidestGauge - second.WidestGauge) <= tolerance) return false;
        var (firstLeft, firstRight) = Rails(first);
        var (secondLeft, secondRight) = Rails(second);
        return Math.Abs(firstLeft - secondLeft) <= tolerance || Math.Abs(firstRight - secondRight) <= tolerance;

        static (double Left, double Right) Rails(EndProfileTrack track) => (track.Offset - track.WidestGauge / 2, track.Offset + track.WidestGauge / 2);
    }

    /// <summary>
    /// Whether <paramref name="track"/>, of <paramref name="profile"/> or to be placed in it, stands on its end plate:
    /// the plate's top edge is level across the whole width of the track's underside, centred on the track, at the
    /// track's underside (minus its height), or at rail top, where the outline is drawn around the track. True when the
    /// profile has no plate yet.
    /// </summary>
    public static bool StandsOnPlate(EndProfile profile, EndProfileTrack track, FitTolerances? tolerances = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(track);
        tolerances ??= FitTolerances.Default;
        if (PlateShape.From(profile) is not { } plate) return true;
        var half = track.EffectiveBedWidth / 2;
        double left = track.Offset - half, right = track.Offset + half;
        return plate.IsLevel(left, right, -profile.TrackHeightOf(track), tolerances.Joint) || plate.IsLevel(left, right, 0, tolerances.Joint);
    }

    /// <summary>Whether the areas overlap more than touching.</summary>
    private static bool Overlap(BoundingBox a, BoundingBox b) =>
        a.Min.X < b.Max.X - Tolerance.Length && b.Min.X < a.Max.X - Tolerance.Length &&
        a.Min.Y < b.Max.Y - Tolerance.Length && b.Min.Y < a.Max.Y - Tolerance.Length;

    /// <summary>Whether the circle overlaps the area more than touching.</summary>
    private static bool Overlaps(Circle circle, BoundingBox box)
    {
        var nearest = new Point(Math.Clamp(circle.Center.X, box.Min.X, box.Max.X), Math.Clamp(circle.Center.Y, box.Min.Y, box.Max.Y));
        return circle.Center.DistanceTo(nearest) < circle.Diameter / 2 - Tolerance.Length;
    }
}
