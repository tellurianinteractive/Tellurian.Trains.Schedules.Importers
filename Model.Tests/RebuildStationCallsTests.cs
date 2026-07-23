using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class RebuildStationCallsTests
{
    [TestMethod]
    public void DropsTrackCallsWhoseTrainIsNoLongerInTheTimetable()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var orphan = timetable.Trains.First();
        var orphanCall = orphan.Calls.First();
        var track = orphanCall.Track;

        // Simulate a persisted/imported plan whose train was dropped from Timetable.Trains without its
        // calls being removed from the tracks (the exact inconsistency that produced phantom conflicts).
        timetable.Trains.Remove(orphan);
        Assert.IsTrue(track.Calls.Any(c => ReferenceEquals(c.Train, orphan)),
            "Precondition: the track still holds the orphaned call.");

        timetable.RebuildStationCalls();

        Assert.IsFalse(track.Calls.Any(c => ReferenceEquals(c.Train, orphan)),
            "The orphaned call is dropped from the per-track index.");
    }

    [TestMethod]
    public void KeepsTrackCallsOfTrainsStillInTheTimetable()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();

        timetable.RebuildStationCalls();

        foreach (var call in timetable.Trains.SelectMany(t => t.Calls))
            Assert.IsTrue(call.Track.Calls.Contains(call),
                "Every call of a train in the timetable remains indexed on its track.");
    }

    [TestMethod]
    public void IsIdempotent()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var track = timetable.Trains.First().Calls.First().Track;
        var expected = track.Calls.Count;

        timetable.RebuildStationCalls();
        timetable.RebuildStationCalls();

        Assert.AreEqual(expected, track.Calls.Count,
            "Rebuilding twice must not duplicate calls on a track.");
    }
}
