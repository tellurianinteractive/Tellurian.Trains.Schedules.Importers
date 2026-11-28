namespace Tellurian.Trains.Schedules.Model.Files;

/// <summary>The text is not valid JSON.</summary>
/// <param name="Detail">What the JSON reader said.</param>
public sealed record InvalidJson(string Detail);

/// <summary>The file is of another format, or its envelope is incomplete or holds no content.</summary>
/// <param name="Reason">What is wrong with the envelope.</param>
public sealed record NotThisFormat(string Reason);

/// <summary>The file is of the right format but of another kind, e.g. a module file where a hall plan was expected.</summary>
/// <param name="Found">The kind the file says, or null when it says none.</param>
/// <param name="Expected">The kind that was expected.</param>
public sealed record WrongKind(string? Found, string Expected);

/// <summary>The file was written by a newer version, with a schema version this version doesn't read.</summary>
/// <param name="Found">The schema version of the file.</param>
/// <param name="Newest">The newest schema version this version reads.</param>
public sealed record NewerVersion(int Found, int Newest);

/// <summary>There is no step to bring the content from a schema version to the next.</summary>
/// <param name="FromVersion">The schema version without a step.</param>
public sealed record MigrationMissing(int FromVersion);

/// <summary>The content can't be read as what the file says it is.</summary>
/// <param name="Detail">What the reader said.</param>
public sealed record InvalidContent(string Detail);

/// <summary>Why a file can't be read: one of the cases, each with what it knows. A switch over it is checked to be complete.</summary>
public union FileError(InvalidJson, NotThisFormat, WrongKind, NewerVersion, MigrationMissing, InvalidContent);

/// <summary>A file can't be read; <see cref="Error"/> says why.</summary>
public sealed class FileFormatException : Exception
{
    /// <summary>Creates the exception.</summary>
    public FileFormatException(FileError error, string message, Exception? innerException = null) : base(message, innerException) => Error = error;

    /// <summary>Why the file can't be read.</summary>
    public FileError Error { get; }
}
