namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>The outcome of an auto-draw, from best to worst.</summary>
public enum AutoDrawStatus
{
    /// <summary>The module is fully drawn.</summary>
    Complete,

    /// <summary>Drawn as far as possible, but unusual; the user should check it (loops, turntables, 360°).</summary>
    Check,

    /// <summary>Drawn, but parts must be completed by hand (junction exits, tracks that couldn't be generated).</summary>
    NeedsManualCompletion,

    /// <summary>Not drawn: the parameters don't describe the module (no geometry, unknown standard).</summary>
    NeedsManualDrawing,
}
