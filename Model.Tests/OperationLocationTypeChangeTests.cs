namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class OperationLocationTypeChangeTests
{
    // A -> B -> C, with one track at B, used as the conversion subject.
    private static (Layout layout, Station a, Station b, Station c) LineABC()
    {
        var layout = new Layout { Name = "Test" };
        Station Add(string sig) => (Station)layout.Add(new Station(sig[0], sig, sig));
        var a = Add("A");
        var b = Add("B");
        var c = Add("C");
        layout.Add(new TrackStretch(1, a, b, 10));
        layout.Add(new TrackStretch(2, b, c, 10));
        b.Add(new StationTrack(10, "1"));
        return (layout, a, b, c);
    }

    [TestMethod]
    public void ChangingToSignalControlledReplacesTheInstanceAndKeepsItInPlace()
    {
        var (layout, _, b, _) = LineABC();

        var replacement = layout.ChangeOperationLocationType(b, OperationLocationKind.SignalControlled);

        Assert.IsInstanceOfType<SignalControlledLocation>(replacement);
        Assert.AreEqual(3, layout.OperationLocations.Count);
        Assert.AreSame(replacement, layout.OperationLocations.Single(o => o.Signature == "B"));
    }

    [TestMethod]
    public void ChangingToSignalControlledDefaultsControllerToThePrecedingStation()
    {
        var (layout, a, b, _) = LineABC();

        var replacement = (SignalControlledLocation)layout.ChangeOperationLocationType(b, OperationLocationKind.SignalControlled);

        // A -> B is the incoming stretch, so A is the station before B.
        Assert.AreSame(a, replacement.ControlledBy);
    }

    [TestMethod]
    public void ChangingTypeRepointsTrackStretchesAndTracks()
    {
        var (layout, _, b, _) = LineABC();

        var replacement = layout.ChangeOperationLocationType(b, OperationLocationKind.Other);

        Assert.AreSame(replacement, layout.TrackStretches.Single(s => s.Id == 1).End, "incoming stretch End");
        Assert.AreSame(replacement, layout.TrackStretches.Single(s => s.Id == 2).Start, "outgoing stretch Start");
        Assert.AreSame(replacement, replacement.Tracks.Single().Station, "carried-over track back-reference");
    }

    [TestMethod]
    public void ChangingAControllerStationDropsTheControlledByLink()
    {
        var (layout, a, _, _) = LineABC();
        var signal = new SignalControlledLocation(99, "S", "S") { ControlledBy = a };
        layout.Add(signal);

        layout.ChangeOperationLocationType(a, OperationLocationKind.Other);

        Assert.IsNull(signal.ControlledBy, "controller is no longer a station");
    }

    [TestMethod]
    public void ChangingToIndustrialAreaReplacesTheInstanceAndForcesItsFixedFlags()
    {
        var (layout, _, b, _) = LineABC();

        var replacement = (IndustrialArea)layout.ChangeOperationLocationType(b, OperationLocationKind.IndustrialArea);

        Assert.AreSame(replacement, layout.OperationLocations.Single(o => o.Signature == "B"));
        Assert.IsFalse(replacement.HasPassengerExchange, "no passenger exchange");
        Assert.IsTrue(replacement.HasCargoExchange, "always has cargo exchange");
        Assert.IsFalse(replacement.IsChangingTrainDirectionPossible, "no direction change");
    }

    [TestMethod]
    public void ChangingTypeCarriesOverHideMeetsAndHidePassings()
    {
        var (layout, _, b, _) = LineABC();
        b.HideMeets = true;
        b.HidePassings = true;

        var replacement = layout.ChangeOperationLocationType(b, OperationLocationKind.SignalControlled);

        Assert.IsTrue(replacement.HideMeets);
        Assert.IsTrue(replacement.HidePassings);
    }

    [TestMethod]
    public void ChangingTypeCarriesOverTheLockKey()
    {
        var (layout, a, b, _) = LineABC();
        b.IsManned = false;
        b.LockKey = new LockKey { HeldAt = a, Name = "A1" };

        // Turning a location into an industrial area is exactly when a key matters: nobody works it.
        var replacement = layout.ChangeOperationLocationType(b, OperationLocationKind.IndustrialArea);

        Assert.AreEqual("A1", replacement.LockKey?.Name);
        Assert.AreSame(a, replacement.LockKey?.HeldAt);
    }

    [TestMethod]
    public void ChangingAKeyHoldingStationDropsTheKeyItHeld()
    {
        var (layout, a, b, _) = LineABC();
        b.IsManned = false;
        b.LockKey = new LockKey { HeldAt = a, Name = "A1" };

        layout.ChangeOperationLocationType(a, OperationLocationKind.IndustrialArea);

        // Only a station can hold a key, so converting one always leaves the location it held the key
        // for without one, rather than pointing at a station the layout no longer has.
        Assert.IsNull(b.LockKey);
    }

    [TestMethod]
    public void ChangingToTheSameKindIsANoOp()
    {
        var (layout, _, b, _) = LineABC();

        var result = layout.ChangeOperationLocationType(b, OperationLocationKind.Station);

        Assert.AreSame(b, result);
    }
}
