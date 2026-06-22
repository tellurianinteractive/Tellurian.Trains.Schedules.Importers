using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies that the distance-axis extent encloses the last station's last track, so it is not clipped.
/// Regression: the extent was re-derived with a different formula than <see cref="GraphScheduleDrawingExtensions.TrackOffset"/>,
/// which folded the last station's track fan-out inside the MinStationSpacing floor and under-reserved it
/// when KilometerSpacing was low. See the graphical schedule editor distance-clipping fix.
/// </summary>
[TestClass]
public class GraphExtentTests
{
    // Line A —dist— B where the terminal station B has more tracks than A (a shadow yard). The distance
    // factor (KilometerSpacing) is configurable so a low value can be exercised.
    private static GraphSchedule CreateGraph(TimeAxisDirection direction, double distance, int kilometerSpacing, int terminalTrackCount)
    {
        var layout = new Layout { Name = "Test" };
        var a = new Station(1, "Alpha", "A");
        a.Add(new StationTrack(11, "1"));
        a.Add(new StationTrack(12, "2"));
        var b = new Station(2, "Beta", "B");
        for (var t = 0; t < terminalTrackCount; t++) b.Add(new StationTrack(20 + t, (t + 1).ToString()));
        layout.Add(a);
        layout.Add(b);
        var stretch = new TrackStretch(1, a, b, distance, 1);
        layout.Add(stretch);
        var line = new TimetableStretch(1, "1");
        line.AddLast(stretch);
        layout.Add(line);

        var timetable = new Timetable("Test", layout);
        var settings = GraphSettings.Default with { AxisDirection = direction, KilometerSpacing = kilometerSpacing };
        return new GraphSchedule(line, timetable, settings);
    }

    [TestMethod]
    public void ExtentMatchesTheLastStationsLastTrackPosition()
    {
        var graph = CreateGraph(TimeAxisDirection.Horisontal, distance: 2, kilometerSpacing: 1, terminalTrackCount: 6);
        var lastStation = graph.Stations.Length - 1;
        var lastTrack = graph.Stations[lastStation].Tracks.Count - 1;
        Assert.AreEqual(graph.Y(lastStation, lastTrack), graph.MaxTrackOffset().Y);
    }

    [TestMethod]
    public void CanvasEnclosesTheLastTrackWithLowDistanceFactor()
    {
        // Low KilometerSpacing makes the per-stretch spacing fall to the MinStationSpacing floor; the
        // terminal station's six-track fan-out must still be reserved beyond it.
        var horizontal = CreateGraph(TimeAxisDirection.Horisontal, distance: 1, kilometerSpacing: 1, terminalTrackCount: 6);
        var lastStation = horizontal.Stations.Length - 1;
        var lastTrack = horizontal.Stations[lastStation].Tracks.Count - 1;
        Assert.IsTrue(horizontal.Height() > horizontal.Y(lastStation, lastTrack),
            "Horizontal canvas height must extend past the last station's last track.");

        var vertical = CreateGraph(TimeAxisDirection.Vertical, distance: 1, kilometerSpacing: 1, terminalTrackCount: 6);
        Assert.IsTrue(vertical.Width() > vertical.X(lastStation, lastTrack),
            "Vertical canvas width must extend past the last station's last track.");
    }
}
