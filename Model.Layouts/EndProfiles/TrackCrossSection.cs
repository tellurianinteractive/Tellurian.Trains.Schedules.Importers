using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>
/// How a track is drawn in end view: rails on a sleeper, standing on the track bed, the top of the end plate. Rail top
/// is at Y = 0 for every track, whatever its height, so the track bed is at −height (<see cref="EndProfile.TrackHeightOf"/>).
/// Moved from Module Draw.
/// </summary>
public static class TrackCrossSection
{
    /// <summary>The height from the track bed to rail top of a track whose end profile states none, mm.</summary>
    public const double DefaultHeight = 4;

    /// <summary>The width of a rail head relative to the gauge.</summary>
    public const double RailWidthPerGauge = 0.1;

    /// <summary>The sleeper length relative to the gauge; about 30 mm in H0, as a 2.6 m sleeper at 1:87.</summary>
    public const double SleeperLengthPerGauge = 1.8;

    /// <summary>The height of the rails of a track <paramref name="trackHeight"/> high: the upper half.</summary>
    public static double RailHeight(double trackHeight) => trackHeight / 2;

    /// <summary>The height of the sleeper of a track <paramref name="trackHeight"/> high: the lower half.</summary>
    public static double SleeperHeight(double trackHeight) => trackHeight - RailHeight(trackHeight);

    public static double RailWidth(double gauge) => gauge * RailWidthPerGauge;

    public static double SleeperLength(double gauge) => gauge * SleeperLengthPerGauge;

    /// <summary>
    /// The area a track <paramref name="height"/> high takes in end view: its underside (<see cref="EndProfileTrack.EffectiveBedWidth"/>)
    /// and rails, from where it stands on the plate to rail top.
    /// </summary>
    public static BoundingBox Bounds(EndProfileTrack track, double height)
    {
        ArgumentNullException.ThrowIfNull(track);
        var half = Math.Max(track.EffectiveBedWidth, track.WidestGauge + RailWidth(track.WidestGauge)) / 2;
        return new(new(track.Offset - half, -height), new(track.Offset + half, 0));
    }
}
