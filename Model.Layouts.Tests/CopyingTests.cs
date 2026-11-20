namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// Copying the shared types, for an app that keeps earlier states of what it edits, and changing the kind of a location.
/// </summary>
[TestClass]
public class CopyingTests
{
    private static Station Tellby()
    {
        var station = new Station(1, "Tellby", "Tby") { IsManned = true, HasTurntable = true, Instructions = "Shunt on track 3.", LockKey = new() { Name = "K" } };
        station.Tracks.Add(new StationTrack(1, "1") { Station = station, Usage = "Passenger" });
        station.StaffPosts.Add(new() { Roles = StaffRoles.Dispatcher });
        station.Timings.MinimumStopMinutes = 3;
        return station;
    }

    [TestMethod]
    public void WithChangesACopyAndLeavesTheOriginal()
    {
        var original = Tellby();

        var changed = original.With(s => s.Name = "Ytterby");

        Assert.AreEqual("Tellby", original.Name);
        Assert.AreEqual("Ytterby", changed.Name);
        Assert.IsInstanceOfType<Station>(changed);
    }

    [TestMethod]
    public void WhatALocationOwnsIsCopiedToo()
    {
        var original = Tellby();

        var copy = (Station)original.Copy();
        copy.Tracks.Single().Usage = "Goods";
        copy.StaffPosts.Single().Roles = StaffRoles.Shuntman;
        copy.Timings.MinimumStopMinutes = 5;
        copy.LockKey!.Name = "L";

        Assert.AreEqual("Passenger", original.Tracks.Single().Usage);
        Assert.AreEqual(StaffRoles.Dispatcher, original.StaffPosts.Single().Roles);
        Assert.AreEqual(3, original.Timings.MinimumStopMinutes);
        Assert.AreEqual("K", original.LockKey!.Name);
        Assert.AreSame(copy, copy.Tracks.Single().Station, "a copied track belongs to the copied location");
    }

    [TestMethod]
    public void WhatALocationRefersToIsNotCopied()
    {
        var controller = new Station(2, "Kode", "Kd") { IsManned = true };
        var original = new Station(1, "Tellby", "Tby") { ControlledBy = controller };

        Assert.AreSame(controller, original.Copy().ControlledBy);
    }

    [TestMethod]
    public void AModuleIsCopiedWithItsLocationAndTracks()
    {
        var module = new Module { Name = "Tellby", OperationLocation = Tellby(), Tracks = { new StationTrack(9, "9") } };

        var copy = module.With(m => m.Tracks.Single().Designation = "10");

        Assert.AreEqual("9", module.Tracks.Single().Designation);
        Assert.AreNotSame(module.OperationLocation, copy.OperationLocation);
        Assert.AreEqual("Tellby", copy.OperationLocation!.Name);
    }

    [TestMethod]
    public void AModuleWhoseTracksAreItsLocationsKeepsThemOneList()
    {
        var location = Tellby();
        var module = new Module { Name = "Tellby", OperationLocation = location, Tracks = location.Tracks };

        var copy = module.Copy();

        Assert.AreSame(copy.OperationLocation!.Tracks, copy.Tracks);
    }

    [TestMethod]
    public void ALocationWithoutANameCanBeStarted()
    {
        var location = OperationLocation.Create(OperationLocationKind.IndustrialArea);

        Assert.IsInstanceOfType<IndustrialArea>(location);
        Assert.AreEqual(LocationProblemCode.NameMissing, OperationLocationRules.Validate(location).First().Code);
    }

    [TestMethod]
    public void ChangingKindKeepsWhatTheNewKindCanHave()
    {
        var station = Tellby();

        var area = OperationLocationRules.Conforming(station, OperationLocationKind.IndustrialArea);

        Assert.IsInstanceOfType<IndustrialArea>(area);
        Assert.AreEqual(1, area.Id);
        Assert.AreEqual("Tellby", area.Name);
        Assert.AreEqual("Shunt on track 3.", area.Instructions);
        Assert.AreEqual("K", area.LockKey!.Name);
        Assert.IsTrue(area.HasCargoExchange);
        Assert.AreSame(area, area.Tracks.Single().Station, "the tracks move to the new location");
        Assert.AreSame(station, station.Tracks.Single().Station, "the old location keeps its own");
    }

    [TestMethod]
    public void ChangingKindTellsWhatIsCleared()
    {
        var station = Tellby();

        CollectionAssert.AreEquivalent(
            new[] { nameof(Station.HasTurntable), nameof(Station.IsManned), nameof(OperationLocation.HasPassengerExchange), nameof(OperationLocation.HasCargoExchange), nameof(OperationLocation.Instructions) },
            OperationLocationRules.ClearedBy(station, OperationLocationKind.SignalControlled).ToArray());
        Assert.IsEmpty(OperationLocationRules.ClearedBy(station, OperationLocationKind.Station));
    }
}
