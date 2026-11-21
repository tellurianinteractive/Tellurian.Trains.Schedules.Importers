namespace Tellurian.Trains.Schedules.Importers.Interfaces;

/// <summary>
/// Reads a <typeparamref name="T"/> from a file or a stream: a layout, a Module Draw module or end profile, a catalogue.
/// An app registers the importers it uses with its services and offers them where the user opens or imports files;
/// the first one that recognises a file reads it (<see cref="ImporterExtensions.For{T}"/>).
/// </summary>
/// <remarks>
/// An importer never throws because of what a file contains: a file it can't read, or of a newer version, gives a
/// failed <see cref="ImportResult{T}"/> with messages saying why. A message may carry a <see cref="Tellurian.Trains.Schedules.Model.Message.Code"/> that
/// the app translates.
/// </remarks>
/// <typeparam name="T">What the importer reads.</typeparam>
public interface IImporter<T>
{
    /// <summary>
    /// The resource key of what the importer reads, as shown to the user in their language: for example
    /// <c>ModuleDrawHallPlan</c>, "Hall plan from Module Draw". The app looks it up in its own labels, so adding an
    /// importer to the app includes adding this label in every language of the app.
    /// </summary>
    string ResourceKey { get; }

    /// <summary>The file extensions the importer reads, with the dot and in lower case: for example ".json".</summary>
    IReadOnlyList<string> FileExtensions { get; }

    /// <summary>
    /// Whether <paramref name="content"/>, the whole file named <paramref name="fileName"/>, is one this importer reads.
    /// Called only for a file with one of <see cref="FileExtensions"/>; should be quick and must not throw, since several
    /// formats may share an extension.
    /// </summary>
    bool CanImport(string fileName, ReadOnlySpan<byte> content);

    /// <summary>What <paramref name="stream"/> holds, or a failure with messages saying why it can't be read.</summary>
    Task<ImportResult<T>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}

/// <summary>Choosing among registered <see cref="IImporter{T}"/>s.</summary>
/// <remarks>
/// Classic extension methods, not an extension block: generic extension members can fail to load under the Blazor
/// WebAssembly interpreter.
/// </remarks>
public static class ImporterExtensions
{
    /// <summary>The first importer that reads the file <paramref name="fileName"/> with <paramref name="content"/>, if any.</summary>
    public static IImporter<T>? For<T>(this IEnumerable<IImporter<T>> importers, string fileName, ReadOnlySpan<byte> content)
    {
        ArgumentNullException.ThrowIfNull(importers);
        ArgumentNullException.ThrowIfNull(fileName);
        var extension = Path.GetExtension(fileName);
        foreach (var importer in importers)
        {
            if (!importer.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) continue;
            if (importer.CanImport(fileName, content)) return importer;
        }
        return null;
    }

    /// <summary>Every file extension some of <paramref name="importers"/> reads, without duplicates.</summary>
    public static IReadOnlyList<string> ReadableFileExtensions<T>(this IEnumerable<IImporter<T>> importers)
    {
        ArgumentNullException.ThrowIfNull(importers);
        return [.. importers.SelectMany(i => i.FileExtensions).Distinct(StringComparer.OrdinalIgnoreCase)];
    }
}
