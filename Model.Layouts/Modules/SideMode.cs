namespace Tellurian.Trains.Schedules.Model.Layouts.Modules;

/// <summary>How a side between two end plates is made (ModuleDrawing §5.2).</summary>
public enum SideMode
{
    /// <summary>Generated from the main path and the width.</summary>
    Auto,

    /// <summary>Drawn by hand; its ends stay attached to the end-plate corners.</summary>
    Custom,
}
