namespace Tellurian.Trains.Schedules.Model.Drawings.Tracks;

/// <summary>The kind of a <see cref="Turnout"/>: which routes it has and which way they go.</summary>
public enum TurnoutKind
{
    /// <summary>Straight route and a route diverging to the left.</summary>
    Left,

    /// <summary>Straight route and a route diverging to the right.</summary>
    Right,

    /// <summary>Two routes diverging symmetrically, each by half the angle.</summary>
    Wye,

    /// <summary>Straight route and routes diverging to the left and the right.</summary>
    ThreeWay,

    /// <summary>Both routes curve to the left, the inner one more sharply.</summary>
    CurvedLeft,

    /// <summary>Both routes curve to the right, the inner one more sharply.</summary>
    CurvedRight,
}
