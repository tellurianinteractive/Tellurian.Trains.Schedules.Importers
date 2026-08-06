using System.IO.Compression;
using System.Text;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Odt;

/// <summary>
/// Writes an OpenDocument Text package: the zip container, its manifest, and the four XML parts a
/// <c>.odt</c> is made of.
/// </summary>
/// <remarks>
/// <para>
/// This is the whole of the format machinery a report needs. An ODT is a zip holding
/// <c>content.xml</c> (the text), <c>styles.xml</c> (the styles, page layout and the running headers),
/// <c>meta.xml</c> and a manifest — so writing one is writing four strings and zipping them, and no
/// object model over the ODF element tree earns its place. Reading or editing an arbitrary ODT is the
/// hard problem, and it is not one we have: every document here is one we generated.
/// </para>
/// <para>
/// The one rule that is genuinely easy to get wrong is <c>mimetype</c>: it must be the first entry and
/// stored uncompressed, so that its bytes land at a fixed offset and a file-type sniffer can read the
/// media type without unzipping anything. LibreOffice falls back to the manifest and opens the document
/// either way, which is exactly what makes the mistake worth guarding with a test rather than trusting to
/// review — nothing visibly breaks.
/// </para>
/// </remarks>
public static class OdtPackage
{
    /// <summary>The media type of an OpenDocument text document.</summary>
    public const string MediaType = "application/vnd.oasis.opendocument.text";

    /// <summary>The file extension of an OpenDocument text document, including the dot.</summary>
    public const string FileExtension = ".odt";

    /// <summary>The media type of a bundle of documents, as produced by <see cref="CreateZip"/>.</summary>
    public const string ZipMediaType = "application/zip";

    /// <summary>
    /// The XML namespace declarations the parts share, as attributes ready to place on a root element.
    /// </summary>
    /// <remarks>
    /// Declared on every part rather than pared down per part: the cost is a few hundred bytes in a file
    /// that is then compressed, and the alternative is a document that opens with an element silently
    /// dropped because the part it lived in had never declared its prefix.
    /// </remarks>
    public const string Namespaces = """
        xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
            xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0"
            xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0"
            xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"
            xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0"
            xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0"
            xmlns:dc="http://purl.org/dc/elements/1.1/"
            xmlns:meta="urn:oasis:names:tc:opendocument:xmlns:meta:1.0"
        """;

    /// <summary>
    /// Packs the parts of one document into an <c>.odt</c> file.
    /// </summary>
    /// <param name="contentXml">The whole of <c>content.xml</c>, including its XML declaration.</param>
    /// <param name="stylesXml">The whole of <c>styles.xml</c>, including its XML declaration.</param>
    /// <param name="title">The document title, recorded in <c>meta.xml</c>.</param>
    /// <param name="created">
    /// When the document was generated, recorded in <c>meta.xml</c> so a station owner can tell one
    /// version of a sheet from another. Omitted from the document when <c>null</c>, which is what keeps
    /// the output byte-for-byte reproducible for the tests.
    /// </param>
    public static byte[] Create(string contentXml, string stylesXml, string title, DateTimeOffset? created = null)
    {
        contentXml = contentXml.ValueOrException(nameof(contentXml));
        stylesXml = stylesXml.ValueOrException(nameof(stylesXml));

        using var buffer = new MemoryStream();
        // Left open so the archive is flushed and the central directory written before the bytes are read.
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddStored(archive, "mimetype", MediaType);
            Add(archive, "META-INF/manifest.xml", ManifestXml);
            Add(archive, "styles.xml", stylesXml);
            Add(archive, "content.xml", contentXml);
            Add(archive, "meta.xml", MetaXml(title, created));
        }
        return buffer.ToArray();
    }

    /// <summary>
    /// Bundles several documents into one zip, so a browser download that would otherwise be one file per
    /// station is a single file the sender can attach.
    /// </summary>
    /// <param name="files">The documents, by the file name each is to have inside the zip.</param>
    public static byte[] CreateZip(IEnumerable<(string Name, byte[] Content)> files)
    {
        files = files.ValueOrException(nameof(files));

        using var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in files)
            {
                var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
                using var stream = entry.Open();
                stream.Write(content);
            }
        }
        return buffer.ToArray();
    }

    /// <summary>
    /// A file name safe on every platform, built from a report-supplied name.
    /// </summary>
    /// <remarks>
    /// Station names carry punctuation that is legal in the model and not in a file name — a full stop in
    /// an abbreviation, a slash in a joint station's name — and the file is going out as an email
    /// attachment, so it has to survive whatever the recipient's system allows.
    /// </remarks>
    /// <param name="name">The name to make safe.</param>
    /// <param name="fallback">What to use when nothing usable is left of the name.</param>
    public static string SafeFileName(string? name, string fallback)
    {
        var cleaned = string.Join(
            "_", (name ?? "").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return cleaned.HasValue ? cleaned.Trim() : fallback;
    }

    // Every part is text/xml, and the root entry carries the document's media type. Kept in step with
    // Create by hand: a part missing from here is a part LibreOffice will not load.
    private const string ManifestXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <manifest:manifest xmlns:manifest="urn:oasis:names:tc:opendocument:xmlns:manifest:1.0" manifest:version="1.2">
          <manifest:file-entry manifest:full-path="/" manifest:version="1.2" manifest:media-type="application/vnd.oasis.opendocument.text"/>
          <manifest:file-entry manifest:full-path="styles.xml" manifest:media-type="text/xml"/>
          <manifest:file-entry manifest:full-path="content.xml" manifest:media-type="text/xml"/>
          <manifest:file-entry manifest:full-path="meta.xml" manifest:media-type="text/xml"/>
        </manifest:manifest>
        """;

    private static string MetaXml(string title, DateTimeOffset? created)
    {
        // ISO 8601 without an offset, which is what ODF asks for and what Writer shows in File Properties.
        var creationDate = created is { } when
            ? $"""<meta:creation-date>{when.DateTime:yyyy-MM-ddTHH:mm:ss}</meta:creation-date>"""
            : "";
        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <office:document-meta {Namespaces} office:version="1.2">
              <office:meta>
                <meta:generator>Tellurian Trains Schedules</meta:generator>
                <dc:title>{OdtXml.Escape(title)}</dc:title>
                {creationDate}
              </office:meta>
            </office:document-meta>
            """;
    }

    private static void Add(ZipArchive archive, string path, string xml)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        // No byte-order mark: the parts declare their encoding, and a BOM ahead of the XML declaration
        // trips some ODF readers.
        stream.Write(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(xml));
    }

    // The mimetype entry: first, and stored rather than deflated. See the type's remarks.
    private static void AddStored(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.NoCompression);
        using var stream = entry.Open();
        stream.Write(Encoding.ASCII.GetBytes(content));
    }
}
