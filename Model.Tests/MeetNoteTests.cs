namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers the crossing and overtaking derivation — the one thing about a driver's booklet the model did
/// not already express. Trains running towards each other cross whenever they overlap; trains running
/// the same way meet only when one passes the other, which the containment test decides.
/// </summary>
[TestClass]
public class MeetNoteTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);
    private static readonly TrainCategory Category = new() { Id = 1, Name = "P", Prefix = "P" };

    // The test layout runs G -> Yb -> Snu, so a forward train meets an opposite one at Ytterby.
    private static Timetable CreateTimetable()
    {
        TestDataFactory.Init();
        return new Timetable("Test", TestDataFactory.Layout());
    }

    private static StationCall CallAtYtterby(Train train) =>
        train.Calls.Single(c => c.OperationLocation.Signature == "Yb");

    // A forward train whose Ytterby call the test dictates — the only way to state an overtaking, since
    // that needs one train's stay to contain the other's. Its ends are far enough away to stay out of
    // the way, and are where the origin-and-terminus rule applies rather than at Ytterby.
    private static Train ForwardTrainAtYtterby(int number, Time arrival, Time departure)
    {
        var stations = TestDataFactory.Stations.ToArray();
        var train = new Train(number, Category, number) { Category = Category };
        _ = train.Add(new StationCall(1, stations[0]["3"], Time.FromHourAndMinute(11, 00), Time.FromHourAndMinute(11, 00)));
        _ = train.Add(new StationCall(2, stations[1]["2"], arrival, departure));
        _ = train.Add(new StationCall(3, stations[2]["1"], Time.FromHourAndMinute(14, 00), Time.FromHourAndMinute(14, 00)));
        return train;
    }

    [TestMethod]
    public void TrainsInOppositeDirectionsCross()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        var opposite = TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(12, 00));
        timetable.Add(forward);
        timetable.Add(opposite);

        var notes = CallAtYtterby(forward).MeetNotes(Sessions.All, Settings).ToList();

        var crossing = notes.OfType<CrossingNote>().Single();
        Assert.AreEqual(opposite, crossing.Meets.Single().Other);
        Assert.IsEmpty(notes.OfType<OvertakesNote>());
        Assert.IsEmpty(notes.OfType<IsOvertakenNote>());
    }

    [TestMethod]
    public void ATrainThatPassesAStandingTrainOvertakesIt()
    {
        var timetable = CreateTimetable();
        // The standing train is there before the passing one arrives and after it leaves, which is
        // exactly what lets the second get ahead of the first.
        var standing = ForwardTrainAtYtterby(1, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30));
        var passing = ForwardTrainAtYtterby(2, Time.FromHourAndMinute(12, 02), Time.FromHourAndMinute(12, 05));
        timetable.Add(standing);
        timetable.Add(passing);

        var notes = CallAtYtterby(passing).MeetNotes(Sessions.All, Settings).ToList();

        var overtakes = notes.OfType<OvertakesNote>().Single();
        Assert.AreEqual(standing, overtakes.Meets.Single().Other);
        Assert.IsEmpty(notes.OfType<IsOvertakenNote>());
        Assert.IsEmpty(notes.OfType<CrossingNote>());
    }

    [TestMethod]
    public void TheStandingTrainReadsTheSameEventAsBeingOvertaken()
    {
        var timetable = CreateTimetable();
        var standing = ForwardTrainAtYtterby(1, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30));
        var passing = ForwardTrainAtYtterby(2, Time.FromHourAndMinute(12, 02), Time.FromHourAndMinute(12, 05));
        timetable.Add(standing);
        timetable.Add(passing);

        var notes = CallAtYtterby(standing).MeetNotes(Sessions.All, Settings).ToList();

        // One event, two readings: which train the note names depends on whose booklet it is in.
        var overtaken = notes.OfType<IsOvertakenNote>().Single();
        Assert.AreEqual(passing, overtaken.Meets.Single().Other);
        Assert.IsEmpty(notes.OfType<OvertakesNote>());
    }

    [TestMethod]
    public void OverlappingInTheSameDirectionWithoutPassingIsNotAMeetAtAll()
    {
        var timetable = CreateTimetable();
        var first = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        // Two minutes behind, so both stand at Ytterby together (12:25-12:30 against 12:27-12:32) —
        // but neither gets past the other, so nothing has happened worth a note.
        var second = TestDataFactory.CreateTrainInForwardDirection(Category, 2, Time.FromHourAndMinute(12, 02));
        timetable.Add(first);
        timetable.Add(second);

        Assert.IsEmpty(CallAtYtterby(first).MeetNotes(Sessions.All, Settings));
        Assert.IsEmpty(CallAtYtterby(second).MeetNotes(Sessions.All, Settings));
    }

    [TestMethod]
    public void ArrivingOrLeavingTogetherIsNotAnOvertaking()
    {
        var timetable = CreateTimetable();
        var standing = ForwardTrainAtYtterby(1, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30));
        // Inside the standing train's stay at one end but level with it at the other: neither is
        // unambiguously in front, so this shares the plain overlap's fate.
        var sameArrival = ForwardTrainAtYtterby(2, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 05));
        var sameDeparture = ForwardTrainAtYtterby(3, Time.FromHourAndMinute(12, 25), Time.FromHourAndMinute(12, 30));
        timetable.Add(standing);
        timetable.Add(sameArrival);
        timetable.Add(sameDeparture);

        Assert.IsEmpty(CallAtYtterby(standing).MeetNotes(Sessions.All, Settings));
    }

    [TestMethod]
    public void APassThroughOvertakesAStandingTrainInASingleMoment()
    {
        var timetable = CreateTimetable();
        var standing = ForwardTrainAtYtterby(1, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30));
        // The classic overtaking: the fast train does not stop at all.
        var runningThrough = ForwardTrainAtYtterby(2, Time.FromHourAndMinute(12, 02), Time.FromHourAndMinute(12, 02));
        timetable.Add(standing);
        timetable.Add(runningThrough);

        var meet = CallAtYtterby(standing).MeetNotes(Sessions.All, Settings)
            .OfType<IsOvertakenNote>().Single().Meets.Single();

        Assert.AreEqual(runningThrough, meet.Other);
        Assert.AreEqual(meet.From, meet.To, "There is no interval: the other train is there for an instant.");
    }

    [TestMethod]
    public void TrainsThatAreNeverThereTogetherProduceNoNote()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        var muchLater = TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(16, 00));
        timetable.Add(forward);
        timetable.Add(muchLater);

        Assert.IsEmpty(CallAtYtterby(forward).MeetNotes(Sessions.All, Settings));
    }

    [TestMethod]
    public void HideMeetsSilencesBothKinds()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        var opposite = TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(12, 00));
        timetable.Add(forward);
        timetable.Add(opposite);
        var call = CallAtYtterby(forward);
        call.OperationLocation.HideMeets = true;

        // Where such events are routine, one flag silences both at once.
        Assert.IsEmpty(call.MeetNotes(Sessions.All, Settings));
    }

    [TestMethod]
    public void TheNoteCarriesTheWindowBothTrainsAreActuallyPresent()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        var opposite = TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(12, 02));
        timetable.Add(forward);
        timetable.Add(opposite);

        var note = CallAtYtterby(forward).MeetNotes(Sessions.All, Settings).OfType<CrossingNote>().Single();
        var meet = note.Meets.Single();

        // Mine 12:25-12:30, theirs 12:27-12:32: both are there 12:27-12:30, which is what the driver needs.
        Assert.AreEqual(Time.FromHourAndMinute(12, 27), meet.From);
        Assert.AreEqual(Time.FromHourAndMinute(12, 30), meet.To);
    }

    [TestMethod]
    public void TrainsSharingNoSessionNeverMeet()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        var opposite = TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(12, 00));
        forward.Sessions = Sessions.FromSessionNumbers(1, 3, 5);
        opposite.Sessions = Sessions.FromSessionNumbers(2, 4, 6);
        timetable.Add(forward);
        timetable.Add(opposite);

        Assert.IsEmpty(CallAtYtterby(forward).MeetNotes(Sessions.All, Settings));
    }

    [TestMethod]
    public void ASessionQualifierAppearsOnlyWhenItSaysSomething()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        var opposite = TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(12, 00));
        opposite.Sessions = Sessions.FromSessionNumbers(1, 3, 5);
        timetable.Add(forward);
        timetable.Add(opposite);
        var call = CallAtYtterby(forward);

        var qualified = call.MeetNotes(Sessions.All, Settings).OfType<CrossingNote>().Single();
        Assert.IsNotNull(qualified.Meets.Single().Sessions, "The meet happens on fewer sessions than the duty runs.");

        // When the shared sessions are exactly the duty's own, repeating them says nothing.
        var unqualified = call.MeetNotes(Sessions.FromSessionNumbers(1, 3, 5), Settings)
            .OfType<CrossingNote>().Single();
        Assert.IsNull(unqualified.Meets.Single().Sessions);
    }

    [TestMethod]
    public void SeveralCrossingsAggregateIntoOneNoteOrderedByStartThenEndThenTrainNumber()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        // Both stand at Ytterby exactly 12:25-12:30 against the forward train; numbers deliberately added
        // out of order, so a correct result proves the sort, not the insertion order.
        var higherNumber = TestDataFactory.CreateTrainInOppositeDirection(Category, 9, Time.FromHourAndMinute(12, 00));
        var lowerNumber = TestDataFactory.CreateTrainInOppositeDirection(Category, 3, Time.FromHourAndMinute(12, 00));
        // Two minutes earlier, so the shared window is trimmed to 12:25-12:28 — an earlier end time than
        // the other two, even though all three share the same start time.
        var earlierEnd = TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(11, 58));
        timetable.Add(forward);
        timetable.Add(higherNumber);
        timetable.Add(earlierEnd);
        timetable.Add(lowerNumber);

        var notes = CallAtYtterby(forward).MeetNotes(Sessions.All, Settings).ToList();

        var crossing = notes.OfType<CrossingNote>().Single();
        Assert.IsEmpty(notes.OfType<OvertakesNote>(), "All three meets are opposite-direction crossings.");
        Assert.HasCount(3, crossing.Meets);
        Assert.AreEqual(2, crossing.Meets[0].Other.Number, "Earliest end time (12:28) sorts first.");
        Assert.AreEqual(3, crossing.Meets[1].Other.Number, "Same window as the next; lower number sorts first.");
        Assert.AreEqual(9, crossing.Meets[2].Other.Number);
    }

    [TestMethod]
    public void AMeetAtTheTrainsOwnOriginOrTerminusProducesNoNote()
    {
        var timetable = CreateTimetable();
        var stations = TestDataFactory.Stations.ToArray();

        // The train "stands" at both ends of its run — which is the case the rule exists for. Those
        // spans are not dwells: the first call's arrival is the driver reporting before departure and
        // the last call's departure is them standing down after arrival. With equal times at the ends
        // the window is zero-length and can overlap nothing, so the rule would never be reached.
        var train = new Train(1, Category, 1) { Category = Category };
        _ = train.Add(new StationCall(1, stations[0]["3"], Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 10)));
        _ = train.Add(new StationCall(2, stations[1]["2"], Time.FromHourAndMinute(12, 25), Time.FromHourAndMinute(12, 30)));
        _ = train.Add(new StationCall(3, stations[2]["1"], Time.FromHourAndMinute(12, 55), Time.FromHourAndMinute(13, 05)));
        timetable.Add(train);

        // One opposite train at each of the three locations while this train is there: at Göteborg
        // 12:05, at Ytterby 12:25-12:30, and at Stenungsund 13:00.
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(11, 10)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(Category, 3, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(Category, 4, Time.FromHourAndMinute(13, 00)));

        // The middle call proves the meets are there to be found at all.
        Assert.IsNotEmpty(train.Calls[1].MeetNotes(Sessions.All, Settings),
            "A meet along the way is exactly what these notes are for.");
        Assert.IsEmpty(train.Calls[0].MeetNotes(Sessions.All, Settings),
            "At its origin the train has not started, so another train there is not an event on this run.");
        Assert.IsEmpty(train.Calls[^1].MeetNotes(Sessions.All, Settings),
            "At its terminus the run is over, so the same holds.");
    }

    [TestMethod]
    public void AMeetAtTheOtherTrainsOriginOrTerminusProducesNoNoteEither()
    {
        var timetable = CreateTimetable();
        var stations = TestDataFactory.Stations.ToArray();

        // This train's run ends at Ytterby: it arrives at 12:00 and its driver stands down at 12:30.
        // The stock stands there, but the train is no longer running.
        var terminating = new Train(1, Category, 1) { Category = Category };
        _ = terminating.Add(new StationCall(1, stations[0]["3"], Time.FromHourAndMinute(11, 00), Time.FromHourAndMinute(11, 00)));
        _ = terminating.Add(new StationCall(2, stations[1]["2"], Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30)));
        timetable.Add(terminating);

        // One train that would pass it and one that would cross it during that stand-down span; the two
        // are never there together themselves, so anything they report is about the terminating train.
        var passing = ForwardTrainAtYtterby(2, Time.FromHourAndMinute(12, 02), Time.FromHourAndMinute(12, 05));
        var opposite = TestDataFactory.CreateTrainInOppositeDirection(Category, 3, Time.FromHourAndMinute(11, 45));
        timetable.Add(passing);
        timetable.Add(opposite);

        Assert.IsEmpty(CallAtYtterby(passing).MeetNotes(Sessions.All, Settings),
            "Nothing was overtaken: the other train had already finished its run.");
        Assert.IsEmpty(CallAtYtterby(opposite).MeetNotes(Sessions.All, Settings),
            "Nor crossed, for the same reason.");
    }

    [TestMethod]
    public void MeetNotesAttachToTheArrival()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        var opposite = TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(12, 00));
        timetable.Add(forward);
        timetable.Add(opposite);

        var note = CallAtYtterby(forward).MeetNotes(Sessions.All, Settings).Single();

        // They answer "who am I meeting here", which the driver asks on pulling in.
        Assert.IsTrue(note.IsForArrival);
        Assert.IsFalse(note.IsForDeparture);
    }

    [TestMethod]
    public void DirectionComesFromTheStretchTheTrainArrivesOn()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        var opposite = TestDataFactory.CreateTrainInOppositeDirection(Category, 2, Time.FromHourAndMinute(12, 00));
        timetable.Add(forward);
        timetable.Add(opposite);

        Assert.AreEqual(TravelDirection.Forward, CallAtYtterby(forward).DirectionInto);
        Assert.AreEqual(TravelDirection.Backward, CallAtYtterby(opposite).DirectionInto);
    }

    [TestMethod]
    public void ATrainOriginatingAtAStationFallsBackToItsOutboundStretch()
    {
        var timetable = CreateTimetable();
        var forward = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        timetable.Add(forward);

        // The first call has no inbound stretch, but where the train is going still defines its direction.
        Assert.AreEqual(TravelDirection.Forward, forward.Calls[0].DirectionInto);
    }

    [TestMethod]
    public void PassThroughCallsAreMarkedNoStopAndStandingCallsNoExchange()
    {
        var timetable = CreateTimetable();
        var train = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        timetable.Add(train);
        var call = CallAtYtterby(train);

        // An ordinary working stop says nothing: only the absence of a stop needs stating.
        call.IsArrival = true;
        call.IsDeparture = true;
        Assert.IsEmpty(call.StopNotes);

        // The train no longer stops, but still stands 12:25-12:30 — it has no work here.
        call.IsArrival = false;
        call.IsDeparture = false;
        Assert.IsInstanceOfType<NoExchangeNote>(call.StopNotes.Single());

        // Equal times make it a single moment: the train runs through.
        call.Departure = call.Arrival;
        Assert.IsInstanceOfType<NoStopNote>(call.StopNotes.Single());
    }

    [TestMethod]
    public void NoStopIsADepartureNoteAndNoExchangeAnArrivalNote()
    {
        var timetable = CreateTimetable();
        var train = TestDataFactory.CreateTrainInForwardDirection(Category, 1, Time.FromHourAndMinute(12, 00));
        timetable.Add(train);
        var call = CallAtYtterby(train);
        call.IsArrival = false;
        call.IsDeparture = false;

        // "No exchange" answers a question asked on pulling in.
        Assert.IsTrue(call.StopNotes.Single().IsForArrival);

        // A pass-through's single row shows the departure time, so its note belongs to that half.
        call.Departure = call.Arrival;
        Assert.IsTrue(call.StopNotes.Single().IsForDeparture);
    }
}
