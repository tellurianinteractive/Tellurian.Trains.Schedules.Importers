namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>The kind of a module drawing, derived from its number of exits (ModuleDrawing §3).</summary>
public enum ModuleKind
{
    /// <summary>No exits; a warning.</summary>
    Invalid,

    /// <summary>One exit: a terminus, staging track or loop module.</summary>
    EndOfLine,

    /// <summary>Two exits.</summary>
    PassThrough,

    /// <summary>Three or more exits.</summary>
    Junction,
}
