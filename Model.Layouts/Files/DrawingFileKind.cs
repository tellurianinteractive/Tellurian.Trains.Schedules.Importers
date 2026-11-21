namespace Tellurian.Trains.Schedules.Model.Files;

/// <summary>The kinds of drawing files (spec §6), stored as <c>kind</c> in the file envelope.</summary>
public enum DrawingFileKind
{
    /// <summary>An end profile file.</summary>
    EndProfile,
    /// <summary>A module file: one module with its drawing.</summary>
    Module,
    /// <summary>A hall file: the room that modules are set up in.</summary>
    Hall,
    /// <summary>A hall plan file: modules placed in a hall.</summary>
    HallPlan,
    /// <summary>A catalogue file of standards, add-ons and end profiles.</summary>
    Catalogue,
}
