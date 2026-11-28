using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.Tracks;

/// <summary>
/// A turnout with its points at <see cref="Start"/>, heading into the turnout.
/// A diverging route is an arc of <see cref="Radius"/> turning <see cref="Angle"/>, followed by a
/// straight that ends level with the straight route (<see cref="Length"/>), if there is room for one.
/// A curved turnout has an outer route (<see cref="Radius"/>, <see cref="Angle"/>) and an inner
/// route (<see cref="InnerRadius"/>, <see cref="InnerAngle"/>).
/// </summary>
public sealed class Turnout : TrackElement
{
    /// <summary>The points of the turnout, in module coordinates, heading into the turnout.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Pose Start { get; set; }
    /// <summary>Which routes the turnout has.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required TurnoutKind Kind { get; set; }

    /// <summary>Length of the straight route, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Length { get; set; }

    /// <summary>Radius of the diverging route, or of the outer route of a curved turnout, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Radius { get; set; }

    /// <summary>
    /// Angle of the diverging route (degrees, positive). For a wye, the angle between its two routes;
    /// for a curved turnout, the angle of the outer route.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Angle { get; set; }

    /// <summary>Radius of the inner route of a curved turnout, mm; required for curved turnouts, otherwise unused.</summary>
    public double? InnerRadius { get; set; }
    /// <summary>Angle of the inner route of a curved turnout, degrees (positive); required for curved turnouts, otherwise unused.</summary>
    public double? InnerAngle { get; set; }

    /// <inheritdoc/>
    public override IReadOnlyList<SegmentPath> Routes() => Kind switch
    {
        TurnoutKind.Left => [Straight(), Diverging(Angle)],
        TurnoutKind.Right => [Straight(), Diverging(-Angle)],
        TurnoutKind.Wye => [Diverging(Angle / 2), Diverging(-Angle / 2)],
        TurnoutKind.ThreeWay => [Straight(), Diverging(Angle), Diverging(-Angle)],
        TurnoutKind.CurvedLeft => [Curve(Radius, Angle), Curve(Inner(nameof(InnerRadius), InnerRadius), Inner(nameof(InnerAngle), InnerAngle))],
        TurnoutKind.CurvedRight => [Curve(Radius, -Angle), Curve(Inner(nameof(InnerRadius), InnerRadius), -Inner(nameof(InnerAngle), InnerAngle))],
        _ => throw new NotSupportedException($"Unknown turnout kind {Kind}."),
    };

    /// <inheritdoc/>
    public override IReadOnlyList<Pose> ConnectionPoints() => [Start.Reversed(), .. Routes().Select(r => r.EndPose)];

    private SegmentPath Straight() => new PathBuilder(Start).Straight(Length).Build();

    private SegmentPath Curve(double radius, double angle) => new PathBuilder(Start).Curve(radius, angle).Build();

    private SegmentPath Diverging(double angle)
    {
        var builder = new PathBuilder(Start).Curve(Radius, angle);
        var radians = Math.Abs(angle) * Math.PI / 180;
        var extension = (Length - Radius * Math.Sin(radians)) / Math.Cos(radians);
        return builder.Straight(Math.Max(0, extension)).Build();
    }

    private static double Inner(string name, double? value) =>
        value ?? throw new InvalidOperationException($"A curved turnout needs {name}.");
}
