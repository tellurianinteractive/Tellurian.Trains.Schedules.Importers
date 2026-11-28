using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>
/// The fixed joint between two sections of the same module, along a section line (MD-4a): a polyline across the
/// module with its ends on the outline. Never offered for joining in a hall plan, and it always fits: its end
/// profiles are never compared.
/// </summary>
public sealed class InternalJoint : LayoutPart
{
    /// <summary>The label of the section on one side of the joint (<see cref="Section.Label"/>).</summary>
    public required string FirstSection { get; set; }
    /// <summary>The label of the section on the other side of the joint (<see cref="Section.Label"/>).</summary>
    public required string SecondSection { get; set; }

    /// <summary>One end of the section line, at the outline.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Point Start { get; set; }

    /// <summary>The other end of the section line, at the outline.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Point End { get; set; }

    /// <summary>
    /// The points where the section line bends between <see cref="Start"/> and <see cref="End"/>, in order; null or
    /// empty for a straight line. A section line can have any number of straight parts (MD-4).
    /// </summary>
    public IReadOnlyList<Point>? Bends { get; set; }

    /// <summary>The section line from <see cref="Start"/> through the bends to <see cref="End"/>.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyList<Point> Points => [Start, .. Bends ?? [], End];

    /// <summary>An optional, freely designed internal end profile; not checked against any standard.</summary>
    public CatalogueReference? EndProfile { get; set; }

    /// <summary>A copy with its own list of bends.</summary>
    public override InternalJoint Copy()
    {
        var copy = (InternalJoint)base.Copy();
        copy.Bends = Bends is null ? null : [.. Bends];
        return copy;
    }
}
