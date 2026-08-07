using System.Globalization;
using System.Text;

using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Odt;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

/// <summary>
/// One station's dispatch list as an editable OpenDocument text document.
/// </summary>
/// <remarks>
/// <para>
/// The printed sheets are worked from during a session; this is the same list before the session, when the
/// station's owner still wants to add the local instructions only they know. So it is a document to be
/// edited, not a rendering to be printed: the styles are named ones a reader can restyle from Writer's
/// stylist, and the rows are ordinary table rows they can add to.
/// </para>
/// <para>
/// One document per station, because that is what gets sent: a station's owner is emailed their own
/// station and edits it without touching anybody else's. <see cref="CreateBundle"/> zips them for the
/// sender's convenience only — the recipient never sees the bundle.
/// </para>
/// <para>
/// <strong>Nothing here paginates.</strong> That is the whole reason for the format. LibreOffice breaks
/// the table where it falls, repeats the column headings through <c>table:table-header-rows</c>, and
/// repeats the station heading through the master page's <c>style:header</c> — so the sheet still
/// identifies itself on every page and still heads its columns, without a single measurement on our side.
/// <see cref="DispatchPageGeometry"/>'s calibrated millimetres exist because the HTML report has to place
/// its own breaks; the moment a station owner adds a note here, any break we had placed would be wrong
/// anyway.
/// </para>
/// <para>
/// What is lost with the breaks is the interval each page covers, which the printed heading carries: it
/// cannot be known before the text is laid out, which is exactly what we are handing over. The page
/// number takes its place as what orders a station's pile, and the first and last row of a page still say
/// what it covers.
/// </para>
/// </remarks>
public static class StationDispatchOdt
{
    /// <summary>Builds one station's dispatch list as an <c>.odt</c> file.</summary>
    /// <param name="list">The station's list.</param>
    /// <param name="settings">Chooses sessions or days, the operating period and the day names.</param>
    /// <param name="translator">Supplies the column headings in the reader's language.</param>
    /// <param name="created">When the document was generated; omitted from it when <c>null</c>.</param>
    /// <param name="fontFamily">The layout's report font, or <c>null</c> for the default one.</param>
    public static byte[] Create(
        DispatchList list, SessionsSettings settings, Translator translator, DateTimeOffset? created = null,
        string? fontFamily = null)
    {
        list = list.ValueOrException(nameof(list));
        settings = settings.ValueOrException(nameof(settings));
        translator = translator.ValueOrException(nameof(translator));

        return OdtPackage.Create(
            ContentXml(list, settings, translator), StylesXml(list, fontFamily), TitleOf(list), created);
    }

    /// <summary>
    /// Builds one document per station and bundles them into a zip.
    /// </summary>
    /// <remarks>
    /// A browser can only be handed one file per gesture, so a report covering eleven stations has to
    /// arrive as one download. A single station is <em>not</em> bundled by the caller — see
    /// <c>StationDispatchReport</c> — because a lone <c>.odt</c> is what the owner can open directly.
    /// </remarks>
    /// <param name="lists">The stations' lists, in the order they were selected.</param>
    /// <param name="settings">Chooses sessions or days, the operating period and the day names.</param>
    /// <param name="translator">Supplies the column headings in the reader's language.</param>
    /// <param name="created">When the documents were generated; omitted from them when <c>null</c>.</param>
    /// <param name="fontFamily">The layout's report font, or <c>null</c> for the default one.</param>
    public static byte[] CreateBundle(
        IEnumerable<DispatchList> lists, SessionsSettings settings, Translator translator,
        DateTimeOffset? created = null, string? fontFamily = null)
    {
        lists = lists.ValueOrException(nameof(lists));

        // Numbered where two stations would otherwise produce the same file name — a signature is unique
        // in the model, but SafeFileName can collapse two different ones onto the same characters.
        var used = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var files = new List<(string Name, byte[] Content)>();
        foreach (var list in lists)
        {
            var name = FileNameOf(list);
            used[name] = used.TryGetValue(name, out var count) ? count + 1 : 1;
            if (used[name] > 1)
                name = $"{Path.GetFileNameWithoutExtension(name)} ({used[name]}){OdtPackage.FileExtension}";
            files.Add((name, Create(list, settings, translator, created, fontFamily)));
        }
        return OdtPackage.CreateZip(files);
    }

    /// <summary>
    /// The file name for one station's document: its signature and name, as the recipient will see it in
    /// their mail.
    /// </summary>
    /// <remarks>
    /// The signature leads so a folder of them sorts the way the layout is signed, and the name follows
    /// because a signature alone is not what an owner recognises their own station by.
    /// </remarks>
    /// <param name="list">The station's list.</param>
    public static string FileNameOf(DispatchList list)
    {
        list = list.ValueOrException(nameof(list));
        var station = list.Station;
        var name = station.Signature.HasValue && station.Signature != station.Name
            ? $"{station.Signature} {station.Name}"
            : station.Name;
        return OdtPackage.SafeFileName(name, "station") + OdtPackage.FileExtension;
    }

    /// <summary>
    /// The document's <c>content.xml</c>: the table, and the automatic styles only it uses.
    /// </summary>
    /// <remarks>
    /// Exposed so the tests can read the markup without unzipping, and so the two parts can be embedded
    /// in a document assembled elsewhere.
    /// </remarks>
    /// <param name="list">The station's list.</param>
    /// <param name="settings">Chooses sessions or days, the operating period and the day names.</param>
    /// <param name="translator">Supplies the column headings in the reader's language.</param>
    public static string ContentXml(DispatchList list, SessionsSettings settings, Translator translator)
    {
        list = list.ValueOrException(nameof(list));
        settings = settings.ValueOrException(nameof(settings));
        translator = translator.ValueOrException(nameof(translator));

        var positions = SessionsFormatting.PositionsOf(settings);
        var body = list.Rows.Count == 0
            // A station with no trains says so. A dispatcher handed a heading and nothing under it cannot
            // tell an empty list from a list that was never filled in.
            ? OdtXml.Paragraph(Style.Empty, translator("NoTrainsAtStation"))
            : Table(list, settings, translator, positions);

        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <office:document-content {OdtPackage.Namespaces} office:version="1.2">
              <office:automatic-styles>
            {CellStyles(positions.Count)}
              </office:automatic-styles>
              <office:body>
                <office:text>
            {body}
                  <text:p text:style-name="Standard"/>
                </office:text>
              </office:body>
            </office:document-content>
            """;
    }

    /// <summary>
    /// The document's <c>styles.xml</c>: the named styles, the A4 landscape page, and the master page
    /// whose header repeats this station's identification on every page.
    /// </summary>
    /// <param name="list">The station's list, which supplies the running header.</param>
    /// <param name="fontFamily">The layout's report font, or <c>null</c> for the default one. The
    /// document declares it as a font face and names it in the Standard style, so Writer sets the
    /// whole document in it — and falls back to a font of the same kind where it is not installed.</param>
    public static string StylesXml(DispatchList list, string? fontFamily = null)
    {
        list = list.ValueOrException(nameof(list));

        var font = ReportFonts.FamilyName(fontFamily) is { Length: > 0 } chosen ? chosen : BodyFont;
        var fallback = FallbackOf(font);

        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <office:document-styles {OdtPackage.Namespaces} office:version="1.2">
              <office:font-face-decls>
                <style:font-face style:name="{font}" svg:font-family="'{font}', {fallback.Families}" style:font-family-generic="{fallback.Generic}" style:font-pitch="variable"/>
              </office:font-face-decls>
              <office:styles>
            {NamedStyles(font)}
              </office:styles>
              <office:automatic-styles>
                <style:page-layout style:name="{PageLayoutName}">
                  <style:page-layout-properties fo:page-width="297mm" fo:page-height="210mm" style:print-orientation="landscape" fo:margin-top="{MarginMm}mm" fo:margin-bottom="{MarginMm}mm" fo:margin-left="{MarginMm}mm" fo:margin-right="{MarginMm}mm" style:writing-mode="lr-tb"/>
                  <style:header-style>
                    <!-- Grows with its content: a station with many neighbours takes a second line and the
                         body starts lower, rather than the heading overprinting the first row. -->
                    <style:header-footer-properties fo:min-height="14mm" fo:margin-bottom="3mm" style:dynamic-spacing="true"/>
                  </style:header-style>
                </style:page-layout>
              </office:automatic-styles>
              <office:master-styles>
                <!-- Named "Standard" so the document simply starts on it: it is Writer's default page
                     style, so no page-style switch and therefore no page break is needed to reach it. -->
                <style:master-page style:name="Standard" style:page-layout-name="{PageLayoutName}">
                  <style:header>
            {HeaderParagraph(list)}
                  </style:header>
                </style:master-page>
              </office:master-styles>
            </office:document-styles>
            """;
    }

    private static string TitleOf(DispatchList list) => list.Station.Name;

    /* THE RUNNING HEADER — the page heading, repeated by LibreOffice on every page.

       One line, as on the printed sheet: the station set large, then the neighbours and their numbers at
       half that size, then the page number out at the right margin. The two halves are read differently —
       the station from across a desk by somebody checking they have the right sheet, the numbers close up
       when there is a call to make — which is why they are not the same size. */
    private static string HeaderParagraph(DispatchList list)
    {
        var header = new StringBuilder();
        header.Append($"""        <text:p text:style-name="{Style.Heading}">""");
        header.Append(OdtXml.Span(Style.StationName, list.Station.Name));
        if (list.Neighbours.Count > 0)
        {
            header.Append("""<text:s text:c="3"/>""");
            header.Append(OdtXml.Span(Style.Neighbours, NeighboursText(list)));
        }
        // Right-aligned at the tab stop the heading style puts on the right margin. The printed sheet has
        // the interval this page covers here instead, which cannot be known before the text is laid out.
        header.Append($"""<text:tab/><text:span text:style-name="{Style.Neighbours}"><text:page-number text:select-page="current">1</text:page-number></text:span>""");
        header.Append("</text:p>");
        return header.ToString();
    }

    // "Avesta ☎ 21, Fors ☎ 22". The glyph is a plain character rather than the report's Font Awesome
    // icon, because a document a station owner opens has only the fonts on their own machine.
    private static string NeighboursText(DispatchList list) =>
        string.Join(", ", list.Neighbours.Select(neighbour => $"{neighbour.Name} ☎ {neighbour.PhoneNumber}"));

    /* THE TABLE — the column headings go in table:table-header-rows, which is what makes LibreOffice
       repeat them at the top of every page the table runs onto. This is a table feature, not a page
       feature: putting the headings in the page header instead would leave them free to drift out of line
       with the columns beneath them. */
    private static string Table(
        DispatchList list, SessionsSettings settings, Translator translator, IReadOnlyList<int> positions)
    {
        var table = new StringBuilder();
        table.AppendLine($"""      <table:table table:name="{OdtXml.Escape(TableNameOf(list))}" table:style-name="{Style.Table}">""");

        foreach (var width in ColumnWidthsMm(positions.Count))
            table.AppendLine($"""        <table:table-column table:style-name="{ColumnStyleName(width)}"/>""");

        table.AppendLine("        <table:table-header-rows>");
        table.AppendLine(HeadingRow(settings, translator, positions));
        table.AppendLine("        </table:table-header-rows>");

        foreach (var row in list.Rows)
            table.AppendLine(Row(row, settings, positions));

        table.Append("      </table:table>");
        return table.ToString();
    }

    // Unique within the document, and stable: Writer shows it in the navigator, and a station owner who
    // renames it is not breaking anything.
    private static string TableNameOf(DispatchList list) =>
        $"Dispatch_{OdtPackage.SafeFileName(list.Station.Signature, "Station").Replace(' ', '_')}";

    private static string HeadingRow(
        SessionsSettings settings, Translator translator, IReadOnlyList<int> positions)
    {
        var row = new StringBuilder();
        row.AppendLine($"""          <table:table-row table:style-name="{Style.Row}">""");
        row.AppendLine(HeadCell(translator("Train"), centred: false));
        // "Runs", not "Sessions": a verb saying what the column tells you about the train, which is
        // shorter than the noun in every language. See the report's own markup for the whole reasoning.
        row.AppendLine(HeadCell(translator(settings.UseDaysInsteadOfSessionNumbers ? "Days" : "SessionsShort"), centred: false));
        row.AppendLine(HeadCell(translator("Track"), centred: true));
        row.AppendLine(HeadCell(translator("Arr"), centred: true));
        row.AppendLine(HeadCell(translator("From"), centred: false));
        row.AppendLine(HeadCell(translator("Dep"), centred: true));
        row.AppendLine(HeadCell(translator("To"), centred: false));
        foreach (var position in positions)
            row.AppendLine(HeadCell(SessionsFormatting.PositionTextOf(position, settings), centred: true, tick: true));
        row.Append(HeadCell(translator("Notes"), centred: false));
        row.Append($"{Environment.NewLine}          </table:table-row>");
        return row.ToString();
    }

    /* ONE CLEARANCE.

       Every row states the whole call — arrived when and from where, leaves when and for where — so what
       says which clearance the row is for is the emphasis: an arrival row sets the arrival pair bold, a
       departure row the departure pair. The other pair is carried for reference, set italic with the time
       in brackets, so a reader running down the Arr column sees at a glance which times are that column's
       business. A sole row has no other clearance, so all four are bold and nothing is bracketed.

       The tint says the same thing a second way, and is the one thing on the sheet that must never be
       misread: white for a train being cleared in, light yellow for one being cleared onward. */
    private static string Row(DispatchRow row, SessionsSettings settings, IReadOnlyList<int> positions)
    {
        var departing = row.Kind != DispatchRowKind.Arrival;
        var cell = departing ? Style.CellDeparture : Style.CellArrival;
        var tick = departing ? Style.TickDeparture : Style.TickArrival;

        var arrivalPair = row.IsSoleRow || !departing ? Emphasis.Acting : Emphasis.Reference;
        var departurePair = row.IsSoleRow || departing ? Emphasis.Acting : Emphasis.Reference;

        var text = new StringBuilder();
        text.AppendLine($"""        <table:table-row table:style-name="{Style.Row}">""");
        text.AppendLine(Cell(cell, Style.CellStrong, row.TrainIdentity));
        // Without the on-demand marker — that is carried among the notes, where it has room to be read.
        text.AppendLine(Cell(cell, Style.Cell, row.SessionsText));
        text.AppendLine(Cell(cell, Style.CellCentre, row.TrackNumber));
        text.AppendLine(Cell(cell, ParagraphFor(arrivalPair, centred: true), Bracketed(row.ArrivalTime, arrivalPair)));
        text.AppendLine(Cell(cell, ParagraphFor(arrivalPair, centred: false), row.OriginName));
        text.AppendLine(Cell(cell, ParagraphFor(departurePair, centred: true), Bracketed(row.DepartureTime, departurePair)));
        text.AppendLine(Cell(cell, ParagraphFor(departurePair, centred: false), row.DestinationName));
        foreach (var position in positions)
        {
            // Left empty by design: the box is ticked by hand as the session is worked. A session the
            // train does not run is greyed so it cannot be ticked by mistake.
            var covered = row.Sessions.Covers(position, settings);
            text.AppendLine(Cell(covered ? tick : Style.TickOff, Style.CellCentre, null));
        }
        text.AppendLine(NotesCell(cell, row));
        text.Append("        </table:table-row>");
        return text.ToString();
    }

    // One note per paragraph: each is a discrete instruction, and running them together lets the eye
    // slide past the one that mattered. Plain text, because a note's markup is HTML.
    private static string NotesCell(string cellStyle, DispatchRow row)
    {
        var notes = row.PrintingNotes;
        var paragraphs = notes.Count == 0
            ? OdtXml.Paragraph(Style.Note, null)
            : string.Join("", notes.Select(note => OdtXml.Paragraph(Style.Note, note.ToText)));
        return $"""          <table:table-cell table:style-name="{cellStyle}" office:value-type="string">{paragraphs}</table:table-cell>""";
    }

    private static string Cell(string cellStyle, string paragraphStyle, string? text) =>
        $"""          <table:table-cell table:style-name="{cellStyle}" office:value-type="string">{OdtXml.Paragraph(paragraphStyle, text)}</table:table-cell>""";

    private static string HeadCell(string text, bool centred, bool tick = false) =>
        $"""            <table:table-cell table:style-name="{(tick ? Style.HeadTick : Style.Head)}" office:value-type="string">{OdtXml.Paragraph(centred ? Style.HeadingCellCentre : Style.HeadingCell, text)}</table:table-cell>""";

    // Which of a row's two time-and-place pairs the reader acts on.
    private enum Emphasis { Acting, Reference }

    private static string ParagraphFor(Emphasis emphasis, bool centred) => emphasis switch
    {
        Emphasis.Acting => centred ? Style.CellStrongCentre : Style.CellStrong,
        _ => centred ? Style.CellReferenceCentre : Style.CellReference,
    };

    // The printed sheet brackets a reference time through CSS generated content; a text document has no
    // such thing, so the brackets are part of the value. Only the times are bracketed, not the places.
    private static string? Bracketed(string? time, Emphasis emphasis) =>
        emphasis == Emphasis.Reference && time.HasValue ? $"({time})" : time;

    /* COLUMN WIDTHS, in the order the cells are written.

       These mirror table.dispatch in StationDispatchTable.razor.css so the two reports look like the same
       sheet, with one deliberate departure: the printed report clips what does not fit (its cells are
       nowrap with overflow hidden) and a document wraps instead. So a column the stylesheet can size to
       its content has to be sized here to its HEADING as well — 8 mm holds a track number and not the word
       "Track" above it, which arrived as "Trac / k".

       They are otherwise not a calibration. Nothing here depends on them being exact, because a value too
       wide for its column takes another line and the table takes it from the page it is on. A millimetre
       wrong is cosmetic here and a lost row on the printed sheet. */
    private static IEnumerable<double> ColumnWidthsMm(int sessionCount)
    {
        foreach (var width in FixedColumnWidthsMm)
            yield return width;
        for (var i = 0; i < sessionCount; i++)
            yield return DispatchPageGeometry.A4Landscape.SessionColumnWidthMm;
        yield return NotesWidthMm(sessionCount);
    }

    // Train, runs, track, arrival, from, departure, to.
    private static IReadOnlyList<double> FixedColumnWidthsMm { get; } =
        [20, DispatchPageGeometry.A4Landscape.SessionsColumnWidthMm, 11, 13, 34, 13, 34];

    /// <summary>
    /// What the notes are left once the other columns have taken theirs, which is why a long operating
    /// period narrows them.
    /// </summary>
    /// <remarks>
    /// Measured against the columns this report actually writes rather than against
    /// <c>DispatchPageGeometry.FixedColumnsWidthMm</c>, so widening one of them cannot quietly push the
    /// table off the side of the page. Never below a legible minimum either: a period long enough to eat
    /// the whole width would otherwise give a zero or negative column, which is not a narrow document but
    /// a broken one. The table then runs wider than the page — visible, and fixable by shortening the
    /// period.
    /// </remarks>
    private static double NotesWidthMm(int sessionCount) => Math.Max(
        MinimumNotesWidthMm,
        DispatchPageGeometry.A4Landscape.PrintableWidthMm
            - FixedColumnWidthsMm.Sum()
            - (sessionCount * DispatchPageGeometry.A4Landscape.SessionColumnWidthMm));

    // Column styles are named after their width, so the distinct ones are written once however many
    // columns share them — fourteen tick-off columns are one style, not fourteen.
    private static string CellStyles(int sessionCount)
    {
        var styles = new StringBuilder();
        foreach (var width in ColumnWidthsMm(sessionCount).Distinct())
        {
            styles.AppendLine($"""    <style:style style:name="{ColumnStyleName(width)}" style:family="table-column">""");
            styles.AppendLine($"""      <style:table-column-properties style:column-width="{Millimetres(width)}mm"/>""");
            styles.AppendLine("    </style:style>");
        }
        styles.Append(AutomaticStyles);
        return styles.ToString();
    }

    // Invariant and without a trailing zero: a decimal comma in a length is a document LibreOffice cannot
    // read, and this machine's culture is not the reader's.
    private static string Millimetres(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);

    // The same width as a style name, where the decimal point has to go: a full stop is legal in an ODF
    // style name but is how LibreOffice separates a table style from its cell styles ("Table1.A1").
    private static string ColumnStyleName(double width) =>
        $"{Style.Column}{Millimetres(width).Replace('.', '_')}";

    // What the document is set in when the layout names no report font. Liberation Sans is metric-
    // compatible with Arial and ships with LibreOffice, so it is there for whoever opens the file.
    private const string BodyFont = "Liberation Sans";

    // What Writer reaches for when the named font is not installed on the reader's machine. The ODF
    // generic name is the one that actually decides it; the family list is what a converter reads.
    private static (string Families, string Generic) FallbackOf(string font) =>
        ReportFonts.GroupOf(font) switch
        {
            ReportFontGroup.Serif => ("'Liberation Serif', 'Times New Roman', serif", "roman"),
            ReportFontGroup.Monospace => ("'Liberation Mono', 'Courier New', monospace", "modern"),
            _ => ("'Liberation Sans', Arial, sans-serif", "swiss")
        };
    private const string PageLayoutName = "DispatchPage";
    private const double MarginMm = 10;

    /// <summary>The narrowest the notes column may become; see <see cref="ColumnWidthsMm"/>.</summary>
    private const double MinimumNotesWidthMm = 20;

    /// <summary>The style names the document uses, so the markup and the stylesheet cannot drift apart.</summary>
    private static class Style
    {
        public const string Heading = "DispatchHeading";
        public const string StationName = "DispatchStationName";
        public const string Neighbours = "DispatchNeighbours";
        public const string HeadingCell = "DispatchColumnHeading";
        public const string HeadingCellCentre = "DispatchColumnHeadingCentre";
        public const string Cell = "DispatchCell";
        public const string CellCentre = "DispatchCellCentre";
        public const string CellStrong = "DispatchCellStrong";
        public const string CellStrongCentre = "DispatchCellStrongCentre";
        public const string CellReference = "DispatchCellReference";
        public const string CellReferenceCentre = "DispatchCellReferenceCentre";
        public const string Note = "DispatchNote";
        public const string Empty = "DispatchNoTrains";

        // Automatic styles: cell shading and borders, which Writer does not expose as named cell styles.
        public const string Table = "DispatchTable";
        public const string Row = "DispatchRow";
        public const string Column = "DispatchColumn";
        public const string Head = "DispatchHeadCell";
        public const string HeadTick = "DispatchHeadTickCell";
        public const string CellArrival = "DispatchArrivalCell";
        public const string CellDeparture = "DispatchDepartureCell";
        public const string TickArrival = "DispatchArrivalTick";
        public const string TickDeparture = "DispatchDepartureTick";
        public const string TickOff = "DispatchTickOff";
    }

    /* THE STYLESHEET.

       Named styles, so a station owner can restyle the whole document from Writer's stylist instead of
       reformatting row by row — which is the difference between a document they can edit and a printout
       they have to work around. Each carries a style:display-name, because the name a reader sees in the
       stylist should read as English rather than as an identifier.

       The sizes are the printed report's: 26 pt for the station, half that for the numbers beside it, 9 pt
       for the table and three quarters of that for its headings. */
    private static string NamedStyles(string bodyFont) => $"""
            <style:style style:name="Standard" style:family="paragraph" style:class="text">
              <style:paragraph-properties fo:margin-top="0mm" fo:margin-bottom="0mm"/>
              <style:text-properties style:font-name="{bodyFont}" fo:font-size="9pt" fo:language="en" fo:country="GB"/>
            </style:style>
            <style:style style:name="DispatchHeading" style:display-name="Dispatch heading" style:family="paragraph" style:parent-style-name="Standard">
              <style:paragraph-properties fo:margin-bottom="1mm" fo:padding-bottom="1mm" fo:border-bottom="1pt solid #000000">
                <style:tab-stops>
                  <!-- On the right margin: the page number sits at the far edge of the printable width. -->
                  <style:tab-stop style:position="277mm" style:type="right"/>
                </style:tab-stops>
              </style:paragraph-properties>
            </style:style>
            <style:style style:name="DispatchStationName" style:display-name="Dispatch station name" style:family="text">
              <style:text-properties fo:font-size="26pt" fo:font-weight="bold"/>
            </style:style>
            <style:style style:name="DispatchNeighbours" style:display-name="Dispatch neighbours" style:family="text">
              <style:text-properties fo:font-size="13pt" fo:font-weight="bold"/>
            </style:style>
            <style:style style:name="DispatchColumnHeading" style:display-name="Dispatch column heading" style:family="paragraph" style:parent-style-name="Standard">
              <style:text-properties fo:font-size="6.75pt" fo:font-style="italic"/>
            </style:style>
            <style:style style:name="DispatchColumnHeadingCentre" style:display-name="Dispatch column heading, centred" style:family="paragraph" style:parent-style-name="DispatchColumnHeading">
              <style:paragraph-properties fo:text-align="center"/>
            </style:style>
            <style:style style:name="DispatchCell" style:display-name="Dispatch cell" style:family="paragraph" style:parent-style-name="Standard"/>
            <style:style style:name="DispatchCellCentre" style:display-name="Dispatch cell, centred" style:family="paragraph" style:parent-style-name="DispatchCell">
              <style:paragraph-properties fo:text-align="center"/>
            </style:style>
            <!-- The clearance this row is for: what the reader acts on. -->
            <style:style style:name="DispatchCellStrong" style:display-name="Dispatch cell, acted on" style:family="paragraph" style:parent-style-name="DispatchCell">
              <style:text-properties fo:font-weight="bold"/>
            </style:style>
            <style:style style:name="DispatchCellStrongCentre" style:display-name="Dispatch cell, acted on, centred" style:family="paragraph" style:parent-style-name="DispatchCellStrong">
              <style:paragraph-properties fo:text-align="center"/>
            </style:style>
            <!-- The train's other clearance, carried so the row can be read on its own. -->
            <style:style style:name="DispatchCellReference" style:display-name="Dispatch cell, for reference" style:family="paragraph" style:parent-style-name="DispatchCell">
              <style:text-properties fo:font-style="italic"/>
            </style:style>
            <style:style style:name="DispatchCellReferenceCentre" style:display-name="Dispatch cell, for reference, centred" style:family="paragraph" style:parent-style-name="DispatchCellReference">
              <style:paragraph-properties fo:text-align="center"/>
            </style:style>
            <style:style style:name="DispatchNote" style:display-name="Dispatch note" style:family="paragraph" style:parent-style-name="Standard"/>
            <style:style style:name="DispatchNoTrains" style:display-name="Dispatch, no trains" style:family="paragraph" style:parent-style-name="Standard">
              <style:paragraph-properties fo:margin-top="4mm" fo:margin-bottom="4mm"/>
              <style:text-properties fo:font-style="italic"/>
            </style:style>
        """;

    /* Cell shading and borders.

       Automatic rather than named because Writer has no cell styles for text tables — a reader restyles
       these through Table Properties, not the stylist. Row height is left to the content; what is pinned
       is that a row may not be split across a page, since half a clearance at the foot of a sheet is
       worse than a shorter sheet. */
    private const string AutomaticStyles = """
            <style:style style:name="DispatchTable" style:family="table">
              <style:table-properties style:width="277mm" table:align="left" fo:margin-top="0mm" table:border-model="collapsing"/>
            </style:style>
            <style:style style:name="DispatchRow" style:family="table-row">
              <style:table-row-properties fo:keep-together="always"/>
            </style:style>
            <style:style style:name="DispatchHeadCell" style:family="table-cell">
              <style:table-cell-properties fo:border-top="0.75pt solid #999999" fo:border-bottom="0.75pt solid #999999" fo:padding-top="0.5mm" fo:padding-bottom="0.5mm" fo:padding-left="1mm" fo:padding-right="1mm"/>
            </style:style>
            <style:style style:name="DispatchHeadTickCell" style:family="table-cell">
              <style:table-cell-properties fo:border="0.75pt solid #999999" fo:padding-top="0.5mm" fo:padding-bottom="0.5mm" fo:padding-left="0.25mm" fo:padding-right="0.25mm"/>
            </style:style>
            <!-- An arrival row stays white: the dispatcher is clearing the train IN. -->
            <style:style style:name="DispatchArrivalCell" style:family="table-cell">
              <style:table-cell-properties fo:background-color="#ffffff" fo:border-top="0.75pt solid #999999" fo:border-bottom="0.75pt solid #999999" fo:padding-top="0.5mm" fo:padding-bottom="0.5mm" fo:padding-left="1mm" fo:padding-right="1mm"/>
            </style:style>
            <!-- A departure row is tinted: the dispatcher is clearing it ONWARD. -->
            <style:style style:name="DispatchDepartureCell" style:family="table-cell">
              <style:table-cell-properties fo:background-color="#fffced" fo:border-top="0.75pt solid #999999" fo:border-bottom="0.75pt solid #999999" fo:padding-top="0.5mm" fo:padding-bottom="0.5mm" fo:padding-left="1mm" fo:padding-right="1mm"/>
            </style:style>
            <style:style style:name="DispatchArrivalTick" style:family="table-cell">
              <style:table-cell-properties fo:background-color="#ffffff" fo:border="0.75pt solid #999999" fo:padding="0.25mm"/>
            </style:style>
            <style:style style:name="DispatchDepartureTick" style:family="table-cell">
              <style:table-cell-properties fo:background-color="#fffced" fo:border="0.75pt solid #999999" fo:padding="0.25mm"/>
            </style:style>
            <!-- A session the train does not run: greyed so it can never be ticked by mistake. -->
            <style:style style:name="DispatchTickOff" style:family="table-cell">
              <style:table-cell-properties fo:background-color="#c8c8c8" fo:border="0.75pt solid #999999" fo:padding="0.25mm"/>
            </style:style>
        """;
}
