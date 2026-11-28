using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.Halls;

/// <summary>A pillar or fixed furniture in a hall. Modules may not overlap it.</summary>
public sealed class Obstacle : LayoutPart
{
    /// <summary>
    /// The identity of the obstacle, unique among the hall's obstacles; given by <see cref="Hall.Add(Obstacle)"/>. A hall
    /// plan refers to an obstacle by it, for the dimensions that place a layout.
    /// </summary>
    public int Id { get; set; }

    /// <summary>What the obstacle is.</summary>
    public ObstacleKind Kind { get; set; }

    /// <summary>A closed path in hall coordinates, mm.</summary>
    public required PathDrawing Area { get; set; }

    /// <summary>A label shown in the drawing, e.g. "P1".</summary>
    public string? Label { get; set; }

    /// <summary>A round pillar with its centre at <paramref name="center"/>, mm.</summary>
    public static Obstacle RoundPillar(Point center, double diameter) =>
        new() { Kind = ObstacleKind.Pillar, Area = PathDrawing.Circle(center, diameter) };

    /// <summary>A rectangular pillar with its centre at <paramref name="center"/>, its width turned <paramref name="rotation"/> from the X axis, mm.</summary>
    public static Obstacle RectangularPillar(Point center, double width, double depth, Angle rotation = default) =>
        new() { Kind = ObstacleKind.Pillar, Area = PathDrawing.Rectangle(center, width, depth, rotation) };

    /// <summary>The centre and diameter if the area is a circle, as made by <see cref="RoundPillar"/>.</summary>
    public bool TryGetCircle(out Point center, out double diameter)
    {
        center = default;
        diameter = 0;
        if (!Area.IsClosed || Area.Steps.Count == 0 || Area.Steps[0] is not ArcStep first) return false;
        if (Area.Steps.Any(s => s is not ArcStep a || !a.Center.IsCloseTo(first.Center))) return false;
        if (Math.Abs(Math.Abs(Area.Steps.Sum(s => ((ArcStep)s).Sweep)) - 2 * Angle.Half.Degrees) > Tolerance.Angle) return false;
        center = first.Center;
        diameter = 2 * Area.Start.DistanceTo(center);
        return true;
    }

    /// <summary>
    /// The centre, size and rotation if the area is a rectangle, as made by <see cref="RectangularPillar"/>: the
    /// width runs along the first side, turned <paramref name="rotation"/> from the X axis.
    /// </summary>
    public bool TryGetRectangle(out Point center, out double width, out double depth, out Angle rotation) =>
        Area.TryGetRectangle(out center, out width, out depth, out rotation);

    /// <inheritdoc/>
    public override Obstacle Copy() => (Obstacle)base.Copy();

    /// <inheritdoc/>
    public override string ToString() => Label ?? Kind.ToString();
}
