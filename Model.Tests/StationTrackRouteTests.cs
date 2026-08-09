using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Which way through a location a track is for (see <c>StationTrack.PreviousLocationId</c>), and how
/// that decides the track a train is put on (see <c>OperationLocation.PreferredTrack</c>).
/// </summary>
[TestClass]
public class StationTrackRouteTests
{
    private const int West = 1;
    private const int East = 2;
    private const int North = 3;

    [TestMethod]
    public void ATrackNamingNoLocationFitsEveryTrainWithoutClaimingOne()
    {
        var track = Track("1");

        Assert.IsFalse(track.HasRoute);
        Assert.AreEqual(0, track.RouteMatch(West, East));
        Assert.AreEqual(0, track.RouteMatch(null, null));
    }

    [TestMethod]
    public void ATrackNamingBothEndsFitsTheTrainRunningThatWayBest()
    {
        var track = Track("1", previous: West, next: East);

        Assert.IsTrue(track.RouteMatch(West, East) > track.RouteMatch(null, East));
        Assert.IsTrue(track.RouteMatch(West, East) > Track("2").RouteMatch(West, East));
    }

    [TestMethod]
    public void ATrackNamingBothEndsIsNotForATrainRunningTheOtherWay()
    {
        var track = Track("1", previous: West, next: East);

        Assert.IsNull(track.RouteMatch(East, West));
    }

    [TestMethod]
    public void BothWaysRoundMakesTheSameTrackFitTheOppositeDirectionEquallyWell()
    {
        var track = Track("1", previous: West, next: East, bothDirections: true);

        Assert.AreEqual(track.RouteMatch(West, East), track.RouteMatch(East, West));
        Assert.IsTrue(track.RouteMatch(East, West) > 0);
    }

    [TestMethod]
    public void ATrackIsNotForATrainComingFromSomewhereElse()
    {
        var track = Track("1", previous: West, next: East);

        Assert.IsNull(track.RouteMatch(North, East));
    }

    [TestMethod]
    public void ATrainStartingHereContradictsNothingButLeavesTheOtherEndUnconfirmed()
    {
        var both = Track("1", previous: West, next: East);
        var onward = Track("2", next: East);

        // A train starting here has no previous location to contradict the named one — but it does not
        // confirm it either, so the track that names only where the train goes on to fits it better.
        Assert.IsNotNull(both.RouteMatch(null, East));
        Assert.IsTrue(onward.RouteMatch(null, East) > both.RouteMatch(null, East));
    }

    [TestMethod]
    public void ATrackNamingOneEndOnlyFitsEveryTrainReachingItThatWay()
    {
        var track = Track("1", next: East);

        Assert.AreEqual(track.RouteMatch(West, East), track.RouteMatch(North, East));
        Assert.IsTrue(track.RouteMatch(North, East) > 0);
        Assert.IsNull(track.RouteMatch(West, North));
    }

    [TestMethod]
    public void ATrackNamingAnEndTheTrainNeverReachesIsWorseThanOneNamingNothing()
    {
        var named = Track("1", previous: West);
        var plain = Track("2");

        // A train starting here is not the train from West the track is for, so the plain track is the
        // better place for it — but the named one is still allowed, nothing having been contradicted.
        Assert.IsTrue(named.RouteMatch(null, East) < plain.RouteMatch(null, East));
        Assert.IsNotNull(named.RouteMatch(null, East));
    }

    [TestMethod]
    public void TheTrackNamedForTheTrainsRouteIsPreferredToTheMainTrack()
    {
        var location = LocationWithTracks();
        location["1"].IsMain = true;
        location["2"].PreviousLocationId = West;
        location["2"].NextLocationId = East;

        var track = location.PreferredTrack(Location(West), Location(East), TrackPreference.MainTrack);

        Assert.AreEqual("2", track?.Number);
    }

    [TestMethod]
    public void EachDirectionOfADoubleLineTakesItsOwnTrack()
    {
        var location = LocationWithTracks();
        location["1"].PreviousLocationId = West;
        location["1"].NextLocationId = East;
        location["2"].PreviousLocationId = East;
        location["2"].NextLocationId = West;

        Assert.AreEqual("1", location.PreferredTrack(Location(West), Location(East), TrackPreference.MainTrack)?.Number);
        Assert.AreEqual("2", location.PreferredTrack(Location(East), Location(West), TrackPreference.MainTrack)?.Number);
    }

    [TestMethod]
    public void ATrainPassingThroughTakesTheMainOfTheTracksNamedForItsRoute()
    {
        var location = LocationWithTracks("1", "2", "3");
        foreach (var track in location.Tracks)
        {
            track.PreviousLocationId = West;
            track.NextLocationId = East;
        }
        location["3"].IsMain = true;

        var chosen = location.PreferredTrack(Location(West), Location(East), TrackPreference.MainTrack);

        Assert.AreEqual("3", chosen?.Number);
    }

    [TestMethod]
    public void APassengerTrainStoppingTakesThePlatformOfTheTracksNamedForItsRoute()
    {
        var location = LocationWithTracks("1", "2", "3");
        foreach (var track in location.Tracks)
        {
            track.PreviousLocationId = West;
            track.NextLocationId = East;
        }
        location["1"].IsMain = true;
        location["3"].PlatformLength = 4.5;

        var chosen = location.PreferredTrack(Location(West), Location(East), TrackPreference.Platform);

        Assert.AreEqual("3", chosen?.Number);
    }

    [TestMethod]
    public void ATrainIsPutSomewhereEvenWhenEveryTrackIsReservedForAnotherWayThrough()
    {
        var location = LocationWithTracks();
        location["1"].PreviousLocationId = East;
        location["1"].NextLocationId = West;
        location["1"].IsMain = true;
        location["2"].PreviousLocationId = East;
        location["2"].NextLocationId = West;

        var chosen = location.PreferredTrack(Location(West), Location(North), TrackPreference.MainTrack);

        Assert.AreEqual("1", chosen?.Number);
    }

    [TestMethod]
    public void AnUnscheduledTrackIsNotTakenOverAScheduledOneWhateverItsRouteSays()
    {
        var location = LocationWithTracks();
        location["1"].IsScheduled = false;
        location["1"].PreviousLocationId = West;
        location["1"].NextLocationId = East;

        var chosen = location.PreferredTrack(Location(West), Location(East), TrackPreference.MainTrack);

        Assert.AreEqual("2", chosen?.Number);
    }

    [TestMethod]
    public void ALocationWithoutTracksHasNoTrackToOffer()
    {
        var location = new Station(10, "Empty", "Em");

        Assert.IsNull(location.PreferredTrack(Location(West), Location(East), TrackPreference.MainTrack));
    }

    [TestMethod]
    public void RemovingALocationReleasesTheTracksReservedForTrainsToAndFromIt()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var west = layout.Add(Location(West));
        var location = layout.Add(LocationWithTracks());
        location["1"].PreviousLocationId = West;
        location["1"].NextLocationId = East;
        location["1"].AppliesInBothDirections = true;

        Assert.IsTrue(layout.ForgetTrackRoutesTo(west));
        Assert.IsNull(location["1"].PreviousLocationId);
        Assert.AreEqual(East, location["1"].NextLocationId);
        // The other end still names a location, so the route — and the flag reversing it — remain.
        Assert.IsTrue(location["1"].AppliesInBothDirections);
    }

    [TestMethod]
    public void ATrackLeftNamingNothingIsNoLongerReversed()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var west = layout.Add(Location(West));
        var location = layout.Add(LocationWithTracks());
        location["1"].PreviousLocationId = West;
        location["1"].AppliesInBothDirections = true;

        Assert.IsTrue(layout.ForgetTrackRoutesTo(west));
        Assert.IsFalse(location["1"].HasRoute);
        Assert.IsFalse(location["1"].AppliesInBothDirections);
    }

    [TestMethod]
    public void ATracksRouteMayNameOnlyTheLocationsATrainCanReachItFrom()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var west = layout.Add(Location(West));
        var east = layout.Add(Location(East));
        var north = layout.Add(Location(North));
        var middle = layout.Add(LocationWithTracks());
        layout.Add(new TrackStretch(1, west, middle, 5, 1));
        layout.Add(new TrackStretch(2, middle, east, 5, 1));

        var neighbours = layout.NeighboursOf(middle);

        CollectionAssert.AreEquivalent(new[] { west, east }, neighbours.ToArray());
        CollectionAssert.DoesNotContain(neighbours.ToArray(), north);
    }

    [TestMethod]
    public void ATracksRouteSurvivesAPlanRoundTrip()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var location = layout.Add(LocationWithTracks());
        location["1"].PreviousLocationId = West;
        location["1"].NextLocationId = East;
        location["1"].AppliesInBothDirections = true;
        var plan = Plan.Create("Plan", new Timetable("TT", layout) { Id = 1 });
        var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve, MaxDepth = 256 };

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, options), options);

        Assert.IsNotNull(restored);
        var track = restored.Timetable.Layout.StationTracks().First(t => t.Number == "1");
        Assert.AreEqual(West, track.PreviousLocationId);
        Assert.AreEqual(East, track.NextLocationId);
        Assert.IsTrue(track.AppliesInBothDirections);
    }

    private static StationTrack Track(string number, int? previous = null, int? next = null, bool bothDirections = false) =>
        new(1, number)
        {
            PreviousLocationId = previous,
            NextLocationId = next,
            AppliesInBothDirections = bothDirections,
            Station = LocationWithTracks(),
        };

    // A location whose tracks are all plain: scheduled, no main track, no platform and no route, so each
    // test says for itself what makes one track different from another.
    private static Station LocationWithTracks(params string[] numbers)
    {
        var station = new Station(10, "Middle", "Mi") { IsManned = true };
        foreach (var number in numbers.Length > 0 ? numbers : ["1", "2"])
            station.Add(new StationTrack(10 * 10 + station.Tracks.Count + 1, number, isMain: false, isScheduled: true));
        return station;
    }

    private static Station Location(int id) => new(id, $"Location {id}", $"L{id}") { IsManned = true };
}
