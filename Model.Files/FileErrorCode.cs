namespace Tellurian.Trains.Schedules.Model.Files;

/// <summary>
/// Stable codes for why a file can't be read, one per case of <see cref="FileError"/>: what an importer puts in a
/// message's <c>Code</c>, so that an app can show its own translated text.
/// </summary>
public static class FileErrorCode
{
    /// <summary>The text is not valid JSON.</summary>
    public const string InvalidJson = nameof(Files.InvalidJson);

    /// <summary>The file is of another format, or its envelope is incomplete.</summary>
    public const string NotThisFormat = nameof(Files.NotThisFormat);

    /// <summary>The file is of another kind.</summary>
    public const string WrongKind = nameof(Files.WrongKind);

    /// <summary>The file was written by a newer version.</summary>
    public const string NewerVersion = nameof(Files.NewerVersion);

    /// <summary>A migration step is missing.</summary>
    public const string MigrationMissing = nameof(Files.MigrationMissing);

    /// <summary>The content can't be read as what the file says it is.</summary>
    public const string InvalidContent = nameof(Files.InvalidContent);

    /// <summary>The code of <paramref name="error"/>.</summary>
    public static string Of(FileError error) => error switch
    {
        Files.InvalidJson => InvalidJson,
        Files.NotThisFormat => NotThisFormat,
        Files.WrongKind => WrongKind,
        Files.NewerVersion => NewerVersion,
        Files.MigrationMissing => MigrationMissing,
        Files.InvalidContent => InvalidContent,
    };
}
