using Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the row structure of a duty booklet's train timetable: how many rows a call gets, which half
/// each shows, and how a call's notes are split between the call row's note column and the full-width
/// rows beneath it.
/// </summary>
[TestClass]
public class TimetableRowTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    // Notes carry per-language texts; the tests read them back in the same language.
    private static string LanguageCode => System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    // A three-call train: origin, an intermediate call, and the terminus.
    private static Train CreateTrain(int intermediateDwellMinutes)
    {
        var layout = new Layout { Name = "Test" };
        var first = layout.Add(NewStation(1, "Munkeröd", "Mkd"));
        var middle = layout.Add(NewStation(2, "Slokärr", "Slk"));
        var last = layout.Add(NewStation(3, "Stilkøbing", "Stk"));
        layout.Add(new TrackStretch(1, first, middle, 10));
        layout.Add(new TrackStretch(2, middle, last, 10));

        var timetable = new Timetable("Test", layout);
        var train = new Train(1, 1234);
        var start = Time.FromHourAndMinute(12, 00);
        train.Add(new StationCall(1, first["1"], start.AddMinutes(-60), start));
        var arrival = start.AddMinutes(10);
        var middleCall = train.Add(new StationCall(2, middle["1"], arrival, arrival.AddMinutes(intermediateDwellMinutes)));
        var end = start.AddMinutes(30);
        var lastCall = train.Add(new StationCall(3, last["1"], end, end.AddMinutes(20)));
        // Ordinary working stops, so the derived "no stop"/"no exchange" notes stay out of the way of
        // the tests that are not about them. A test wanting one clears these flags itself.
        middleCall.IsArrival = true;
        middleCall.IsDeparture = true;
        lastCall.IsArrival = true;
        timetable.Add(train);
        return train;
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature);
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    private static IReadOnlyList<TimetableRow> RowsFor(int dwellMinutes) =>
        TimetableRow.Build(CreateTrain(dwellMinutes).AsTrainPart, Sessions.All, Settings);

    // A part covering only the middle leg of the three-call train: the driver takes over at Slokärr and
    // finishes at Stilkøbing, so Munkeröd is not theirs at all.
    private static IReadOnlyList<TimetableRow> RowsForMiddleLeg(Train train) =>
        TimetableRow.Build(train.AsTrainPart(1, 2), Sessions.All, Settings);

    [TestMethod]
    public void AHalfTheDriverDoesNotWorkIsShownButMarked()
    {
        var rows = RowsForMiddleLeg(CreateTrain(3));

        // The whole train is printed, so the route still reads end to end.
        Assert.HasCount(4, rows);
        var munkerod = rows.Single(r => r.StationName == "Munkeröd");
        Assert.IsFalse(munkerod.IsInPart, "The leg before the driver takes over is not theirs.");
    }

    [TestMethod]
    public void AtTheTakeoverCallTheDepartureIsTheirsButNotTheArrival()
    {
        var slokarr = RowsForMiddleLeg(CreateTrain(3)).Where(r => r.StationName == "Slokärr").ToList();

        // The train arrived worked by somebody else; this driver only takes it out.
        Assert.AreEqual(CallHalf.Arrival, slokarr[0].Half);
        Assert.IsFalse(slokarr[0].IsInPart);
        Assert.AreEqual(CallHalf.Departure, slokarr[1].Half);
        Assert.IsTrue(slokarr[1].IsInPart);
    }

    [TestMethod]
    public void AtTheFinishingCallTheArrivalIsTheirsButNotTheDeparture()
    {
        // The train terminates at Stilkøbing here, so only the arrival row exists — and it is theirs.
        var arrival = RowsForMiddleLeg(CreateTrain(3)).Single(r => r.StationName == "Stilkøbing");
        Assert.AreEqual(CallHalf.Arrival, arrival.Half);
        Assert.IsTrue(arrival.IsInPart);

        // On a train that runs on past the driver, the departure it leaves on is not theirs.
        var train = CreateTrain(3);
        var partEndingMidTrain = TimetableRow.Build(train.AsTrainPart(0, 1), Sessions.All, Settings);
        var slokarr = partEndingMidTrain.Where(r => r.StationName == "Slokärr").ToList();
        Assert.IsTrue(slokarr[0].IsInPart, "The arrival ends the driver's work.");
        Assert.IsFalse(slokarr[1].IsInPart, "The train goes on without them.");
    }

    [TestMethod]
    public void ACancelledHalfCarriesNoNotes()
    {
        var train = CreateTrain(3);
        train.Calls[0].Notes.Add(new TextCallNote("Not your work", LanguageCode) { IsForDeparture = true });

        var munkerod = RowsForMiddleLeg(train).Single(r => r.StationName == "Munkeröd");

        // The notes are work instructions; against a cancelled row they would tell the driver to do
        // something that is not theirs to do.
        Assert.IsEmpty(munkerod.Notes);
    }

    [TestMethod]
    public void EveryHalfIsTheDriversWhenTheyWorkTheWholeTrain()
    {
        Assert.IsTrue(RowsFor(3).All(r => r.IsInPart));
    }

    [TestMethod]
    public void TheFirstCallShowsItsDepartureOnly()
    {
        var rows = RowsFor(3);

        Assert.AreEqual(CallHalf.Departure, rows[0].Half);
        // Its arrival is the header's "starts at" — an hour of preparation at the origin.
        Assert.AreEqual(Time.FromHourAndMinute(12, 00), rows[0].Time);
        Assert.AreEqual("Munkeröd", rows[0].StationName);
    }

    [TestMethod]
    public void TheLastCallShowsItsArrivalOnly()
    {
        var rows = RowsFor(3);

        Assert.AreEqual(CallHalf.Arrival, rows[^1].Half);
        // Its departure is the header's "ends at" — the driver's stand-down time.
        Assert.AreEqual(Time.FromHourAndMinute(12, 30), rows[^1].Time);
    }

    [TestMethod]
    public void AnIntermediateCallThatStandsGetsTwoRows()
    {
        var rows = RowsFor(3);

        var middle = rows.Where(r => r.StationName == "Slokärr").ToList();
        Assert.HasCount(2, middle);
        Assert.AreEqual(CallHalf.Arrival, middle[0].Half);
        Assert.AreEqual(CallHalf.Departure, middle[1].Half);
        Assert.AreEqual("Slokärr", middle[1].StationName,
            "The station name is repeated, so a time is never read against a blank cell.");
    }

    [TestMethod]
    public void AnIntermediateCallThatPassesThroughGetsOneRowShowingTheDeparture()
    {
        var rows = RowsFor(0);

        var middle = rows.Where(r => r.StationName == "Slokärr").ToList();
        Assert.HasCount(1, middle);
        // "Neither" reads more clearly as an empty cell than as a missing word.
        Assert.AreEqual(CallHalf.None, middle[0].Half);
        Assert.AreEqual(Time.FromHourAndMinute(12, 10), middle[0].Time);
    }

    [TestMethod]
    public void RowCountFollowsTheTimesNotTheStopFlag()
    {
        var train = CreateTrain(3);
        var call = train.Calls[1];
        call.IsArrival = false;
        call.IsDeparture = false; // no longer a stop, but still stands three minutes

        var rows = TimetableRow.Build(train.AsTrainPart, Sessions.All, Settings);

        Assert.HasCount(2, rows.Where(r => r.StationName == "Slokärr"),
            "The times say whether the train stands; IsStop only says whether standing means work.");
    }

    [TestMethod]
    public void AShortFirstNoteSitsInTheCallRowsNoteColumn()
    {
        var train = CreateTrain(3);
        var call = train.Calls[1];
        call.IsArrival = false;
        call.IsDeparture = false; // yields a "No exchange" note on the arrival

        var arrival = TimetableRow.Build(train.AsTrainPart, Sessions.All, Settings)
            .First(r => r.StationName == "Slokärr" && r.Half == CallHalf.Arrival);

        Assert.IsNotNull(arrival.InlineNote, "A short note keeps the compact single row.");
        Assert.IsEmpty(arrival.StackedNotes);
    }

    [TestMethod]
    public void ALongFirstNoteMovesTheWholeGroupToFullWidthRows()
    {
        var train = CreateTrain(3);
        var call = train.Calls[1];
        var longText = new string('x', TimetableRow.MaxCharsInNoteColumn + 1);
        call.Notes.Add(new TextCallNote(longText, LanguageCode, 1) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("short", LanguageCode, 2) { IsForArrival = true });

        var arrival = TimetableRow.Build(train.AsTrainPart, Sessions.All, Settings)
            .First(r => r.StationName == "Slokärr" && r.Half == CallHalf.Arrival);

        Assert.IsNull(arrival.InlineNote, "Nothing may wrap in the narrow column, by construction.");
        // Notes are never reordered to fit: a shorter later note is not promoted into the column.
        Assert.HasCount(2, arrival.StackedNotes);
        Assert.AreEqual(longText, arrival.StackedNotes.First().ToText);
    }

    [TestMethod]
    public void ANoteThatPrintsNothingTakesNoRow()
    {
        var train = CreateTrain(3);
        var call = train.Calls[1];
        call.Notes.Add(new TextCallNote("", LanguageCode, 1) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("   ", LanguageCode, 2) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("real", LanguageCode, 3) { IsForArrival = true });

        var arrival = TimetableRow.Build(train.AsTrainPart, Sessions.All, Settings)
            .First(r => r.StationName == "Slokärr" && r.Half == CallHalf.Arrival);

        // An empty note still renders a span, and as a stacked row that is a blank line the reader
        // cannot see but the row still grows by — which is what made call rows uneven on paper.
        Assert.AreEqual("real", arrival.InlineNote?.ToText,
            "An empty note must not take the note column and push a real note into a stacked row.");
        Assert.IsEmpty(arrival.StackedNotes, "An empty note must not occupy a row of its own.");
    }

    [TestMethod]
    public void ASingleRowCallCarriesBothArrivalAndDepartureNotes()
    {
        var train = CreateTrain(0); // pass-through: one row
        var call = train.Calls[1];
        call.Notes.Add(new TextCallNote("a", LanguageCode, 1) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("b", LanguageCode, 2) { IsForDeparture = true });

        var row = TimetableRow.Build(train.AsTrainPart, Sessions.All, Settings)
            .Single(r => r.StationName == "Slokärr");

        // With one row the arrival/departure split has nowhere to land, so a note classified for the
        // missing half would otherwise be dropped silently.
        Assert.HasCount(2, row.Notes);
    }

    [TestMethod]
    public void ATwoRowCallSplitsItsNotesByHalf()
    {
        var train = CreateTrain(3);
        var call = train.Calls[1];
        call.Notes.Add(new TextCallNote("on arrival", LanguageCode, 1) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("on departure", LanguageCode, 2) { IsForDeparture = true });

        var rows = TimetableRow.Build(train.AsTrainPart, Sessions.All, Settings)
            .Where(r => r.StationName == "Slokärr").ToList();

        Assert.AreEqual("on arrival", rows[0].Notes.Single().ToText);
        Assert.AreEqual("on departure", rows[1].Notes.Single().ToText);
    }

    [TestMethod]
    public void NotesKeepTheirDisplayOrder()
    {
        var train = CreateTrain(0);
        var call = train.Calls[1];
        call.Notes.Add(new TextCallNote("second", LanguageCode, 20) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("first", LanguageCode, 10) { IsForArrival = true });

        var row = TimetableRow.Build(train.AsTrainPart, Sessions.All, Settings)
            .Single(r => r.StationName == "Slokärr");

        Assert.AreEqual("first", row.Notes[0].ToText);
        Assert.AreEqual("second", row.Notes[1].ToText);
    }

    [TestMethod]
    public void OnlyAnArrivalIsFollowedByItsOwnDepartureWithinTheSameStation()
    {
        // The rule weight in the view follows from this: an arrival row takes the light rule because
        // its own departure follows, and every other row closes a station and takes the heavy one.
        // A pass-through is a whole station in a single row, so it closes one too.
        var standing = RowsFor(3).Where(r => r.StationName == "Slokärr").ToList();
        Assert.AreEqual(CallHalf.Arrival, standing[0].Half);
        Assert.AreEqual(CallHalf.Departure, standing[1].Half);

        var passing = RowsFor(0).Single(r => r.StationName == "Slokärr");
        Assert.AreEqual(CallHalf.None, passing.Half);
    }

    [TestMethod]
    public void StationStaffNotesNeverReachADriversBooklet()
    {
        var train = CreateTrain(0);
        var call = train.Calls[1];
        call.Notes.Add(new TextCallNote("for the station", LanguageCode) { IsDriverNote = false, IsForArrival = true });

        var row = TimetableRow.Build(train.AsTrainPart, Sessions.All, Settings)
            .Single(r => r.StationName == "Slokärr");

        Assert.IsEmpty(row.Notes);
    }
}
