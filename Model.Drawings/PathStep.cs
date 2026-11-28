using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>One step of a <see cref="PathDrawing"/>, starting where the previous step ends.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LineStep), "line")]
[JsonDerivedType(typeof(ArcStep), "arc")]
public abstract record PathStep : ExtensibleRecord
{
    /// <summary>The geometry of this step when it starts at <paramref name="start"/>.</summary>
    public abstract Segment ToSegment(Point start);

    /// <summary>The step that draws <paramref name="segment"/>; only lines and arcs are supported.</summary>
    public static PathStep From(Segment segment) => segment switch
    {
        LineSegment line => new LineStep(line.End),
        ArcSegment arc => new ArcStep(arc.Center, arc.SweepDegrees),
        _ => throw new NotSupportedException($"Unknown segment type {segment.GetType().Name}."),
    };
}
