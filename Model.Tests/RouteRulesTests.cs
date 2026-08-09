using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers the locations a train may be given a call at: rule T5 offered forwards, so the planner is
/// handed the locations that keep the route whole rather than told afterwards that it broke.
/// See <c>RouteRules</c>.
/// </summary>
/// <remarks>
/// The layout every test runs on. A main line A–B–C–D, a second way round from A to C through Bx, and a
/// branch from C to E. Every location has tracks, so a train can call anywhere on it.
/// <code>
///         Bx
///       /    \
///      A — B — C — D
///                \
///                 E
/// </code>
/// </remarks>
[TestClass]
public class RouteRulesTests
{
    private Layout Layout = default!;
    private OperationLocation A = default!, B = default!, Bx = default!, C = default!, D = default!, E = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        Layout = new Layout { Id = 1, Name = "Test" };
        A = AddStation(1, "Alingsås", "A");
        B = AddStation(2, "Borås", "B");
        Bx = AddStation(3, "Bollebygd", "Bx");
        C = AddStation(4, "Coldwater", "C");
        D = AddStation(5, "Dalsjöfors", "D");
        E = AddStation(6, "Ekenäs", "E");
        AddStretch(1, A, B);
        AddStretch(2, B, C);
        AddStretch(3, A, Bx);
        AddStretch(4, Bx, C);
        AddStretch(5, C, D);
        AddStretch(6, C, E);
    }

    // Two tracks everywhere, the second one displayed first, so a test that reads the display order
    // cannot pass by reading the order the tracks were added in.
    private OperationLocation AddStation(int id, string name, string signature)
    {
        var station = Layout.Add(new Station(id, name, signature));
        station.Add(new StationTrack(id * 10 + 1, "1") { DisplayOrder = 2 });
        station.Add(new StationTrack(id * 10 + 2, "2") { DisplayOrder = 1 });
        return station;
    }

    private void AddStretch(int id, OperationLocation from, OperationLocation to) =>
        Layout.Add(new TrackStretch(id, from, to, 1000));

    // A train calling at each location in turn, twenty minutes apart, on the first track of each.
    private static Train CreateTrain(params OperationLocation[] locations)
    {
        var train = new Train(1, 1000);
        var time = Time.FromHourAndMinute(8, 0);
        for (var i = 0; i < locations.Length; i++)
        {
            train.Add(new StationCall(i + 1, locations[i].TracksInDisplayOrder[0], time, time.AddMinutes(2)));
            time = time.AddMinutes(20);
        }
        return train;
    }

    private static List<string> Names(IEnumerable<OperationLocation> locations) => [.. locations.Select(l => l.Name)];

    // ---- Where a call may be moved to ------------------------------------------------------------

    [TestMethod]
    public void TheOnlyCallOfATrainMayBeAnywhere()
    {
        var train = CreateTrain(A);

        var choices = train.LocationChoicesFor(train.Calls[0]);

        Assert.HasCount(6, choices, "Nothing before or after it constrains the one call a train has.");
    }

    [TestMethod]
    public void AMiddleCallIsOfferedOnlyTheLocationsJoinedToBothItsNeighbours()
    {
        var train = CreateTrain(A, B, C);

        var choices = Names(train.LocationChoicesFor(train.CallsInRunOrder[1]));

        CollectionAssert.AreEquivalent(new[] { B.Name, Bx.Name }, choices,
            "Both ways round from A to C are offered, and nothing that would leave the route jumping a location.");
    }

    [TestMethod]
    public void TheLastCallIsOfferedTheLocationsTheTrainCanRunOnTo()
    {
        var train = CreateTrain(A, B, C);

        var choices = Names(train.LocationChoicesFor(train.CallsInRunOrder[2]));

        CollectionAssert.AreEquivalent(new[] { B.Name, C.Name }, choices,
            "On from B, the train reaches C, or changes track at B; D and E are a stretch further off.");
    }

    [TestMethod]
    public void TheLocationTheTrainCameFromIsNotOffered()
    {
        var train = CreateTrain(A, B, C);

        var choices = Names(train.LocationChoicesFor(train.CallsInRunOrder[2]));

        CollectionAssert.DoesNotContain(choices, A.Name,
            "The first leg sets the direction; the train does not run straight back over it.");
    }

    [TestMethod]
    public void TheSameLocationIsOfferedAgain()
    {
        var train = CreateTrain(A, B, C);

        var choices = Names(train.LocationChoicesFor(train.CallsInRunOrder[2]));

        CollectionAssert.Contains(choices, B.Name,
            "Two calls at one location is a train changing track there, which travels no stretch.");
    }

    [TestMethod]
    public void ACallsOwnLocationIsOfferedEvenWhenTheRouteAlreadyBreaksTheRules()
    {
        // B and D have no stretch between them, so this route already jumps a location — as an import or a
        // hand-edited file can leave it. The drop-down still has to show where the train actually is.
        var train = CreateTrain(A, B, D);

        var choices = Names(train.LocationChoicesFor(train.CallsInRunOrder[1]));

        CollectionAssert.Contains(choices, B.Name);
    }

    [TestMethod]
    public void ACallOfAnotherTrainIsOfferedOnlyItsOwnLocation()
    {
        var train = CreateTrain(A, B, C);
        var other = CreateTrain(D, C);

        var choices = Names(train.LocationChoicesFor(other.Calls[0]));

        CollectionAssert.AreEquivalent(new[] { D.Name }, choices,
            "A call the train does not hold has no place in its run order, so nothing is offered but where it is.");
    }

    // ---- Where a call added at the end may be -----------------------------------------------------

    [TestMethod]
    public void ATrainWithNoCallsMayStartAnywhere()
    {
        var train = new Train(1, 1000);

        Assert.HasCount(6, train.LocationChoicesForNextCall(Layout));
    }

    [TestMethod]
    public void ACallAddedAtTheEndCarriesOnFromTheLastOne()
    {
        var train = CreateTrain(A, B, C);

        var choices = Names(train.LocationChoicesForNextCall(Layout));

        CollectionAssert.AreEquivalent(new[] { Bx.Name, C.Name, D.Name, E.Name }, choices,
            "On to Bx, D or E, or a track change at C; B is where the train came from.");
    }

    [TestMethod]
    public void ALocationWithNoTrackIsNotOffered()
    {
        var trackless = Layout.Add(new Station(7, "Falköping", "F"));
        AddStretch(7, C, trackless);
        var train = CreateTrain(A, B, C);

        var choices = Names(train.LocationChoicesForNextCall(Layout));

        CollectionAssert.DoesNotContain(choices, trackless.Name,
            "A train has nowhere to call at a location with no track, however well the layout joins it up.");
    }

    // ---- The tracks a location offers -------------------------------------------------------------

    [TestMethod]
    public void TracksAreOfferedInDisplayOrder()
    {
        var tracks = A.TracksInDisplayOrder;

        Assert.AreEqual("2", tracks[0].Number, "The planner's order, not the order the tracks were added in.");
        Assert.AreEqual("1", tracks[1].Number);
    }
}
