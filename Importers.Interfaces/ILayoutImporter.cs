namespace Tellurian.Trains.Schedules.Importers.Interfaces;

/// <summary>
/// Reads a <see cref="Layout"/> made elsewhere, such as the layout block of a Module Draw hall plan, so that a plan can
/// be made from it or updated with a newer revision of it. An importer is registered with the app's services, which
/// offer every registered importer on the Import tab; the first one that recognises a file reads it.
/// </summary>
/// <remarks>
/// The layout returned should carry what lets a newer revision of the same source be compared with it:
/// <see cref="Layout.SourceId"/> naming the source and <see cref="Layout.SourceRevision"/> its revision, and ids or
/// external references of the locations that stay the same from one revision to the next. Its references must be
/// resolved: a location's layout, a track's location, a stretch's ends.
/// </remarks>
public interface ILayoutImporter
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

    /// <summary>The layout in <paramref name="stream"/>, or a failure with messages saying why it can't be read.</summary>
    Task<ImportResult<Layout>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}

/// <summary>Choosing among the registered <see cref="ILayoutImporter"/>s.</summary>
public static class LayoutImporterExtensions
{
    extension(IEnumerable<ILayoutImporter> importers)
    {
        /// <summary>The first importer that reads the file <paramref name="fileName"/> with <paramref name="content"/>, if any.</summary>
        public ILayoutImporter? For(string fileName, ReadOnlySpan<byte> content)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            var extension = Path.GetExtension(fileName);
            foreach (var importer in importers)
            {
                if (!importer.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) continue;
                if (importer.CanImport(fileName, content)) return importer;
            }
            return null;
        }

        /// <summary>Every file extension some importer reads, without duplicates.</summary>
        public IReadOnlyList<string> FileExtensions =>
            [.. importers.SelectMany(i => i.FileExtensions).Distinct(StringComparer.OrdinalIgnoreCase)];
    }
}
