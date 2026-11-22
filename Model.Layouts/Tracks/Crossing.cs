using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tracks;

/// <summary>
/// A diamond crossing: two straight tracks of <see cref="Length"/> crossing at their middle,
/// <see cref="Center"/>. The second track is turned <see cref="Angle"/> degrees from the first.
/// </summary>
public sealed class Crossing : TrackElement
{
    /// <summary>The middle of the crossing, where the tracks cross, with the heading of the first track; module coordinates.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Pose Center { get; set; }
    /// <summary>Length of each of the two straight tracks, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Length { get; set; }
    /// <summary>Angle of the second track relative to the first, degrees (counter-clockwise positive).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Angle { get; set; }

    /// <inheritdoc/>
    public override IReadOnlyList<SegmentPath> Routes() => [Line(Center), Line(SecondCenter)];

    /// <inheritdoc/>
    public override IReadOnlyList<Pose> ConnectionPoints() =>
    [
        Center.Reversed().Advanced(Length / 2), Center.Advanced(Length / 2),
        SecondCenter.Reversed().Advanced(Length / 2), SecondCenter.Advanced(Length / 2),
    ];

    internal Pose SecondCenter => Center with { Heading = Center.Heading + new Angle(Angle) };

    private SegmentPath Line(Pose middle) => new PathBuilder(middle.Advanced(-Length / 2)).Straight(Length).Build();
}
