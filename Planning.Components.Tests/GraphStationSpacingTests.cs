using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies that the minimum spacing between two operation locations on the distance axis is a floor on the
/// gap between them — from the last track of one to the first track of the next — and not on their track-zero
/// positions. Regression: the track fan-out was folded inside the floor, so a station with many tracks drew
/// its tracks over the following station's.
/// </summary>
[TestClass]
public class GraphStationSpacingTests
{
    // Line A —dist— B, where each station's track count and the distance between them are configurable.
    private static GraphSchedule CreateGraph(int firstTrackCount, int secondTrackCount, double distance)
    {
        var layout = new Layout { Name = "Test" };
        var a = new Station(1, "Alpha", "A");
        for (var t = 0; t < firstTrackCount; t++) a.Add(new StationTrack(10 + t, (t + 1).ToString()));
        var b = new Station(2, "Beta", "B");
        for (var t = 0; t < secondTrackCount; t++) b.Add(new StationTrack(20 + t, (t + 1).ToString()));
        layout.Add(a);
        layout.Add(b);
        var stretch = new TrackStretch(1, a, b, distance, 1);
        layout.Add(stretch);
        var line = new TimetableStretch(1, "1");
        line.AddLast(stretch);
        layout.Add(line);

        var timetable = new Timetable("Test", layout);
        var settings = GraphSettings.Default with { AxisDirection = TimeAxisDirection.Horisontal, KilometerSpacing = 3 };
        return new GraphSchedule(line, timetable, settings);
    }

    private static int Gap(GraphSchedule graph) =>
        graph.Y(1, 0) - graph.Y(0, graph.StationTracks[0].Length - 1);

    [TestMethod]
    public void MinimumSpacingIsTheGapBetweenTheLastAndTheFirstTrack()
    {
        // A twelve-track station fans out over more than the minimum spacing; the next station must still be
        // a full minimum spacing beyond its last track. Distance is short, so the floor governs.
        var graph = CreateGraph(firstTrackCount: 12, secondTrackCount: 3, distance: 1);
        Assert.AreEqual(graph.GraphSettings.MinStationSpacing, Gap(graph));
    }

    [TestMethod]
    public void TrackCountDoesNotChangeTheGapToTheNextLocation()
    {
        var few = CreateGraph(firstTrackCount: 2, secondTrackCount: 3, distance: 1);
        var many = CreateGraph(firstTrackCount: 12, secondTrackCount: 3, distance: 1);
        Assert.AreEqual(Gap(few), Gap(many), "The gap is measured from the last track, so the fan-out does not eat into it.");
    }

    [TestMethod]
    public void DistanceGovernsTheGapWhenItExceedsTheMinimum()
    {
        var graph = CreateGraph(firstTrackCount: 12, secondTrackCount: 3, distance: 50);
        var settings = graph.GraphSettings;
        Assert.AreEqual((int)Math.Round(settings.KilometerSpacing * 50.0), Gap(graph));
    }
}
