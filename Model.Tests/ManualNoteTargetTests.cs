namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Which half of a call a manual note may be written for, and how a note saved without a half is given
/// one. See <c>ManualNoteRules</c>.
/// </summary>
[TestClass]
public class ManualNoteTargetTests
{
    private static StationCall CallAt(OperationLocation location, bool arrives, bool departs)
    {
        var track = new StationTrack(1, "1");
        location.Add(track);
        return new StationCall(1, track, Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 2))
        {
            IsArrival = arrives,
            IsDeparture = departs,
        };
    }

    private static StationCall Call(bool arrives, bool departs) =>
        CallAt(new Station(1, "Falun", "Fln"), arrives, departs);

    // ---- What a call offers ---------------------------------------------------------------------

    [TestMethod]
    public void AStopThatArrivesAndDepartsOffersBothHalves()
    {
        var call = Call(arrives: true, departs: true);

        CollectionAssert.AreEqual(
            new[] { CallNoteTarget.Arrival, CallNoteTarget.Departure },
            call.ManualNoteTargets.ToArray());
        Assert.IsTrue(call.CanHaveManualNote);
    }

    [TestMethod]
    public void ACallThatOnlyArrivesOffersTheArrivalAlone()
    {
        var call = Call(arrives: true, departs: false);

        CollectionAssert.AreEqual(new[] { CallNoteTarget.Arrival }, call.ManualNoteTargets.ToArray());
        Assert.AreEqual(CallNoteTarget.Arrival, call.DefaultManualNoteTarget);
    }

    [TestMethod]
    public void ACallThatOnlyDepartsOffersTheDepartureAlone()
    {
        var call = Call(arrives: false, departs: true);

        CollectionAssert.AreEqual(new[] { CallNoteTarget.Departure }, call.ManualNoteTargets.ToArray());
        Assert.AreEqual(CallNoteTarget.Departure, call.DefaultManualNoteTarget);
    }

    [TestMethod]
    public void APassthroughOffersNoHalfAtAll()
    {
        var call = Call(arrives: false, departs: false);

        Assert.AreEqual(0, call.ManualNoteTargets.Count);
        Assert.IsFalse(call.CanHaveManualNote);
        Assert.IsNull(call.DefaultManualNoteTarget);
    }

    [TestMethod]
    public void ACallWhereTheTrainCannotStopOffersNoHalfEitherWithTheFlagsSet()
    {
        // A train never stops at a signal controlled location, so the flags count for nothing there and
        // the editor shows them cleared (see StationCall.CanBeStop).
        var call = CallAt(new SignalControlledLocation(1, "Block 1", "Bl1"), arrives: true, departs: true);

        Assert.AreEqual(0, call.ManualNoteTargets.Count);
        Assert.IsFalse(call.CanHaveManualNote);
    }

    [TestMethod]
    public void WhereBothHalvesAreOfferedANoteGoesToTheDeparture()
    {
        var call = Call(arrives: true, departs: true);

        Assert.AreEqual(CallNoteTarget.Departure, call.DefaultManualNoteTarget);
        Assert.AreEqual(CallNoteTarget.Departure, call.ManualNoteTarget);
    }

    // ---- Writing a note for one half ------------------------------------------------------------

    [TestMethod]
    public void ANoteIsStampedWithTheHalfItIsWrittenFor()
    {
        var call = Call(arrives: true, departs: true);
        call.SetManualNote("Wait for the branch train", CallNoteTarget.Arrival);

        var note = call.ManualNote!;
        Assert.IsTrue(note.IsForArrival);
        Assert.IsFalse(note.IsForDeparture);
        Assert.AreEqual(CallNoteTarget.Arrival, call.ManualNoteTarget);
    }

    [TestMethod]
    public void ChangingTheHalfMovesTheNoteRatherThanAddingOne()
    {
        var call = Call(arrives: true, departs: true);
        call.SetManualNote("Wait for the branch train", CallNoteTarget.Arrival);
        call.SetManualNote("Wait for the branch train", CallNoteTarget.Departure);

        Assert.AreEqual(1, call.Notes.Count);
        var note = call.ManualNote!;
        Assert.IsFalse(note.IsForArrival);
        Assert.IsTrue(note.IsForDeparture);
    }

    [TestMethod]
    public void AHalfTheCallDoesNotOfferFallsBackToTheOneItDoes()
    {
        var call = Call(arrives: true, departs: false);
        call.SetManualNote("Shunt the vans off", CallNoteTarget.Departure);

        Assert.AreEqual(CallNoteTarget.Arrival, call.ManualNoteTarget);
        Assert.IsTrue(call.ManualNote!.IsForArrival);
    }

    [TestMethod]
    public void ANoteWhoseHalfWasTakenAwayShowsTheOneThatIsLeft()
    {
        var call = Call(arrives: true, departs: true);
        call.SetManualNote("Shunt the vans off", CallNoteTarget.Departure);
        call.IsDeparture = false;

        Assert.AreEqual(CallNoteTarget.Arrival, call.ManualNoteTarget, "The editor offers only what the call has.");
        Assert.AreEqual(CallNoteTarget.Departure, call.ManualNote!.Target, "What the note says stands until it is next edited.");
    }

    [TestMethod]
    public void NothingIsWrittenAtACallThatOffersNoHalf()
    {
        var call = Call(arrives: false, departs: false);
        call.SetManualNote("Nowhere to show this");

        Assert.IsNull(call.ManualNote);
        Assert.AreEqual(0, call.Notes.Count);
    }

    [TestMethod]
    public void ANoteLeftBehindByACallThatBecameAPassthroughCanStillBeCleared()
    {
        var call = Call(arrives: true, departs: true);
        call.SetManualNote("Written while it was a stop");
        call.IsArrival = false;
        call.IsDeparture = false;

        call.SetManualNote("");

        Assert.AreEqual(0, call.Notes.Count);
    }

    // ---- Giving an older note the half it was saved without --------------------------------------

    [TestMethod]
    public void ANoteSavedWithoutAHalfIsGivenTheCallsOwn()
    {
        // What every note written before a note said which half it was for looks like, and what the
        // XPLN import still leaves a call remark as. Unchanged, such a note is printed nowhere the two
        // halves are told apart.
        var plan = PlanWithRemark(out var call);

        var changed = plan.ApplyManualNoteTargetRules();

        Assert.AreEqual(1, changed);
        Assert.AreEqual(CallNoteTarget.Departure, call.Notes.Single().Target);
        Assert.AreEqual(0, plan.ApplyManualNoteTargetRules(), "Nothing is left to put right.");
    }

    [TestMethod]
    public void ANoteAtAPassthroughIsLeftAsItIs()
    {
        // A pass-through prints as a single row, which shows the notes it carries whole; there is no
        // half to be given, and stamping one would hide the note from that row's other half.
        var plan = PlanWithRemark(out var call);
        call.IsArrival = false;
        call.IsDeparture = false;

        Assert.AreEqual(0, plan.ApplyManualNoteTargetRules());
        Assert.IsNull(call.Notes.Single().Target);
    }

    [TestMethod]
    public void AHalfAlreadySaidIsNotChanged()
    {
        var plan = PlanWithRemark(out var call);
        call.Notes.Single().SetTarget(CallNoteTarget.Arrival);

        Assert.AreEqual(0, plan.ApplyManualNoteTargetRules());
        Assert.AreEqual(CallNoteTarget.Arrival, call.Notes.Single().Target);
    }

    // A plan whose train stops at two locations, with an imported remark — a note with no half — at the
    // second of them.
    private static Plan PlanWithRemark(out StationCall remarked)
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var train = new Train(1, 1000);
        var origin = Call(arrives: true, departs: true);
        var track = new StationTrack(2, "1");
        new Station(2, "Gävle", "Gä").Add(track);
        remarked = new StationCall(2, track, Time.FromHourAndMinute(8, 20), Time.FromHourAndMinute(8, 22),
            "Imported remark");
        // The flags are set after the calls join the train: the first call added is made a departure
        // only, and both of these are meant to be stops.
        foreach (var call in new[] { origin, remarked })
        {
            train.Add(call);
            call.IsArrival = true;
            call.IsDeparture = true;
        }
        var timetable = new Timetable("Test", layout);
        timetable.Add(train);
        return new Plan("Test", timetable);
    }
}
