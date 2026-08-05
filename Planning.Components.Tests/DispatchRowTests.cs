using Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers what a station's dispatch list is made of: which clearances a call produces, what each row
/// names, and the time order the list is worked through in.
/// </summary>
[TestClass]
public class DispatchRowTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    // Notes carry per-language texts; the tests read them back in the same language.
    private static string LanguageCode => System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    private const string Origin = "Munkeröd";
    private const string Middle = "Slokärr";
    private const string Terminus = "Stilkøbing";

    // A layout of three manned stations in a row, with one three-call train over the whole of it.
    private static Timetable CreateTimetable(int dwellMinutes)
    {
        var layout = new Layout { Name = "Test" };
        var first = layout.Add(NewStation(1, Origin, "Mkd"));
        var middle = layout.Add(NewStation(2, Middle, "Slk"));
        var last = layout.Add(NewStation(3, Terminus, "Stk"));
        layout.Add(new TrackStretch(1, first, middle, 10));
        layout.Add(new TrackStretch(2, middle, last, 10));

        var timetable = new Timetable("Test", layout);
        timetable.Add(CreateTrain(1, 1234, first, middle, last, Time.FromHourAndMinute(12, 00), dwellMinutes));
        return timetable;
    }

    private static Train CreateTrain(
        int id, int number, OperationLocation first, OperationLocation middle, OperationLocation last,
        Time start, int dwellMinutes, string middleTrack = "1")
    {
        var train = new Train(id, number);
        train.Add(new StationCall(id * 10, first["1"], start.AddMinutes(-60), start));
        var arrival = start.AddMinutes(10);
        var departure = arrival.AddMinutes(dwellMinutes);
        var middleCall = train.Add(new StationCall((id * 10) + 1, middle[middleTrack], arrival, departure));
        // Counted from the middle departure, so a longer dwell moves the terminus later instead of
        // leaving the train due at the terminus before it has left the middle station.
        var end = departure.AddMinutes(20);
        var lastCall = train.Add(new StationCall((id * 10) + 2, last["1"], end, end.AddMinutes(20)));
        // Ordinary working stops, so the derived "no stop"/"no exchange" notes stay out of the way of the
        // tests that are not about them.
        middleCall.IsArrival = dwellMinutes > 0;
        middleCall.IsDeparture = dwellMinutes > 0;
        lastCall.IsArrival = true;
        return train;
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    private static OperationLocation StationNamed(Timetable timetable, string name) =>
        timetable.Layout.OperationLocations.Single(location => location.Name == name);

    private static IReadOnlyList<DispatchRow> RowsAt(Timetable timetable, string stationName) =>
        DispatchList.Create(StationNamed(timetable, stationName), timetable.Trains, Settings).Rows;

    [TestMethod]
    public void TheTrainsOriginGivesADepartureRowOnly()
    {
        var rows = RowsAt(CreateTimetable(3), Origin);

        // Nothing arrives, so there is nothing to clear in — only the train to send on its way.
        var row = rows.Single();
        Assert.AreEqual(DispatchRowKind.Departure, row.Kind);
        Assert.AreEqual("12:00", row.DepartureTime);
        Assert.IsNull(row.ArrivalTime);
    }

    [TestMethod]
    public void TheTrainsDestinationGivesAnArrivalRowOnly()
    {
        var rows = RowsAt(CreateTimetable(3), Terminus);

        var row = rows.Single();
        Assert.AreEqual(DispatchRowKind.Arrival, row.Kind);
        Assert.AreEqual("12:33", row.ArrivalTime);
        Assert.IsNull(row.DepartureTime);
    }

    [TestMethod]
    public void AStandingTrainGivesOneRowPerClearance()
    {
        var rows = RowsAt(CreateTimetable(3), Middle);

        // Clearing a train in and clearing it onward are separate actions minutes apart, so they are
        // separate lines on the sheet.
        Assert.HasCount(2, rows);
        Assert.AreEqual(DispatchRowKind.Arrival, rows[0].Kind);
        Assert.AreEqual("12:10", rows[0].ArrivalTime);
        Assert.AreEqual(DispatchRowKind.Departure, rows[1].Kind);
        Assert.AreEqual("12:13", rows[1].DepartureTime);
    }

    [TestMethod]
    public void ADepartureRowAlsoShowsWhenAndWhereTheTrainArrivedFrom()
    {
        var rows = RowsAt(CreateTimetable(3), Middle);

        // Each row stands on its own: other trains fall between a standing train's two clearances in
        // time order, so its arrival row can be several rows up the page by then. The place goes with
        // the time — a time with no place beside it is half a fact.
        Assert.AreEqual("12:10", rows[1].ArrivalTime);
        Assert.AreEqual(Origin, rows[1].OriginName);
        Assert.AreEqual("12:13", rows[1].DepartureTime);
        Assert.AreEqual(Terminus, rows[1].DestinationName);
    }

    [TestMethod]
    public void ADepartureFromTheTrainsOriginShowsNoArrivalAndNoOrigin()
    {
        // The arrival recorded on an originating call is when preparing the train begins; printing it
        // under Arr would state a movement that never happened — and naming this very station as where
        // the train has come from says nothing.
        var row = RowsAt(CreateTimetable(3), Origin).Single();

        Assert.IsNull(row.ArrivalTime);
        Assert.IsNull(row.OriginName);
    }

    [TestMethod]
    public void AnArrivalAtTheTrainsDestinationShowsNoDepartureAndNoDestination()
    {
        // The departure recorded on a terminating call is when the train is finished up, not a departure
        // anybody clears — and naming this very station as where it is going says nothing.
        var row = RowsAt(CreateTimetable(3), Terminus).Single();

        Assert.IsNull(row.DepartureTime);
        Assert.IsNull(row.DestinationName);
    }

    [TestMethod]
    public void ATrainRunningPastAppearsInBothTimeColumns()
    {
        var row = RowsAt(CreateTimetable(0), Middle).Single();

        // Its arrival and departure are the same moment, and it is stated in both columns rather than
        // once: a reader scanning Arr for the trains that arrive must find every one of them, and the
        // train that only passes through arrives just the same.
        Assert.AreEqual("12:10", row.ArrivalTime);
        Assert.AreEqual("12:10", row.DepartureTime);
    }

    [TestMethod]
    public void ATrainRunningPastGivesASingleRow()
    {
        var rows = RowsAt(CreateTimetable(0), Middle);

        // One moment, so one clearance.
        var row = rows.Single();
        Assert.AreEqual(DispatchRowKind.PassThrough, row.Kind);
        Assert.AreEqual("12:10", row.DepartureTime);
    }

    [TestMethod]
    public void RowCountFollowsTheTimesNotTheStopFlag()
    {
        var timetable = CreateTimetable(3);
        var call = timetable.Trains.Single().Calls[1];
        call.IsArrival = false;
        call.IsDeparture = false; // no longer a stop, but still stands three minutes

        // What the list schedules is the dispatcher's clearances, and a train standing at the platform
        // needs two of those whether or not anybody gets on.
        Assert.HasCount(2, RowsAt(timetable, Middle));
    }

    [TestMethod]
    public void ATrainRunningPastIsListedEvenWhereThePassingsAreHidden()
    {
        var timetable = CreateTimetable(0);
        StationNamed(timetable, Middle).HidePassings = true;

        // HidePassings suppresses passings where they are noise; here a missing one is a train nobody
        // at the station is expecting.
        Assert.HasCount(1, RowsAt(timetable, Middle));
    }

    [TestMethod]
    public void EveryRowStatesTheWholeCall()
    {
        var rows = RowsAt(CreateTimetable(3), Middle);

        // Both rows of a standing train carry all four facts. What the cells hold is a property of the
        // call, not of which clearance the row is for — that is carried by the emphasis instead, so a
        // row read on its own is never missing half of what the reader needs.
        foreach (var row in rows)
        {
            Assert.AreEqual("12:10", row.ArrivalTime);
            Assert.AreEqual(Origin, row.OriginName);
            Assert.AreEqual("12:13", row.DepartureTime);
            Assert.AreEqual(Terminus, row.DestinationName);
        }
    }

    [TestMethod]
    public void OnlyOneOfAPairOfRowsIsTheTrainsSoleRowHere()
    {
        // Neither row of a standing train stands for the whole call, so each emphasises its own
        // clearance; a train that only passes through, starts or ends here has one row that does.
        Assert.IsTrue(RowsAt(CreateTimetable(3), Middle).All(row => !row.IsSoleRow));
        Assert.IsTrue(RowsAt(CreateTimetable(0), Middle).Single().IsSoleRow, "passes through");
        Assert.IsTrue(RowsAt(CreateTimetable(3), Origin).Single().IsSoleRow, "starts its run here");
        Assert.IsTrue(RowsAt(CreateTimetable(3), Terminus).Single().IsSoleRow, "ends its run here");
    }

    [TestMethod]
    public void ATrainRunningPastNamesBothEndsOfItsRun()
    {
        // This is what tells a pass-through row apart from an ordinary departure, which the tint alone
        // cannot: an ordinary departure leaves the From cell empty.
        var row = RowsAt(CreateTimetable(0), Middle).Single();

        Assert.AreEqual(Origin, row.OriginName);
        Assert.AreEqual(Terminus, row.DestinationName);
    }

    [TestMethod]
    public void RowsAreOrderedByTimeSoOtherTrainsFallBetweenAStandingTrainsTwoClearances()
    {
        var timetable = CreateTimetable(30); // 1234 stands at Slokärr 12:10–12:40
        var first = StationNamed(timetable, Origin);
        var middle = StationNamed(timetable, Middle);
        var last = StationNamed(timetable, Terminus);
        middle.Add(new StationTrack(99, "2"));
        // A second train passes Slokärr at 12:25, while the first is still standing there.
        timetable.Add(CreateTrain(2, 5678, first, middle, last, Time.FromHourAndMinute(12, 15), 0, "2"));

        var rows = RowsAt(timetable, Middle);

        // Time order, not train order: the sheet is worked down the page as the clock runs, and the
        // train that passes falls between the two clearances of the train that stands.
        Assert.AreSequenceEqual(new[] { "12:10", "12:25", "12:40" }, rows.Select(r => r.Time.HHMM()).ToArray());
        Assert.AreEqual(5678.ToString(), rows[1].TrainIdentity);
    }

    [TestMethod]
    public void SimultaneousClearancesPutTheArrivalFirst()
    {
        var timetable = CreateTimetable(3);
        var middle = StationNamed(timetable, Middle);
        middle.Add(new StationTrack(99, "2"));
        var first = StationNamed(timetable, Origin);
        var last = StationNamed(timetable, Terminus);
        // Arrives at Slokärr at exactly 12:13, when the first train is due away.
        timetable.Add(CreateTrain(2, 5678, first, middle, last, Time.FromHourAndMinute(12, 03), 3, "2"));

        var rows = RowsAt(timetable, Middle).Where(r => r.Time.HHMM() == "12:13").ToList();

        // A train pulling in has to be cleared in before the platform is given away.
        Assert.AreEqual(DispatchRowKind.Arrival, rows[0].Kind);
        Assert.AreEqual(DispatchRowKind.Departure, rows[1].Kind);
    }

    [TestMethod]
    public void ADriversOwnNoteNeverReachesTheStationsList()
    {
        var timetable = CreateTimetable(3);
        var call = timetable.Trains.Single().Calls[1];
        call.Notes.Add(new TextCallNote("for the driver", LanguageCode) { IsStationNote = false, IsForArrival = true });
        call.Notes.Add(new TextCallNote("for the station", LanguageCode) { IsForArrival = true });

        var arrival = RowsAt(timetable, Middle)[0];

        Assert.AreEqual("for the station", arrival.Notes.Single().ToText);
    }

    [TestMethod]
    public void ATwoRowCallSplitsItsNotesByClearance()
    {
        var timetable = CreateTimetable(3);
        var call = timetable.Trains.Single().Calls[1];
        call.Notes.Add(new TextCallNote("on arrival", LanguageCode, 1) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("on departure", LanguageCode, 2) { IsForDeparture = true });

        var rows = RowsAt(timetable, Middle);

        Assert.AreEqual("on arrival", rows[0].Notes.Single().ToText);
        Assert.AreEqual("on departure", rows[1].Notes.Single().ToText);
    }

    [TestMethod]
    public void ASingleRowCallCarriesBothClearancesNotes()
    {
        var timetable = CreateTimetable(0);
        var call = timetable.Trains.Single().Calls[1];
        call.Notes.Add(new TextCallNote("a", LanguageCode, 1) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("b", LanguageCode, 2) { IsForDeparture = true });

        var texts = RowsAt(timetable, Middle).Single().Notes.Select(note => note.ToText).ToList();

        // With one row the arrival/departure split has nowhere to land, so a note classified for the
        // missing half would otherwise be dropped silently. (The row also carries the derived "no stop"
        // note, which is the whole point of the row.)
        Assert.Contains("a", texts);
        Assert.Contains("b", texts);
    }

    [TestMethod]
    public void ANoteThatPrintsNothingIsNotPrinted()
    {
        var timetable = CreateTimetable(0);
        var call = timetable.Trains.Single().Calls[1];
        call.Notes.Add(new TextCallNote("", LanguageCode, 1) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("   ", LanguageCode, 2) { IsForArrival = true });
        call.Notes.Add(new TextCallNote("real", LanguageCode, 3) { IsForArrival = true });

        var row = RowsAt(timetable, Middle).Single();

        // An empty note still renders an element, which as a line of its own is blank space the reader
        // cannot see but the page still pays for.
        Assert.AreEqual(row.Notes.Count - 2, row.PrintingNotes.Count);
        Assert.Contains("real", row.PrintingNotes.Select(note => note.ToText).ToList());
    }

    [TestMethod]
    public void TheNeighboursOnTheHeadingAreTheOnesTheDispatchStretchesDefine()
    {
        var timetable = CreateTimetable(3);
        var layout = timetable.Layout;
        var origin = (Station)StationNamed(timetable, Origin);
        var middle = (Station)StationNamed(timetable, Middle);
        var terminus = (Station)StationNamed(timetable, Terminus);
        origin.PhoneNumber = 15;
        terminus.PhoneNumber = 16;
        layout.DispatchStretches.Add(new DispatchStretch(1, origin, middle));

        var list = DispatchList.Create(middle, timetable.Trains, Settings);

        // Only Munkeröd, although the track network reaches Stilkøbing just as directly and it has a
        // number to ring: the dispatch stretches are the planner's own answer to who talks to whom, and
        // a station told to ring somebody it has no dispatch stretch to would be ringing the wrong people.
        Assert.AreEqual(Origin, list.Neighbours.Single().Name);
        Assert.AreEqual(15, list.Neighbours.Single().PhoneNumber);
    }

    [TestMethod]
    public void ANeighbourWithNoPhoneNumberIsLeftOffTheHeading()
    {
        var timetable = CreateTimetable(3);
        var layout = timetable.Layout;
        var origin = (Station)StationNamed(timetable, Origin);
        var middle = (Station)StationNamed(timetable, Middle);
        var terminus = (Station)StationNamed(timetable, Terminus);
        terminus.PhoneNumber = 16; // Munkeröd's is left unrecorded
        layout.DispatchStretches.Add(new DispatchStretch(1, origin, middle));
        layout.DispatchStretches.Add(new DispatchStretch(2, middle, terminus));

        var list = DispatchList.Create(middle, timetable.Trains, Settings);

        // The heading exists so a call can be made; a name with no number beside it is something to read
        // past rather than something to use.
        Assert.AreEqual(Terminus, list.Neighbours.Single().Name);
    }

    [TestMethod]
    public void AStationWithNoDispatchStretchesNamesNoNeighbours()
    {
        var timetable = CreateTimetable(3);

        // Never derived from the track network as a stand-in; the planner generates the stretches on the
        // Stretches tab, and until they do there is no answer to give.
        Assert.IsEmpty(DispatchList.Create(StationNamed(timetable, Middle), timetable.Trains, Settings).Neighbours);
    }

    [TestMethod]
    public void AnOnDemandTrainSaysSoInItsNotesNotItsSessionsColumn()
    {
        var timetable = CreateTimetable(3);
        var train = timetable.Trains.Single();
        train.Sessions = Sessions.FromBitPattern(CommonSessionPatterns.All | CommonSessionPatterns.OnDemand);

        var rows = RowsAt(timetable, Middle);

        // The column keeps the sessions alone — "on demand only" would wrap over several lines of a
        // column a few millimetres wide, on every row of that train.
        Assert.DoesNotContain("demand", rows[0].SessionsText, StringComparison.OrdinalIgnoreCase);
        Assert.IsFalse(rows[0].DisplayedSessions.IsOnDemand);

        // It is said once per row among the notes, and on both rows: each row of a dispatch list is read
        // on its own, and whether the train runs at all is not something to learn from the row above.
        foreach (var row in rows)
            Assert.ContainsSingle(row.Notes.Where(note => note is OnDemandNote));

        // The train's own value is untouched — only the rendering drops the marker.
        Assert.IsTrue(train.Sessions.IsOnDemand);
    }

    [TestMethod]
    public void ATrainThatIsNotOnDemandGetsNoSuchNote()
    {
        Assert.IsEmpty(RowsAt(CreateTimetable(3), Middle)
            .SelectMany(row => row.Notes)
            .Where(note => note is OnDemandNote));
    }

    [TestMethod]
    public void ShadowStationsAreDispatchEndpointsWhetherMannedOrNot()
    {
        var layout = new Layout { Name = "Test" };
        layout.Add(new Station(1, "Manned", "Man") { IsManned = true });
        layout.Add(new Station(2, "Fiddle yard", "Fid") { IsShadow = true, IsManned = false });
        layout.Add(new Station(3, "Unstaffed halt", "Uns"));

        // A shadow yard is always worked by somebody, so trains are cleared on and off the modelled
        // railway there like anywhere else.
        Assert.AreSequenceEqual(new[] { "Manned", "Fiddle yard" }, layout.DispatchEndpoints.Select(s => s.Name).ToArray());
    }
}
