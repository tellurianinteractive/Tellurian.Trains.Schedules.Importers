using System.IO.Compression;
using System.Xml.Linq;

using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Odt;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the station dispatch list as an OpenDocument text document.
/// </summary>
/// <remarks>
/// <para>
/// Two kinds of test live here, and the first kind is the reason the second can be written at all. The
/// document's XML is generated as text rather than through <c>XmlWriter</c>, which trades a compile-time
/// guarantee of well-formedness for a payload that stays out of the WebAssembly download — so every test
/// that reads the document parses it first, and a malformed part fails them all rather than reaching a
/// reader as a file LibreOffice refuses to open.
/// </para>
/// <para>
/// The second kind is the point of the format: that the repetition of the station heading and the column
/// headings is <em>declared</em> and the page breaks are not ours. A test asserting that no part of the
/// document breaks a page is therefore not pedantry — a hardcoded break is exactly the regression that
/// would make this an unusable document for the station owner who adds a line to it.
/// </para>
/// </remarks>
[TestClass]
public class StationDispatchOdtTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(4);

    // Returns the key, so a test can say which column it means without depending on a translation.
    private static readonly Translator Untranslated = key => key ?? "";

    private const string Origin = "Munkeröd";
    private const string Middle = "Slokärr";
    private const string Terminus = "Stilkøbing";

    private static readonly XNamespace Office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    private static readonly XNamespace Style = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    private static readonly XNamespace Table = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
    private static readonly XNamespace Text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

    // The same three-station layout the row tests use: one train over the whole of it, standing three
    // minutes in the middle, so the middle station has a pair of rows and the ends have a sole row each.
    private static Timetable CreateTimetable(int dwellMinutes = 3)
    {
        var layout = new Layout { Name = "Test" };
        var first = layout.Add(NewStation(1, Origin, "Mkd"));
        var middle = layout.Add(NewStation(2, Middle, "Slk"));
        var last = layout.Add(NewStation(3, Terminus, "Stk"));
        layout.Add(new TrackStretch(1, first, middle, 10));
        layout.Add(new TrackStretch(2, middle, last, 10));

        var timetable = new Timetable("Test", layout);
        var train = new Train(1, 1234);
        var start = Time.FromHourAndMinute(12, 00);
        train.Add(new StationCall(10, first["1"], start.AddMinutes(-60), start));
        var arrival = start.AddMinutes(10);
        var departure = arrival.AddMinutes(dwellMinutes);
        var middleCall = train.Add(new StationCall(11, middle["1"], arrival, departure));
        var end = departure.AddMinutes(20);
        var lastCall = train.Add(new StationCall(12, last["1"], end, end.AddMinutes(20)));
        middleCall.IsArrival = dwellMinutes > 0;
        middleCall.IsDeparture = dwellMinutes > 0;
        lastCall.IsArrival = true;
        // Runs two of the four sessions, so some tick-off boxes are greyed and some are not.
        train.Sessions = Sessions.FromSessionNumbers(1, 3);
        timetable.Add(train);
        return timetable;
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    private static OperationLocation StationNamed(Timetable timetable, string name) =>
        timetable.Layout.OperationLocations.Single(location => location.Name == name);

    private static DispatchList ListAt(Timetable timetable, string stationName) =>
        DispatchList.Create(StationNamed(timetable, stationName), timetable.Trains, Settings);

    private static DispatchList ListAt(string stationName, int dwellMinutes = 3)
    {
        var timetable = CreateTimetable(dwellMinutes);
        return ListAt(timetable, stationName);
    }

    // Parsing is the assertion: a part that is not well-formed XML throws here.
    private static XDocument Part(byte[] odt, string path)
    {
        using var archive = new ZipArchive(new MemoryStream(odt), ZipArchiveMode.Read);
        var entry = archive.GetEntry(path);
        Assert.IsNotNull(entry, $"{path} is missing from the package");
        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    private static XDocument ContentOf(DispatchList list) =>
        XDocument.Parse(StationDispatchOdt.ContentXml(list, Settings, Untranslated));

    private static XDocument StylesOf(DispatchList list) =>
        XDocument.Parse(StationDispatchOdt.StylesXml(list));

    private static IEnumerable<XElement> BodyRows(XDocument content) =>
        content.Descendants(Table + "table-row")
            .Where(row => row.Parent?.Name != Table + "table-header-rows");

    // A cell's text, with the paragraph style that decides how it is set — the two together are what a
    // row actually says, since the emphasis carries which clearance the row is for.
    private static (string Text, string Style) CellAt(XElement row, int index)
    {
        var cell = row.Elements(Table + "table-cell").ElementAt(index);
        var paragraph = cell.Elements(Text + "p").First();
        return (paragraph.Value, paragraph.Attribute(Text + "style-name")?.Value ?? "");
    }

    private static string CellStyleAt(XElement row, int index) =>
        row.Elements(Table + "table-cell").ElementAt(index).Attribute(Table + "style-name")?.Value ?? "";

    // Column order, as written by StationDispatchOdt.
    private const int Train = 0;
    private const int Runs = 1;
    private const int Track = 2;
    private const int Arrival = 3;
    private const int From = 4;
    private const int Departure = 5;
    private const int To = 6;
    private const int FirstTick = 7;

    [TestMethod]
    public void EveryPartOfThePackageIsWellFormedXml()
    {
        var odt = StationDispatchOdt.Create(ListAt(Middle), Settings, Untranslated);

        // Parsing each part is the whole test; see the class remarks for why it is worth its own name.
        foreach (var path in new[] { "content.xml", "styles.xml", "meta.xml", "META-INF/manifest.xml" })
            Assert.IsNotNull(Part(odt, path).Root, path);
    }

    [TestMethod]
    public void TheMimetypeIsTheFirstEntryAndIsStoredUncompressed()
    {
        var odt = StationDispatchOdt.Create(ListAt(Middle), Settings, Untranslated);

        using var archive = new ZipArchive(new MemoryStream(odt), ZipArchiveMode.Read);
        var first = archive.Entries[0];
        Assert.AreEqual("mimetype", first.FullName);
        Assert.AreEqual(OdtPackage.MediaType, new StreamReader(first.Open()).ReadToEnd());

        // Stored, not deflated, so the media type sits at a fixed offset for a file-type sniffer to read
        // without unzipping. LibreOffice opens the document either way, which is why nothing but a test
        // would catch this: the compression method is two bytes at offset 8 of the local file header.
        Assert.AreEqual(0, odt[8] + odt[9], "mimetype must be stored, not compressed");
        Assert.AreEqual(first.Length, first.CompressedLength);
    }

    [TestMethod]
    public void TheColumnHeadingsAreDeclaredAsRepeatingTableHeaderRows()
    {
        var content = ContentOf(ListAt(Middle));

        // This is what makes LibreOffice repeat them at the top of every page the table runs onto — the
        // point of exporting a document instead of a paginated rendering.
        var headerRows = content.Descendants(Table + "table-header-rows").Single();
        var row = headerRows.Elements(Table + "table-row").Single();
        Assert.AreEqual("Train", CellAt(row, Train).Text);
        Assert.AreEqual("Notes", CellAt(row, row.Elements(Table + "table-cell").Count() - 1).Text);
    }

    [TestMethod]
    public void NothingInTheDocumentBreaksAPage()
    {
        var list = ListAt(Middle);
        var parts = new[] { StationDispatchOdt.ContentXml(list, Settings, Untranslated), StationDispatchOdt.StylesXml(list) };

        // The reason for the format: where the pages fall is LibreOffice's business, because the moment a
        // station owner adds a line any break we had placed would be in the wrong place.
        foreach (var part in parts)
        {
            Assert.DoesNotContain("break-before", part);
            Assert.DoesNotContain("break-after", part);
        }
    }

    [TestMethod]
    public void TheStationHeadingRepeatsThroughTheMasterPageHeader()
    {
        var timetable = CreateTimetable();
        var layout = timetable.Layout;
        var origin = (Station)StationNamed(timetable, Origin);
        var middle = (Station)StationNamed(timetable, Middle);
        origin.PhoneNumber = 15;
        layout.DispatchStretches.Add(new DispatchStretch(1, origin, middle));

        var styles = StylesOf(ListAt(timetable, Middle));

        // A sheet handed to a station has to identify itself whichever page it is, and say who to ring.
        var header = styles.Descendants(Style + "header").Single();
        Assert.Contains(Middle, header.Value);
        Assert.Contains("15", header.Value);
        Assert.Contains(Origin, header.Value);

        // And the page number, which takes over from the printed sheet's time interval as what orders a
        // station's pile — the interval cannot be known before the text is laid out.
        Assert.ContainsSingle(header.Descendants(Text + "page-number"));
    }

    [TestMethod]
    public void ThePageIsA4LandscapeWithTheReportsMargins()
    {
        var layout = StylesOf(ListAt(Middle)).Descendants(Style + "page-layout-properties").Single();

        Assert.AreEqual("297mm", layout.Attribute(XNamespace.Get("urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0") + "page-width")?.Value);
        Assert.AreEqual("landscape", layout.Attribute(Style + "print-orientation")?.Value);
    }

    [TestMethod]
    public void EveryRowHasOneCellForEveryDeclaredColumn()
    {
        var content = ContentOf(ListAt(Middle));

        // A row short of a cell does not fail to open: it silently shifts every value after it one column
        // left, so a departure time is read as an arrival.
        var columns = content.Descendants(Table + "table-column").Count();
        Assert.AreEqual(7 + SessionsFormatting.PositionsOf(Settings).Count + 1, columns);
        foreach (var row in content.Descendants(Table + "table-row"))
            Assert.AreEqual(columns, row.Elements(Table + "table-cell").Count());
    }

    [TestMethod]
    public void TheColumnsTogetherAreExactlyThePrintableWidth()
    {
        var content = ContentOf(ListAt(Middle));
        var widths = content.Descendants(Table + "table-column")
            .Select(column => content.Descendants(Style + "style")
                .Single(style => style.Attribute(Style + "name")?.Value == column.Attribute(Table + "style-name")!.Value)
                .Descendants(Style + "table-column-properties").Single()
                .Attribute(Style + "column-width")!.Value)
            .Select(width => double.Parse(width.Replace("mm", ""), System.Globalization.CultureInfo.InvariantCulture))
            .Sum();

        // The notes column takes whatever the others leave, so widening one of them silently narrows the
        // notes — but only as long as the total is measured against the columns actually written. Measured
        // against a stale constant instead, the table would run off the side of the page.
        Assert.AreEqual(DispatchPageGeometry.A4Landscape.PrintableWidthMm, widths, 0.001);
    }

    [TestMethod]
    public void TheTickOffColumnsAreHeadedByTheirPosition()
    {
        var content = ContentOf(ListAt(Middle));
        var row = content.Descendants(Table + "table-header-rows").Single().Elements(Table + "table-row").Single();

        // On screen a session number is a filled circle drawn as SVG, which a text document cannot carry;
        // the bare numeral heads the same column.
        Assert.AreEqual("1", CellAt(row, FirstTick).Text);
        Assert.AreEqual("4", CellAt(row, FirstTick + 3).Text);
    }

    [TestMethod]
    public void AnArrivalRowSetsTheArrivalPairBoldAndTheDeparturePairForReference()
    {
        var rows = BodyRows(ContentOf(ListAt(Middle))).ToList();

        // Every row states the whole call, so the emphasis is what says which clearance the row is for.
        var arrival = rows[0];
        Assert.AreEqual("DispatchCellStrongCentre", CellAt(arrival, Arrival).Style);
        Assert.AreEqual("DispatchCellStrong", CellAt(arrival, From).Style);
        Assert.AreEqual("DispatchCellReferenceCentre", CellAt(arrival, Departure).Style);
        Assert.AreEqual("DispatchCellReference", CellAt(arrival, To).Style);
    }

    [TestMethod]
    public void AReferenceTimeIsBracketedBecauseThereIsNoGeneratedContentInADocument()
    {
        var rows = BodyRows(ContentOf(ListAt(Middle))).ToList();

        // The printed sheet brackets it through CSS ::before/::after; a text document has no such thing,
        // so the brackets are part of the value. Only the times, not the place names.
        Assert.AreEqual("12:10", CellAt(rows[0], Arrival).Text);
        Assert.AreEqual("(12:13)", CellAt(rows[0], Departure).Text);
        Assert.AreEqual("(12:10)", CellAt(rows[1], Arrival).Text);
        Assert.AreEqual("12:13", CellAt(rows[1], Departure).Text);
        Assert.AreEqual(Terminus, CellAt(rows[0], To).Text);
    }

    [TestMethod]
    public void ASoleRowSetsAllFourBoldAndBracketsNothing()
    {
        // A train that only passes through has no other clearance, so everything on the row is what the
        // reader acts on and nothing belongs elsewhere.
        var row = BodyRows(ContentOf(ListAt(Middle, dwellMinutes: 0))).Single();

        Assert.AreEqual("DispatchCellStrongCentre", CellAt(row, Arrival).Style);
        Assert.AreEqual("DispatchCellStrong", CellAt(row, To).Style);
        Assert.AreEqual("12:10", CellAt(row, Arrival).Text);
        Assert.AreEqual("12:10", CellAt(row, Departure).Text);
    }

    [TestMethod]
    public void ADepartureRowIsTintedAndAnArrivalRowIsNot()
    {
        var rows = BodyRows(ContentOf(ListAt(Middle))).ToList();

        // The tint is the one thing on the sheet that must never be misread: white for a train being
        // cleared in, yellow for one being cleared onward.
        Assert.AreEqual("DispatchArrivalCell", CellStyleAt(rows[0], Train));
        Assert.AreEqual("DispatchDepartureCell", CellStyleAt(rows[1], Train));
    }

    [TestMethod]
    public void ASessionTheTrainDoesNotRunIsGreyedSoItCannotBeTickedByMistake()
    {
        var row = BodyRows(ContentOf(ListAt(Middle))).First();
        var ticks = Enumerable.Range(FirstTick, 4).Select(index => CellStyleAt(row, index)).ToList();

        // The train runs sessions 1 and 3 of four; the boxes are left empty either way, to be ticked by
        // hand as each session is worked.
        Assert.AreSequenceEqual(
            new[] { "DispatchArrivalTick", "DispatchTickOff", "DispatchArrivalTick", "DispatchTickOff" },
            ticks.ToArray());
        Assert.IsTrue(Enumerable.Range(FirstTick, 4).All(index => CellAt(row, index).Text.Length == 0));
    }

    [TestMethod]
    public void EachNoteIsAParagraphOfItsOwn()
    {
        var timetable = CreateTimetable();
        var call = timetable.Trains.Single().Calls[1];
        var language = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        call.Notes.Add(new TextCallNote("first", language, 1) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("second", language, 2) { IsForArrival = true });

        var row = BodyRows(ContentOf(ListAt(timetable, Middle))).First();
        var notes = row.Elements(Table + "table-cell").Last().Elements(Text + "p").Select(p => p.Value).ToList();

        // Each note is a discrete instruction; running them together lets the eye slide past the one that
        // mattered.
        Assert.Contains("first", notes);
        Assert.Contains("second", notes);
    }

    [TestMethod]
    public void AStationWithNoTrainsSaysSoAndHasNoTable()
    {
        var timetable = CreateTimetable();
        var empty = timetable.Layout.Add(NewStation(4, "Ödeby", "Öby"));

        var content = ContentOf(DispatchList.Create(empty, timetable.Trains, Settings));

        // A dispatcher handed a heading and nothing under it cannot tell an empty list from a list that
        // was never filled in.
        Assert.IsEmpty(content.Descendants(Table + "table"));
        Assert.Contains("NoTrainsAtStation", content.Root!.Value);
    }

    [TestMethod]
    public void ValuesFromTheModelAreEscapedWhereverTheyLand()
    {
        var timetable = CreateTimetable();
        var station = (Station)StationNamed(timetable, Middle);
        station.Name = """Ny & Gammel <"Bro">""";

        var list = DispatchList.Create(station, timetable.Trains, Settings);

        // The name reaches the running header, the document title and the table name, and an unescaped
        // ampersand in any of them is a file LibreOffice refuses to open. Parsing is the assertion.
        Assert.Contains(station.Name, StylesOf(list).Descendants(Style + "header").Single().Value);
        Assert.IsNotNull(ContentOf(list).Root);
        Assert.IsNotNull(Part(StationDispatchOdt.Create(list, Settings, Untranslated), "meta.xml").Root);
    }

    [TestMethod]
    public void EachStationGetsItsOwnDocumentInTheBundle()
    {
        var timetable = CreateTimetable();
        var lists = new[] { ListAt(timetable, Origin), ListAt(timetable, Middle), ListAt(timetable, Terminus) };

        var zip = StationDispatchOdt.CreateBundle(lists, Settings, Untranslated);

        // One file per station, because that is what gets sent: an owner is emailed their own station and
        // edits it without touching anybody else's.
        using var archive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
        Assert.AreSequenceEqual(
            new[] { "Mkd Munkeröd.odt", "Slk Slokärr.odt", "Stk Stilkøbing.odt" },
            archive.Entries.Select(entry => entry.FullName).ToArray());

        // And each is a whole document, not a fragment.
        foreach (var entry in archive.Entries)
        {
            using var stream = entry.Open();
            var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            Assert.IsNotNull(Part(buffer.ToArray(), "content.xml").Root, entry.FullName);
        }
    }

    [TestMethod]
    public void TwoStationsThatWouldShareAFileNameAreNumberedApart()
    {
        var timetable = CreateTimetable();
        var first = (Station)StationNamed(timetable, Origin);
        var second = (Station)StationNamed(timetable, Middle);
        // Legal in the model, and the same characters once a file name has had its slashes taken out.
        first.Name = "A/B";
        second.Name = "A:B";
        first.Signature = "X";
        second.Signature = "X";

        var zip = StationDispatchOdt.CreateBundle(
            [ListAt(timetable, "A/B"), ListAt(timetable, "A:B")], Settings, Untranslated);

        using var archive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
        // A duplicate name in a zip is not an error, so the second file would silently be the one the
        // recipient opens for both stations.
        Assert.AreEqual(2, archive.Entries.Select(entry => entry.FullName).Distinct().Count());
    }

    [TestMethod]
    public void TheFileNameLeadsWithTheSignatureSoAFolderSortsAsTheLayoutIsSigned()
    {
        Assert.AreEqual("Slk Slokärr.odt", StationDispatchOdt.FileNameOf(ListAt(Middle)));
    }
}
