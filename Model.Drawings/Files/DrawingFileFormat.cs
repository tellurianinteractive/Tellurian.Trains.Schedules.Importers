using System.Reflection;

namespace Tellurian.Trains.Schedules.Model.Drawings.Files;

/// <summary>
/// The drawing files of Module Draw: their envelope (<c>format</c> <c>tellurian-drawing</c>, a <c>kind</c>, a schema
/// version and the content), the schema version of each kind, and the file names suggested for them. Read by
/// Importers.Drawing and written by Exporters.Drawing through <see cref="FileEnvelope"/>.
/// </summary>
public static class DrawingFileFormat
{
    /// <summary>The value of <c>format</c> in every drawing file.</summary>
    public const string Format = "tellurian-drawing";

    /// <summary>The media type of drawing files.</summary>
    public const string MediaType = "application/json";

    /// <summary>The name of the property that holds a drawing file's content.</summary>
    public const string ContentProperty = "content";

    /// <summary>The extension of every drawing file: they are JSON files, readable in any JSON editor.</summary>
    public const string JsonExtension = ".json";

    /// <summary>The schema version written for <paramref name="kind"/>, and the newest read.</summary>
    public static int CurrentSchemaVersion(DrawingFileKind kind) => kind switch
    {
        DrawingFileKind.EndProfile => 2,
        DrawingFileKind.Module => 2,
        DrawingFileKind.Catalogue => 2,
        DrawingFileKind.Hall => 2,
        DrawingFileKind.HallPlan => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown file kind."),
    };

    /// <summary>
    /// The file kind of <paramref name="kind"/> for <see cref="FileEnvelope"/>, without migrations: the reader adds
    /// those (Importers.Drawing), since writing needs only the current version.
    /// </summary>
    public static FileKind FileKind(DrawingFileKind kind) => new()
    {
        Name = Name(kind),
        Format = Format,
        Kind = kind.ToString(),
        ContentProperty = ContentProperty,
        CurrentSchemaVersion = CurrentSchemaVersion(kind),
        LowestSchemaVersion = 1,
    };

    /// <summary>A name of <paramref name="kind"/> for messages, e.g. "end profile".</summary>
    public static string Name(DrawingFileKind kind) => kind switch
    {
        DrawingFileKind.EndProfile => "end profile",
        DrawingFileKind.Module => "module",
        DrawingFileKind.Hall => "hall",
        DrawingFileKind.HallPlan => "hall plan",
        DrawingFileKind.Catalogue => "catalogue",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown file kind."),
    };

    /// <summary>
    /// The end of the file names suggested, including the dot, e.g. <c>.module.json</c>: the kind before
    /// <see cref="JsonExtension"/>, so the kind shows in a folder listing. The kind of a file is always read from its
    /// content, so a file with any name ending in <c>.json</c> can be opened.
    /// </summary>
    public static string Extension(DrawingFileKind kind) => kind switch
    {
        DrawingFileKind.EndProfile => ".endprofile.json",
        DrawingFileKind.Module => ".module.json",
        DrawingFileKind.Hall => ".hall.json",
        DrawingFileKind.HallPlan => ".hallplan.json",
        DrawingFileKind.Catalogue => ".catalogue.json",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown file kind."),
    };

    /// <summary>The extension used up to Module Draw 0.4, e.g. <c>.tdmodule</c>. Such files can still be opened.</summary>
    public static string LegacyExtension(DrawingFileKind kind) => kind switch
    {
        DrawingFileKind.EndProfile => ".tdendprofile",
        DrawingFileKind.Module => ".tdmodule",
        DrawingFileKind.Hall => ".tdhall",
        DrawingFileKind.HallPlan => ".tdhallplan",
        DrawingFileKind.Catalogue => ".tdcatalogue",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown file kind."),
    };

    /// <summary>
    /// The kind a file name (or an extension, with or without the dot) says, by <see cref="Extension"/> or
    /// <see cref="LegacyExtension"/>; null when it doesn't say, e.g. a plain <c>.json</c> name.
    /// </summary>
    public static DrawingFileKind? KindOf(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var normalized = name.StartsWith('.') ? name : $".{name}";
        foreach (var kind in Enum.GetValues<DrawingFileKind>())
        {
            if (normalized.EndsWith(Extension(kind), StringComparison.OrdinalIgnoreCase) ||
                normalized.EndsWith(LegacyExtension(kind), StringComparison.OrdinalIgnoreCase)) return kind;
        }
        return null;
    }

    /// <summary>True if a file with this name may be a drawing file: a JSON file, or one with a legacy extension.</summary>
    public static bool MayBeDrawingFile(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        return fileName.EndsWith(JsonExtension, StringComparison.OrdinalIgnoreCase) || KindOf(fileName) is not null;
    }

    /// <summary>The file name with a legacy extension replaced by <see cref="Extension"/>; other names are kept.</summary>
    public static string WithCurrentExtension(string fileName, DrawingFileKind kind)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        var legacy = LegacyExtension(kind);
        return fileName.EndsWith(legacy, StringComparison.OrdinalIgnoreCase) ? $"{fileName[..^legacy.Length]}{Extension(kind)}" : fileName;
    }

    /// <summary>The version of this library, without build metadata: written as <c>appVersion</c> when the app gives none.</summary>
    public static string LibraryVersion { get; } = ReadLibraryVersion();

    private static string ReadLibraryVersion()
    {
        var version = typeof(DrawingFileFormat).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
        var metadata = version.IndexOf('+');
        return metadata < 0 ? version : version[..metadata];
    }
}
