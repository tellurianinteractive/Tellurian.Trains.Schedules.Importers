namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>
/// Stored on an auto-drawn drawing (ModuleDrawing §8.7). While it is there, the drawing can be
/// re-generated from changed parameters; editing the drawing by hand removes it.
/// </summary>
public sealed record AutoDrawRecord : ExtensibleRecord
{
    /// <summary>The version of auto-draw that generated the drawing (<see cref="ModuleAutoDraw.GeneratorVersion"/>).</summary>
    public required string GeneratorVersion { get; init; }
    /// <summary>The parameters the drawing was generated from.</summary>
    public required AutoDrawParameters Parameters { get; init; }
}
