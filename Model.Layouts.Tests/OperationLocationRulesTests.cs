namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>What each kind of operation location can and can't have: by its type, and as checks with a code per broken rule.</summary>
[TestClass]
public class OperationLocationRulesTests
{
    private static Station Tellby() => new(1, "Tellby", "Tby") { CountryCode = "SE", Centre = 1.5 };

    [TestMethod]
    public void AStationWithEverythingBreaksNoRule() =>
        Assert.IsEmpty(OperationLocationRules.Validate(Tellby()));

    [TestMethod]
    public void NameAndSignatureAreRequired()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new Station(1, " ", "Tby"));
        Assert.ThrowsExactly<ArgumentException>(() => new Station(1, "Tellby", ""));
        var unnamed = Tellby();
        unnamed.Name = "";
        CollectionAssert.AreEqual(new[] { LocationProblemCode.NameMissing }, OperationLocationRules.Validate(unnamed).Select(p => p.Code).ToArray());
    }

    [TestMethod]
    [DataRow("SE", true)]
    [DataRow("se", false)]
    [DataRow("SWE", false)]
    public void TheCountryIsAnIsoCode(string code, bool isValid)
    {
        var location = Tellby();
        location.CountryCode = code;
        Assert.AreEqual(isValid, OperationLocationRules.Validate(location).Count == 0);
    }

    [TestMethod]
    public void TheKindIsTheType()
    {
        Assert.AreEqual(OperationLocationKind.Station, Tellby().Kind);
        Assert.AreEqual(OperationLocationKind.SignalControlled, new SignalControlledLocation(2, "Kvarntorp", "Kvt").Kind);
        Assert.AreEqual(OperationLocationKind.IndustrialArea, new IndustrialArea(3, "Sockerbruket", "Skb").Kind);
        Assert.AreEqual(OperationLocationKind.Other, new OtherLocation(4, "Hållplats", "Hp").Kind);
    }

    [TestMethod]
    public void ASignalControlledLocationExchangesNothing()
    {
        var location = new SignalControlledLocation(2, "Kvarntorp", "Kvt") { HasPassengerExchange = true, HasCargoExchange = true };
        Assert.IsFalse(location.HasPassengerExchange);
        Assert.IsFalse(location.HasCargoExchange);
    }

    [TestMethod]
    public void AnIndustrialAreaAlwaysExchangesCargoAndNeverPassengers()
    {
        var location = new IndustrialArea(3, "Sockerbruket", "Skb") { HasPassengerExchange = true, HasCargoExchange = false, IsChangingTrainDirectionPossible = true };
        Assert.IsTrue(location.HasCargoExchange);
        Assert.IsFalse(location.HasPassengerExchange);
        Assert.IsFalse(location.IsChangingTrainDirectionPossible);
    }

    [TestMethod]
    public void AHaltHasPassengersOnly()
    {
        var location = new OtherLocation(4, "Hållplats", "Hp") { HasCargoExchange = true };
        Assert.IsTrue(location.HasPassengerExchange);
        Assert.IsFalse(location.HasCargoExchange);
    }

    [TestMethod]
    public void AShadowStationAlwaysExchangesPassengersAndCargo()
    {
        var station = new Station(5, "Skuggby", "Sk") { IsShadow = true, HasPassengerExchange = false, HasCargoExchange = false };
        Assert.IsTrue(station.HasPassengerExchange);
        Assert.IsTrue(station.HasCargoExchange);
    }

    [TestMethod]
    public void AStationCrossesTrainsWhenItHasTwoScheduledTracks()
    {
        var station = Tellby();
        station.Tracks.Add(new StationTrack(1, "1"));
        Assert.IsFalse(OperationLocationRules.TrainsCanCrossAt(station));
        station.Tracks.Add(new StationTrack(2, "2"));
        Assert.IsTrue(OperationLocationRules.TrainsCanCrossAt(station));
        Assert.IsTrue(OperationLocationRules.TrainsCanCrossAt(new SignalControlledLocation(2, "Kvarntorp", "Kvt") { TrainsCanCross = true }));
        Assert.IsFalse(OperationLocationRules.TrainsCanCrossAt(new OtherLocation(4, "Hållplats", "Hp")));
    }

    [TestMethod]
    public void TrackDesignationsAreUniqueWithoutRegardToCase()
    {
        var problems = OperationLocationRules.Validate([new StationTrack(1, "2a"), new StationTrack(2, "2A"), new StationTrack(3, "")]);
        CollectionAssert.AreEquivalent(new[] { LocationProblemCode.TrackDesignationNotUnique, LocationProblemCode.TrackDesignationMissing }, problems.Select(p => p.Code).ToArray());
    }

    [TestMethod]
    public void AKeyNameNeedsALockableTrack()
    {
        Assert.AreEqual(LocationProblemCode.KeyNameWithoutLock, OperationLocationRules.Validate([new StationTrack(1, "1") { KeyName = "K1" }]).Single().Code);
        Assert.IsEmpty(OperationLocationRules.Validate([new StationTrack(1, "1") { KeyName = "K1", IsLockable = true }]));
    }

    [TestMethod]
    public void OnlyAnUnmannedLocationCanBeControlledFromAMannedStation()
    {
        var layout = new Layout();
        var controller = new Station(1, "Tellby", "Tby") { IsManned = true };
        var unmanned = new Station(2, "Ytterby", "Yb") { ControlledBy = controller };
        var halt = new OtherLocation(3, "Hållplats", "Hp") { ControlledBy = controller };
        var manned = new Station(4, "Kode", "Kd") { IsManned = true, ControlledBy = controller };
        foreach (var location in new OperationLocation[] { controller, unmanned, halt, manned }) layout.OperationLocations.Add(location);

        Assert.IsEmpty(OperationLocationRules.Validate(unmanned, layout));
        Assert.AreEqual(LocationProblemCode.ControlledByNotAllowed, OperationLocationRules.Validate(halt, layout).Single().Code);
        Assert.AreEqual(LocationProblemCode.ControlledByNotAllowed, OperationLocationRules.Validate(manned, layout).Single().Code);
        controller.IsManned = false;
        Assert.AreEqual(LocationProblemCode.ControlledByNotManned, OperationLocationRules.Validate(unmanned, layout).Single().Code);
    }
}
