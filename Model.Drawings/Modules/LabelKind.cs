namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>What a label or marker on a module drawing shows (MD-9).</summary>
public enum LabelKind
{
    /// <summary>Free text.</summary>
    Text,
    /// <summary>The module's name.</summary>
    ModuleName,
    /// <summary>The name of the station.</summary>
    StationName,
    /// <summary>A platform edge marker, with a length.</summary>
    PlatformEdge,
    /// <summary>A marker for the operator side.</summary>
    OperatorSide,
    /// <summary>A signal marker.</summary>
    Signal,
}
