namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>Where the straight part of a straight + curve module is, seen from the first exit.</summary>
public enum StraightPosition
{
    /// <summary>Before the curve.</summary>
    Before,
    /// <summary>After the curve.</summary>
    After,

    /// <summary>Half before and half after the curve.</summary>
    Split,
}
