namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>The kinds of hall elements that dimensions can refer to (HP-21).</summary>
public enum HallElementKind
{
    /// <summary>A wall, by its index in the hall's outline.</summary>
    Wall,

    /// <summary>An obstacle, by its id in the hall.</summary>
    Obstacle,
}
