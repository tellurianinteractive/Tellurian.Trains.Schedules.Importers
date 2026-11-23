using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>A wall (by index) or an obstacle (by id) of the hall plan's hall, as a reference for dimensions (HP-21).</summary>
public sealed record HallElementReference : ExtensibleRecord
{
    /// <summary>What is referred to.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required HallElementKind Kind { get; init; }

    /// <summary>For <see cref="HallElementKind.Wall"/>: the index of the wall in the hall outline.</summary>
    public int? WallIndex { get; init; }

    /// <summary>For <see cref="HallElementKind.Obstacle"/>: the obstacle's <see cref="Obstacle.Id"/>.</summary>
    public int? ObstacleId { get; init; }

    /// <summary>The wall with <paramref name="index"/>.</summary>
    public static HallElementReference Wall(int index) => new() { Kind = HallElementKind.Wall, WallIndex = index };

    /// <summary>The obstacle with <paramref name="id"/>.</summary>
    public static HallElementReference Obstacle(int id) => new() { Kind = HallElementKind.Obstacle, ObstacleId = id };
}
