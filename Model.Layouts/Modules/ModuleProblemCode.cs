namespace Tellurian.Trains.Schedules.Model.Layouts.Modules;

/// <summary>Why a module's properties can't be saved; UIs show a text per code.</summary>
public enum ModuleProblemCode
{
    /// <summary>A module without a number needs a name.</summary>
    NameMissing,

    /// <summary>A number needs the prefix (the FREMO convention).</summary>
    PrefixMissing,

    /// <summary>The number isn't 1–9999.</summary>
    FremoNumberOutOfRange,
}
