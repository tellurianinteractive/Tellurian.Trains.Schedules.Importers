namespace Tellurian.Trains.Schedules.Exporters.Interfaces;

/// <summary>
/// Writes a <typeparamref name="T"/> to a stream in one format: a plan as JSON, a Module Draw module or end profile.
/// The counterpart of <c>IImporter&lt;T&gt;</c> in Importers.Interfaces: an app registers the exporters it uses and
/// offers them where the user saves or exports.
/// </summary>
/// <remarks>
/// An exporter doesn't throw because of what it writes: a failure gives a failed <see cref="ExportResult{T}"/> with
/// messages saying why. It doesn't close <c>destination</c>.
/// </remarks>
/// <typeparam name="T">What the exporter writes.</typeparam>
public interface IExporter<T>
{
    /// <summary>
    /// The resource key of what the exporter writes, as shown to the user in their language: for example
    /// <c>TellurianPlanJson</c>. The app looks it up in its own labels.
    /// </summary>
    string ResourceKey { get; }

    /// <summary>The extension of the files written, with the dot and in lower case: for example ".module.json".</summary>
    string FileExtension { get; }

    /// <summary>The media type of what is written, e.g. <c>application/json</c>.</summary>
    string MediaType { get; }

    /// <summary>
    /// Writes <paramref name="item"/> to <paramref name="destination"/>. The result's item is what was written: with
    /// <see cref="ExportOptions.IsRevision"/> a document's revision is counted up and its log has the new entry.
    /// </summary>
    Task<ExportResult<T>> ExportAsync(T item, Stream destination, ExportOptions options, CancellationToken cancellationToken = default);
}

/// <summary>How an <see cref="IExporter{T}"/> writes.</summary>
/// <param name="AppVersion">The version of the app that writes, recorded in the file's envelope; null for the version of the library.</param>
/// <param name="IsRevision">
/// Whether this write is a new revision of the document, as a save to a file is: the revision is counted up and an
/// entry with <paramref name="RevisedBy"/> and <paramref name="Date"/> added to its log. False for a draft kept in
/// browser storage, which is written as it is.
/// </param>
/// <param name="RevisedBy">Who made the revision, for the revision log.</param>
/// <param name="Date">The date of the revision; null for today.</param>
/// <param name="WriteIndented">Whether JSON is written indented, for people to read.</param>
public sealed record ExportOptions(string? AppVersion = null, bool IsRevision = false, string? RevisedBy = null, DateOnly? Date = null, bool WriteIndented = true)
{
    /// <summary>Written as it is, indented, with the library's version: no new revision.</summary>
    public static ExportOptions Default { get; } = new();
}
