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
public interface ILayoutImporter : IImporter<Layout>;

/// <summary>Choosing among the registered <see cref="ILayoutImporter"/>s.</summary>
public static class LayoutImporterExtensions
{
    extension(IEnumerable<ILayoutImporter> importers)
    {
        /// <summary>The first importer that reads the file <paramref name="fileName"/> with <paramref name="content"/>, if any.</summary>
        public ILayoutImporter? For(string fileName, ReadOnlySpan<byte> content) =>
            (ILayoutImporter?)ImporterExtensions.For<Layout>(importers, fileName, content);

        /// <summary>Every file extension some importer reads, without duplicates.</summary>
        public IReadOnlyList<string> FileExtensions => ImporterExtensions.ReadableFileExtensions<Layout>(importers);
    }
}
