using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>
/// A room where a hall plan is built, in hall coordinates: millimetres, X to the right, Y up. A document of its own,
/// with a <see cref="Guid"/>, written in hall files and embedded in the hall plans that use it.
/// </summary>
/// <remarks>
/// A mutable class like the rest of the shared model (decision 20 of <c>CrossAppReview.md</c>): an app that keeps
/// earlier states copies it with <see cref="Copy()"/> or <c>With(change)</c>. It owns its obstacles, doors and no-go
/// zones, which have <see cref="int"/> ids unique among their kind in the hall (decision 10).
/// </remarks>
public sealed class Hall : LayoutPart
{
    /// <summary>The identity of the hall, the same in every file it is written to.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Guid Id { get; set; }

    /// <summary>What the hall is called.</summary>
    public required string Name { get; set; }

    /// <summary>Where the hall is.</summary>
    public string? Address { get; set; }

    /// <summary>A note for the reader.</summary>
    public string? Note { get; set; }

    /// <summary>
    /// The walls as a closed path; wall <c>i</c> is step <c>i</c>. Change them with <see cref="SetOutline"/> to keep
    /// the doors in the walls.
    /// </summary>
    public required PathDrawing Outline { get; set; }

    /// <summary>The minimum aisle width, mm; null uses the minimum clearance of the hall plan's standard.</summary>
    public double? MinimumAisleWidth { get; set; }

    /// <summary>Pillars and fixed furniture.</summary>
    public List<Obstacle> Obstacles { get; set; } = [];

    /// <summary>Doors and emergency exits.</summary>
    public List<Door> Doors { get; set; } = [];

    /// <summary>Areas where nothing may be placed.</summary>
    public List<NoGoZone> NoGoZones { get; set; } = [];

    /// <summary>Counted up each time the hall is saved as a new revision.</summary>
    public int Revision { get; set; }

    /// <summary>Who saved the drawing, and the last date each of them did, newest first.</summary>
    public List<DrawingRevision> RevisionLog { get; set; } = [];

    /// <summary>What other systems keep about the hall, with one entry per system; read and written back unchanged.</summary>
    public Dictionary<string, JsonElement>? ExternalReferences { get; set; }

    /// <summary>The number of walls.</summary>
    [JsonIgnore]
    public int WallCount => Outline.Steps.Count;

    /// <summary>A rectangular hall of <paramref name="width"/> × <paramref name="depth"/> with a corner at the origin.</summary>
    public static Hall Rectangle(string name, double width, double depth, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        Outline = PathDrawing.Rectangle(width, depth, Transform.Identity),
    };

    /// <summary>A copy with copies of what the hall owns: its lists, obstacles, doors, no-go zones and external references.</summary>
    public override Hall Copy()
    {
        var copy = (Hall)base.Copy();
        copy.Obstacles = [.. Obstacles.Select(o => o.Copy())];
        copy.Doors = [.. Doors.Select(d => d.Copy())];
        copy.NoGoZones = [.. NoGoZones.Select(z => z.Copy())];
        copy.RevisionLog = [.. RevisionLog];
        copy.ExternalReferences = ExternalReferences is null ? null : new(ExternalReferences);
        return copy;
    }

    /// <summary>Adds <paramref name="obstacle"/>, giving it the next id unless it has one not in use; returns it.</summary>
    public Obstacle Add(Obstacle obstacle) => Add(Obstacles, obstacle, o => o.Id, (o, id) => o.Id = id);

    /// <summary>Adds <paramref name="door"/>, giving it the next id unless it has one not in use; returns it.</summary>
    public Door Add(Door door) => Add(Doors, door, d => d.Id, (d, id) => d.Id = id);

    /// <summary>Adds <paramref name="zone"/>, giving it the next id unless it has one not in use; returns it.</summary>
    public NoGoZone Add(NoGoZone zone) => Add(NoGoZones, zone, z => z.Id, (z, id) => z.Id = id);

    /// <summary>The obstacle with <paramref name="id"/>, if any.</summary>
    public Obstacle? FindObstacle(int id) => Obstacles.FirstOrDefault(o => o.Id == id);

    /// <summary>The geometry of wall <paramref name="index"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">There is no such wall.</exception>
    public Segment Wall(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, WallCount);
        return Outline.ToPath().Segments[index];
    }

    /// <summary>The floor area inside the walls, mm².</summary>
    [JsonIgnore]
    public double FloorArea => Math.Abs(Outline.Tessellate().SignedArea);

    /// <summary>
    /// The floor area that can be used, mm²: the floor area inside the walls without pillars, furniture, the areas kept
    /// free in front of doors and emergency exits, and no-go zones. Overlaps are counted once, and parts outside the
    /// walls don't count.
    /// </summary>
    [JsonIgnore]
    public double EffectiveFloorArea
    {
        get
        {
            var removed = Obstacles.Select(o => o.Area)
                .Concat(Doors.Select(d => d.KeepFreeZone()))
                .Concat(NoGoZones.Select(z => z.Area))
                .Select(a => a.Tessellate(Tolerance.AreaChord))
                .ToList();
            return AreaMeasure.Remaining(Outline.Tessellate(Tolerance.AreaChord), removed);
        }
    }

    /// <summary>Where a door at <paramref name="point"/> sits: the closest point on the walls, heading into the hall.</summary>
    public Pose PoseOnWall(Point point) => PoseOnWall(Outline, point);

    /// <summary>The closest point on the walls of <paramref name="outline"/> to <paramref name="point"/>, heading into the hall.</summary>
    public static Pose PoseOnWall(PathDrawing outline, Point point)
    {
        ArgumentNullException.ThrowIfNull(outline);
        var along = outline.ToPath().Project(point).Pose;
        return new(along.Position, (along.Heading + Inward(outline)).Normalized);
    }

    /// <summary>
    /// Where a door is along the walls: the wall closest to <paramref name="point"/> and the distance along it from its
    /// starting corner (corner <c>Wall</c>) to the closest point.
    /// </summary>
    public (int Wall, double Distance) WallPositionOf(Point point)
    {
        var walls = Outline.ToPath().Segments;
        var best = (Wall: 0, Distance: 0.0, Gap: double.PositiveInfinity);
        for (var i = 0; i < walls.Count; i++)
        {
            var distance = walls[i].ClosestDistance(point);
            var gap = walls[i].PointAt(distance).DistanceTo(point);
            if (gap < best.Gap) best = (i, distance, gap);
        }
        return (best.Wall, best.Distance);
    }

    /// <summary>The pose <paramref name="distance"/> along wall <paramref name="wall"/> from its starting corner, heading into the hall.</summary>
    /// <exception cref="ArgumentOutOfRangeException">There is no such wall.</exception>
    public Pose PoseOnWall(int wall, double distance)
    {
        var segment = Wall(wall);
        var pose = segment.PoseAt(Math.Clamp(distance, 0, segment.Length));
        return new(pose.Position, (pose.Heading + Inward(Outline)).Normalized);
    }

    /// <summary>Gives the hall new walls; each door moves to the closest point on them, so doors stay in a wall.</summary>
    /// <exception cref="ArgumentException"><paramref name="outline"/> isn't closed.</exception>
    public void SetOutline(PathDrawing outline)
    {
        ArgumentNullException.ThrowIfNull(outline);
        if (!outline.IsClosed) throw new ArgumentException("The walls of a hall must be a closed path.", nameof(outline));
        Outline = outline;
        foreach (var door in Doors) door.Position = PoseOnWall(outline, door.Position.Position);
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>The turn from the walls' direction into the hall: left of a counter-clockwise outline, right of a clockwise one.</summary>
    private static Angle Inward(PathDrawing outline) => outline.Tessellate().IsCounterClockwise ? Angle.Quarter : -Angle.Quarter;

    private static T Add<T>(List<T> items, T item, Func<T, int> idOf, Action<T, int> setId)
    {
        ArgumentNullException.ThrowIfNull(item);
        var id = idOf(item);
        if (id <= 0 || items.Any(i => idOf(i) == id)) setId(item, items.Count == 0 ? 1 : Math.Max(0, items.Max(idOf)) + 1);
        items.Add(item);
        return item;
    }
}
