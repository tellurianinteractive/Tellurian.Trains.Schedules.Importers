using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers the lock key: which locations may require one, which stations may hold one, and the two notes
/// derived from a train's run — where the key is collected and where it is handed back.
/// </summary>
[TestClass]
public class LockKeyNoteTests
{
    private static readonly TrainCategory Freight = new() { Id = 1, Name = "Freight", Prefix = "G", Content = TrainContent.Cargo };
    private static readonly TrainCategory Passenger = new() { Id = 2, Name = "Passenger", Prefix = "P", Content = TrainContent.Passenger };

    // Pin both cultures to invariant so the note texts resolve the neutral (English) Notes resource,
    // independent of the host machine's culture and of the localised Notes.<culture>.resx files.
    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [TestMethod]
    public void OnlyACargoLocationWithNobodyOnDutyCanRequireAKey()
    {
        var manned = MannedStation(1, "Göteborg", "G");
        var unmanned = new Station(2, "Bruket", "Bru") { IsManned = false };
        var halt = new OtherLocation(3, "Hålan", "Hål");

        Assert.IsFalse(manned.CanRequireLockKey, "Somebody is on duty to work the switches.");
        Assert.IsTrue(unmanned.CanRequireLockKey);
        Assert.IsTrue(new IndustrialArea(4, "Sågverket", "Såg").CanRequireLockKey);
        Assert.IsFalse(halt.CanRequireLockKey, "Nothing is exchanged there that needs a siding unlocked.");
    }

    [TestMethod]
    public void OnlyAMannedStationCanHoldAKey()
    {
        var layout = new Layout { Name = "Test" };
        var manned = MannedStation(1, "Göteborg", "G");
        var unmanned = new Station(2, "Bruket", "Bru") { IsManned = false };
        layout.Add(manned);
        layout.Add(unmanned);
        layout.Add(new IndustrialArea(3, "Sågverket", "Såg"));

        // An unmanned location has nobody to hand the key over, which is the whole reason it exists.
        Assert.AreEqual(manned, layout.LockKeyHoldingStations.Single());
    }

    [TestMethod]
    public void TheKeyIsCollectedLeavingTheHoldingStationAndHandedBackOnReturn()
    {
        var (holder, siding) = KeyAt("A1");
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            Stop(2, siding, "08:30", "09:00"),
            Stop(3, holder, "09:30", "09:40"));

        var outward = train.Calls[0].LockKeyNotes.Single();
        var homeward = train.Calls[2].LockKeyNotes.Single();

        Assert.IsInstanceOfType<PickUpLockKeyNote>(outward);
        Assert.IsInstanceOfType<LeaveLockKeyNote>(homeward);
        Assert.AreEqual("Pick up key A1 for unlocking Bruket.", outward.ToText);
        Assert.AreEqual("Leave key A1 from Bruket.", homeward.ToText);
    }

    [TestMethod]
    public void AKeyWithNoNameStillReads()
    {
        var (holder, siding) = KeyAt("");
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            Stop(2, siding, "08:30", "09:00"),
            Stop(3, holder, "09:30", "09:40"));

        Assert.AreEqual("Pick up key for unlocking Bruket.", train.Calls[0].LockKeyNotes.Single().ToText);
        Assert.AreEqual("Leave key from Bruket.", train.Calls[2].LockKeyNotes.Single().ToText);
    }

    [TestMethod]
    public void CollectingIsADepartureNoteAndHandingBackAnArrivalNote()
    {
        var (holder, siding) = KeyAt("A1");
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            Stop(2, siding, "08:30", "09:00"),
            Stop(3, holder, "09:30", "09:40"));

        // The key has to be in the cab before the train leaves, and is not back in the station's hands
        // until the train has pulled in.
        Assert.IsTrue(train.Calls[0].LockKeyNotes.Single().IsForDeparture);
        Assert.IsTrue(train.Calls[2].LockKeyNotes.Single().IsForArrival);
    }

    [TestMethod]
    public void TheNotesAreWrittenAtTheHoldingStationOnly()
    {
        var (holder, siding) = KeyAt("A1");
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            Stop(2, siding, "08:30", "09:00"),
            Stop(3, holder, "09:30", "09:40"));

        // At the siding the driver is already holding the key; there is nothing to tell them there.
        Assert.IsEmpty(train.Calls[1].LockKeyNotes);
    }

    [TestMethod]
    public void ATrainThatNeverCallsAtTheHoldingStationIsToldNothing()
    {
        var (_, siding) = KeyAt("A1");
        var elsewhere = MannedStation(4, "Stenungsund", "Snu");
        var train = FreightTrain(
            Stop(1, elsewhere, "08:00", "08:10"),
            Stop(2, siding, "08:30", "09:00"));

        Assert.IsEmpty(train.Calls[0].LockKeyNotes, "The key is held at a station this train does not visit.");
    }

    [TestMethod]
    public void ATrainRunningThroughTheKeyedLocationFetchesNoKey()
    {
        var (holder, siding) = KeyAt("A1");
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            PassThrough(2, siding, "08:30"),
            Stop(3, holder, "09:30", "09:40"));

        // Nothing is unlocked by a train that does not stop, so there is no key to fetch.
        Assert.IsEmpty(train.Calls[0].LockKeyNotes);
        Assert.IsEmpty(train.Calls[2].LockKeyNotes);
    }

    [TestMethod]
    public void APassengerTrainFetchesNoKey()
    {
        var (holder, siding) = KeyAt("A1");
        var train = new Train(2, Passenger, 101) { Category = Passenger };
        train.Add(Stop(1, holder, "08:00", "08:10"));
        train.Add(Stop(2, siding, "08:30", "09:00"));
        train.Add(Stop(3, holder, "09:30", "09:40"));

        // A key unlocks sidings that are worked with wagons; a train carrying none has no use for it —
        // and cannot stop at an industrial area at all.
        Assert.IsEmpty(train.Calls[0].LockKeyNotes);
        Assert.IsEmpty(train.Calls[2].LockKeyNotes);
    }

    [TestMethod]
    public void TheKeyIsCollectedAtTheLastCallBeforeTheWorkAndHandedBackAtTheFirstAfterIt()
    {
        var (holder, siding) = KeyAt("A1");
        var train = FreightTrain(
            Stop(1, holder, "07:00", "07:10"),
            Stop(2, holder, "07:30", "07:40"),
            Stop(3, siding, "08:00", "08:30"),
            Stop(4, holder, "09:00", "09:10"),
            Stop(5, holder, "09:30", "09:40"));

        // Carrying the key around for an extra visit is not what the driver is told to do.
        Assert.IsEmpty(train.Calls[0].LockKeyNotes);
        Assert.IsInstanceOfType<PickUpLockKeyNote>(train.Calls[1].LockKeyNotes.Single());
        Assert.IsInstanceOfType<LeaveLockKeyNote>(train.Calls[3].LockKeyNotes.Single());
        Assert.IsEmpty(train.Calls[4].LockKeyNotes);
    }

    [TestMethod]
    public void WorkingTheSameLocationTwiceIsStillOneKey()
    {
        var (holder, siding) = KeyAt("A1");
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            Stop(2, siding, "08:30", "09:00"),
            Stop(3, siding, "09:15", "09:45"),
            Stop(4, holder, "10:15", "10:25"));

        Assert.AreEqual("Pick up key A1 for unlocking Bruket.", train.Calls[0].LockKeyNotes.Single().ToText);
        Assert.AreEqual("Leave key A1 from Bruket.", train.Calls[3].LockKeyNotes.Single().ToText);
    }

    [TestMethod]
    public void TwoLocationsWorkedFromOneStationGiveOneNoteEach()
    {
        var (holder, siding) = KeyAt("A1");
        var second = new IndustrialArea(5, "Sågverket", "Såg") { LockKey = new LockKey { HeldAt = holder, Name = "B2" } };
        second.Add(new StationTrack(51, "1"));
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            Stop(2, siding, "08:30", "09:00"),
            Stop(3, second, "09:15", "09:45"),
            Stop(4, holder, "10:15", "10:25"));

        // Each note names its own location, so the driver knows which key is which.
        Assert.HasCount(2, train.Calls[0].LockKeyNotes);
        Assert.HasCount(2, train.Calls[3].LockKeyNotes);
        Assert.Contains("Pick up key B2 for unlocking Sågverket.", train.Calls[0].LockKeyNotes.Select(n => n.ToText));
    }

    [TestMethod]
    public void AKeyIsIgnoredOnceSomebodyIsOnDutyAtTheLocationItUnlocks()
    {
        var holder = MannedStation(1, "Göteborg", "G");
        var manned = new Station(2, "Bruket", "Bru") { IsManned = true };
        manned.LockKey = new LockKey { HeldAt = holder, Name = "A1" };
        manned.Add(new StationTrack(21, "1"));
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            Stop(2, manned, "08:30", "09:00"),
            Stop(3, holder, "09:30", "09:40"));

        // The key is kept — the manning may well be undone — but nothing is derived from it meanwhile.
        Assert.AreEqual(LockKeyFault.LocationIsManned, manned.LockKeyFault);
        Assert.IsNull(manned.EffectiveLockKey);
        Assert.IsNotNull(manned.LockKey, "The key is ignored, not thrown away.");
        Assert.IsEmpty(train.Calls[0].LockKeyNotes);
        Assert.IsEmpty(train.Calls[2].LockKeyNotes);
    }

    [TestMethod]
    public void AKeyIsIgnoredOnceTheStationHoldingItIsNoLongerManned()
    {
        var (holder, siding) = KeyAt("A1");
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            Stop(2, siding, "08:30", "09:00"),
            Stop(3, holder, "09:30", "09:40"));
        Assert.IsNotEmpty(train.Calls[0].LockKeyNotes, "In force while the holder is manned.");

        holder.IsManned = false;

        // Nobody is there to hand the key over, so it cannot be fetched.
        Assert.AreEqual(LockKeyFault.HolderIsNotManned, siding.LockKeyFault);
        Assert.IsNull(siding.EffectiveLockKey);
        Assert.IsNotNull(siding.LockKey);
        Assert.IsEmpty(train.Calls[0].LockKeyNotes);
        Assert.IsEmpty(train.Calls[2].LockKeyNotes);
    }

    [TestMethod]
    public void AKeyIsIgnoredWhereNothingIsWorkedAnyMore()
    {
        var holder = MannedStation(1, "Göteborg", "G");
        var station = new Station(2, "Bruket", "Bru") { IsManned = false };
        station.LockKey = new LockKey { HeldAt = holder, Name = "A1" };
        station.HasCargoExchange = false;

        // No cargo is exchanged there, so no siding is worked and no switch has to be unlocked.
        Assert.AreEqual(LockKeyFault.LocationExchangesNoCargo, station.LockKeyFault);
        Assert.IsNull(station.EffectiveLockKey);
    }

    [TestMethod]
    public void AKeyInForceHasNoFault()
    {
        var (_, siding) = KeyAt("A1");

        Assert.AreEqual(LockKeyFault.None, siding.LockKeyFault);
        Assert.AreSame(siding.LockKey, siding.EffectiveLockKey);
    }

    [TestMethod]
    public void ALocationWithNoKeyHasNoFault()
    {
        Assert.AreEqual(LockKeyFault.None, MannedStation(1, "Göteborg", "G").LockKeyFault);
        Assert.AreEqual(LockKeyFault.None, new IndustrialArea(2, "Bruket", "Bru").LockKeyFault);
    }

    [TestMethod]
    public void ALockKeyNoteIsOneOfTheDriversNotes()
    {
        var (holder, siding) = KeyAt("A1");
        var train = FreightTrain(
            Stop(1, holder, "08:00", "08:10"),
            Stop(2, siding, "08:30", "09:00"),
            Stop(3, holder, "09:30", "09:40"));

        var notes = train.Calls[0].DriverNotes(Sessions.All, SessionsSettings.UseSessions(14));

        Assert.IsInstanceOfType<PickUpLockKeyNote>(notes.Single());
    }

    // A manned station holding the key to an industrial area a freight train works.
    private static (Station Holder, IndustrialArea Siding) KeyAt(string keyName)
    {
        var holder = MannedStation(1, "Göteborg", "G");
        var siding = new IndustrialArea(2, "Bruket", "Bru") { LockKey = new LockKey { HeldAt = holder, Name = keyName } };
        siding.Add(new StationTrack(21, "1"));
        return (holder, siding);
    }

    private static Station MannedStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10 + 1, "1"));
        return station;
    }

    private static Train FreightTrain(params StationCall[] calls)
    {
        var train = new Train(1, Freight, 5001) { Category = Freight };
        foreach (var call in calls) train.Add(call);
        return train;
    }

    private static StationCall Stop(int id, OperationLocation at, string arrival, string departure) =>
        new(id, at["1"], Time.FromString(arrival), Time.FromString(departure)) { IsArrival = true, IsDeparture = true };

    private static StationCall PassThrough(int id, OperationLocation at, string time) =>
        new(id, at["1"], Time.FromString(time), Time.FromString(time));
}
