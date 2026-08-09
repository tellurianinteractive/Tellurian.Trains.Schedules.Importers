using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// The platform along a station track: what having one means, and how a plan written before platforms
/// existed is given the ones it always implicitly had (see <c>Layout.EnsurePlatforms</c>).
/// </summary>
[TestClass]
public class PlatformTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    [TestMethod]
    public void ATrackWithoutAPlatformLengthHasNoPlatform()
    {
        var station = StationWithTracks(1, "Alpha", "A", 2);

        Assert.IsFalse(station.Tracks.Any(track => track.HasPlatform));
    }

    [TestMethod]
    public void PassengersAreExchangedOnlyAtATrackWithAPlatform()
    {
        var station = StationWithTracks(1, "Alpha", "A", 2);
        var platform = station.Tracks.First();
        platform.PlatformLength = 4.5;

        Assert.IsTrue(platform.HasPassengerExchange);
        Assert.IsFalse(station.Tracks.Last().HasPassengerExchange);
    }

    [TestMethod]
    public void APlatformExchangesNoPassengersWhereTheLocationExchangesNone()
    {
        var station = StationWithTracks(1, "Alpha", "A", 2);
        station.HasPassengerExchange = false;
        var track = station.Tracks.First();
        track.PlatformLength = 4.5;

        // The platform is still there — a passenger train may stand at it, for a meet — but nobody
        // gets on or off at a location that exchanges no passengers.
        Assert.IsTrue(track.HasPlatform);
        Assert.IsFalse(track.HasPassengerExchange);
    }

    [TestMethod]
    public void WhereNoTrackHasAPlatformThePassengerLocationGetsTheDefaultOnAllOfThem()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        layout.Add(StationWithTracks(1, "Alpha", "A", 3));

        Assert.IsTrue(layout.EnsurePlatforms());
        Assert.IsTrue(layout.StationTracks().All(track => track.PlatformLength == StationTrack.DefaultPlatformLength));
    }

    [TestMethod]
    public void ALocationThatAlreadyHasOnePlatformIsLeftAlone()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var station = StationWithTracks(1, "Alpha", "A", 3);
        layout.Add(station);
        station.Tracks.First().PlatformLength = 7.5;

        Assert.IsFalse(layout.EnsurePlatforms());
        Assert.AreEqual(7.5, station.Tracks.First().PlatformLength);
        // The planner has said which track has the platform; the others having none is the answer.
        Assert.IsTrue(station.Tracks.Skip(1).All(track => track.PlatformLength == 0));
    }

    [TestMethod]
    public void ALocationThatExchangesNoPassengersGetsNoPlatforms()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var station = StationWithTracks(1, "Alpha", "A", 2);
        station.HasPassengerExchange = false;
        layout.Add(station);

        Assert.IsFalse(layout.EnsurePlatforms());
        Assert.IsTrue(station.Tracks.All(track => track.PlatformLength == 0));
    }

    [TestMethod]
    public void EnsuringPlatformsTwiceChangesNothingTheSecondTime()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        layout.Add(StationWithTracks(1, "Alpha", "A", 2));

        Assert.IsTrue(layout.EnsurePlatforms());
        Assert.IsFalse(layout.EnsurePlatforms());
    }

    [TestMethod]
    public void APlanWrittenWithoutPlatformsReadsBackWithThem()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        layout.Add(StationWithTracks(1, "Alpha", "A", 2));
        var plan = Plan.Create("Plan", new Timetable("TT", layout) { Id = 1 });
        var json = JsonSerializer.Serialize(plan, Options);

        var restored = JsonSerializer.Deserialize<Plan>(json, Options);

        Assert.IsNotNull(restored);
        var tracks = restored.Timetable.Layout.StationTracks().ToList();
        Assert.HasCount(2, tracks);
        Assert.IsTrue(tracks.All(track => track.HasPassengerExchange));
    }

    [TestMethod]
    public void APlatformLengthSurvivesAPlanRoundTrip()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var station = StationWithTracks(1, "Alpha", "A", 2);
        layout.Add(station);
        station.Tracks.First().PlatformLength = 3.2;
        var plan = Plan.Create("Plan", new Timetable("TT", layout) { Id = 1 });
        var json = JsonSerializer.Serialize(plan, Options);

        var restored = JsonSerializer.Deserialize<Plan>(json, Options);

        Assert.IsNotNull(restored);
        var tracks = restored.Timetable.Layout.StationTracks().ToList();
        Assert.AreEqual(3.2, tracks[0].PlatformLength);
        Assert.AreEqual(0, tracks[1].PlatformLength);
    }

    private static Station StationWithTracks(int id, string name, string signature, int trackCount)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        for (var i = 0; i < trackCount; i++)
            station.Add(new StationTrack(id * 10 + i + 1, (i + 1).ToString(), isMain: i == 0, isScheduled: true));
        return station;
    }
}
