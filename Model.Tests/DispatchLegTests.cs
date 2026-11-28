using Tellurian.Trains.Schedules.Model.Dispatching;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Who has to agree on each part of a train's run. The layout:
/// <code>
///  Alpha ── Bj ── Charlie ── Uppby ── Echo
///           │        │         │
///         Delta     Xtra      Yard
/// </code>
/// Alpha, Charlie, Delta and Echo are manned; the junction Bj has signals worked from Alpha; Uppby is an
/// unmanned station on the line Charlie–Echo, where a spur without signals leads to the industrial area Yard;
/// Xtra is an industrial area on a branch nobody controls.
/// </summary>
[TestClass]
public class DispatchLegTests
{
    private static readonly TrainCategory Category = new() { Id = 1, Name = "Goods", Prefix = "G" };

    private static Timetable NewLayout()
    {
        var layout = new Layout { Name = "Test" };
        var a = layout.Add(new Station(1, "Alpha", "A") { IsManned = true });
        var bj = layout.Add(new SignalControlledLocation(2, "Bravo junction", "Bj"));
        var c = layout.Add(new Station(3, "Charlie", "C") { IsManned = true });
        var d = layout.Add(new Station(4, "Delta", "D") { IsManned = true });
        var u = layout.Add(new Station(5, "Uppby", "U"));
        var e = layout.Add(new Station(6, "Echo", "E") { IsManned = true });
        var x = layout.Add(new IndustrialArea(7, "Xtra", "X"));
        var y = layout.Add(new IndustrialArea(8, "Yard", "Y"));
        bj.ControlledBy = (Station)a;
        foreach (var place in new[] { a, bj, c, d, u, e, x, y }) place.Add(new StationTrack(place.Id * 10, "1"));
        layout.Add(new TrackStretch(1, a, bj, 10));
        layout.Add(new TrackStretch(2, bj, c, 10));
        layout.Add(new TrackStretch(3, d, bj, 10));
        layout.Add(new TrackStretch(4, c, u, 10));
        layout.Add(new TrackStretch(5, u, e, 10));
        layout.Add(new TrackStretch(6, c, x, 10));
        layout.Add(new TrackStretch(7, u, y, 10));
        var timetable = new Timetable("Test", layout);
        timetable.DispatchStretches = layout.CreateDispatchStretches();
        return timetable;
    }

    private static Train NewTrain(Timetable timetable, params string[] signatures)
    {
        var train = new Train(1, Category, 101) { Category = Category };
        var time = Time.FromHourAndMinute(10, 0);
        var id = 1;
        foreach (var signature in signatures)
        {
            var place = timetable.Layout.OperationLocations.Single(l => l.Signature == signature);
            _ = train.Add(new StationCall(id++, place["1"], time, time.AddMinutes(1)));
            time = time.AddMinutes(10);
        }
        // The train is not added, so that each test can make several with the same number; it only needs its timetable.
        train.Timetable = timetable;
        return train;
    }

    private static string Describe(DispatchLeg leg) =>
        $"{leg.From.OperationLocation.Signature}-{leg.To.OperationLocation.Signature} {leg.Kind} {leg.FromDispatcher?.Signature ?? "-"}/{leg.ToDispatcher?.Signature ?? "-"}";

    private static string[] Legs(Timetable timetable, params string[] signatures) =>
        [.. NewTrain(timetable, signatures).DispatchLegs.Select(Describe)];

    [TestMethod]
    public void JunctionWorkedFromAStationSplitsTheLine()
    {
        var layout = NewLayout();

        CollectionAssert.AreEqual(
            new[] { "A-Bj OneDispatcher A/A", "Bj-C BetweenDispatchers A/C" },
            Legs(layout, "A", "Bj", "C"));
    }

    [TestMethod]
    public void TrainFromTheOtherLineIsClearedWithTheStationWorkingTheJunction()
    {
        var layout = NewLayout();

        CollectionAssert.AreEqual(
            new[] { "D-Bj BetweenDispatchers D/A", "Bj-C BetweenDispatchers A/C" },
            Legs(layout, "D", "Bj", "C"));
    }

    [TestMethod]
    public void TrainEndingAtAnUnmannedStationIsClearedBetweenTheEndsOfTheStretch()
    {
        var layout = NewLayout();

        CollectionAssert.AreEqual(new[] { "C-U BetweenDispatchers C/E" }, Legs(layout, "C", "U"));
        CollectionAssert.AreEqual(new[] { "U-E BetweenDispatchers C/E" }, Legs(layout, "U", "E"));
    }

    [TestMethod]
    public void TrainPassingAnUnmannedStationIsOneLeg()
    {
        var layout = NewLayout();

        CollectionAssert.AreEqual(new[] { "C-E BetweenDispatchers C/E" }, Legs(layout, "C", "U", "E"));
    }

    [TestMethod]
    public void TrainToAndFromABranchIsClearedByTheStationNextToIt()
    {
        var layout = NewLayout();

        CollectionAssert.AreEqual(new[] { "C-X DriverCalls C/-" }, Legs(layout, "C", "X"));
        CollectionAssert.AreEqual(new[] { "X-C DriverCalls -/C", "C-U BetweenDispatchers C/E" }, Legs(layout, "X", "C", "U"));
    }

    [TestMethod]
    public void TrainLeavingTheLineAtAJunctionWithoutSignalsIsClearedBetweenTheEndsOfTheStretch()
    {
        var layout = NewLayout();

        CollectionAssert.AreEqual(new[] { "C-U BetweenDispatchers C/E" }, Legs(layout, "C", "U", "Y"));
        CollectionAssert.AreEqual(new[] { "U-E BetweenDispatchers C/E" }, Legs(layout, "Y", "U", "E"));
    }
}
