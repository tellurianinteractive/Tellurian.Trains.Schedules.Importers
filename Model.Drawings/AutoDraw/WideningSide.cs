namespace Tellurian.Trains.Schedules.Model.Drawings.AutoDraw;

/// <summary>
/// The side a body wider than its end plates widens to, seen travelling from the first exit to the
/// second. The operator stands on the right, so <see cref="Left"/> is the non-operator side.
/// </summary>
public enum WideningSide
{
    /// <summary>Widens to the left, the non-operator side.</summary>
    Left,
    /// <summary>Widens to the right, the operator side.</summary>
    Right,
}
