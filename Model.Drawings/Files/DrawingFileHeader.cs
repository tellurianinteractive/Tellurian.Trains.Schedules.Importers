namespace Tellurian.Trains.Schedules.Model.Drawings.Files;

/// <summary>The envelope of a drawing file without its content, to decide how to open it.</summary>
/// <param name="Kind">What the file holds.</param>
/// <param name="SchemaVersion">The schema version of its content.</param>
/// <param name="AppVersion">The version of the app that wrote it, or null when not said.</param>
public sealed record DrawingFileHeader(DrawingFileKind Kind, int SchemaVersion, string? AppVersion)
{
    /// <summary>True if the file can be read, possibly after migrating it: its schema version isn't newer than the current one.</summary>
    public bool IsSupported => SchemaVersion <= DrawingFileFormat.CurrentSchemaVersion(Kind);
}
