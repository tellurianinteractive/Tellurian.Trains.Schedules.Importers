using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.Tracks;

/// <summary>
/// A double slip: a <see cref="Crossing"/> with two slip routes that connect the tracks.
/// Each slip route is an arc tangent to both tracks at their ends; its radius follows from the
/// length and angle: (Length / 2) / tan(Angle / 2).
/// </summary>
public sealed class DoubleSlip : TrackElement
{
    /// <summary>The middle of the double slip, where the tracks cross, with the heading of the first track; module coordinates.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Pose Center { get; set; }
    /// <summary>Length of each of the two straight tracks, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Length { get; set; }
    /// <summary>Angle of the second track relative to the first, degrees (counter-clockwise positive).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Angle { get; set; }

    /// <summary>Radius of the slip routes, mm, calculated from <see cref="Length"/> and <see cref="Angle"/>; not stored.</summary>
    [JsonIgnore]
    public double SlipRadius => Length / 2 / Math.Tan(Math.Abs(Angle) * Math.PI / 360);

    /// <inheritdoc/>
    public override IReadOnlyList<SegmentPath> Routes()
    {
        var crossing = AsCrossing();
        var second = crossing.SecondCenter;
        return
        [
            .. crossing.Routes(),
            new PathBuilder(Center.Advanced(-Length / 2)).Curve(SlipRadius, Angle).Build(),
            new PathBuilder(second.Advanced(-Length / 2)).Curve(SlipRadius, -Angle).Build(),
        ];
    }

    /// <inheritdoc/>
    public override IReadOnlyList<Pose> ConnectionPoints() => AsCrossing().ConnectionPoints();

    private Crossing AsCrossing() => new() { Center = Center, Length = Length, Angle = Angle };
}
