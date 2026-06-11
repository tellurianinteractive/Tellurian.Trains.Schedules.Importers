using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies that the graphical timetable handles trains running past 24:00: the axis is forced to the
/// full day 00:00–24:00 and a line crossing the 24:00 boundary is split so the after-midnight part
/// wraps from the right edge to the left edge at the same station height.
/// See GraphScheduleDrawingExtensions and the past-midnight reference note.
/// </summary>
[TestClass]
public class GraphWrappingTests
{
    private static readonly TimeSpan OneDay = TimeSpan.FromHours(24);

    // Two-station line A —10m— B, single track each. Optionally adds a train with the given times so the
    // graph enters full-day (past-midnight) mode. Returns the graph for stretch A–B in horizontal mode.
    private static GraphSchedule CreateGraph(Time? departureFromA = null, Time? arrivalAtB = null)
    {
        var layout = new Layout { Name = "Test" };
        var a = new Station(1, "Alpha", "A");
        a.Add(new StationTrack(11, "1"));
        var b = new Station(2, "Beta", "B");
        b.Add(new StationTrack(21, "1"));
        layout.Add(a);
        layout.Add(b);
        var stretch = new TrackStretch(1, a, b, 10, 1);
        layout.Add(stretch);
        var line = new TimetableStretch(1, "1");
        line.AddLast(stretch);
        layout.Add(line);

        var timetable = new Timetable("Test", layout);
        if (departureFromA is { } dep && arrivalAtB is { } arr)
        {
            var category = new TrainCategory { Id = 1, Prefix = "G", Name = "G" };
            var train = new Train(1, category, 1) { Category = category };
            _ = train.Add(new StationCall(1, a["1"], dep, dep));
            _ = train.Add(new StationCall(2, b["1"], arr, arr));
            timetable.Add(train);
        }
        return new GraphSchedule(line, timetable);
    }

    [TestMethod]
    public void AxisIsForcedToFullDayWhenATrainCrossesMidnight()
    {
        var graph = CreateGraph(Time.FromHourAndMinute(23, 50), Time.FromDayHourMinute(1, 0, 10)); // 23:50 -> 24:10
        Assert.AreEqual(TimeSpan.Zero, graph.StartTime);
        Assert.AreEqual(OneDay, graph.EndTime);
    }

    [TestMethod]
    public void AxisKeepsOperatingWindowWhenNoTrainCrossesMidnight()
    {
        var graph = CreateGraph(Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 30));
        // No past-midnight call -> falls back to GraphSettings.Default window (06:00–20:00).
        Assert.AreEqual(TimeSpan.FromHours(6), graph.StartTime);
        Assert.AreEqual(TimeSpan.FromHours(20), graph.EndTime);
    }

    [TestMethod]
    public void BetweenStationsLineWithinTheDayIsASingleSegment()
    {
        var graph = CreateGraph(Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 30));
        var segments = graph.TrainBetweenStationsLine(0, 0, 1, 0, Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 30)).ToList();
        Assert.AreEqual(1, segments.Count);
        Assert.AreEqual(graph.TimeOffset(TimeSpan.FromHours(8)).X, segments[0].Start.X);
    }

    [TestMethod]
    public void BetweenStationsLineCrossingMidnightSplitsAndWrapsToTheStart()
    {
        var dep = Time.FromHourAndMinute(23, 50);
        var arr = Time.FromDayHourMinute(1, 0, 10); // 24:10
        var graph = CreateGraph(dep, arr);

        var segments = graph.TrainBetweenStationsLine(0, 0, 1, 0, dep, arr).ToList();

        Assert.AreEqual(2, segments.Count, "A line crossing 24:00 should split into two segments.");

        var leftEdgeX = graph.TimeOffset(TimeSpan.Zero).X;
        var rightEdgeX = graph.MaxTimeOffset().X;

        // Segment 1: 23:50 -> 24:00, ending at the right edge.
        Assert.AreEqual(graph.TimeOffset(dep.Value).X, segments[0].Start.X, "First segment starts at 23:50.");
        Assert.AreEqual(rightEdgeX, segments[0].End.X, "First segment ends at the right edge (24:00).");

        // Segment 2: 00:00 -> 00:10, starting at the left edge.
        Assert.AreEqual(leftEdgeX, segments[1].Start.X, "Second segment starts at the left edge (00:00).");
        Assert.AreEqual(graph.TimeOffset(TimeSpan.FromMinutes(10)).X, segments[1].End.X, "Second segment ends at 00:10.");

        // The line leaves the right edge and re-enters the left edge at the same station height.
        Assert.AreEqual(segments[0].End.Y, segments[1].Start.Y, "Cross-axis position is continuous across the wrap.");

        // The boundary height is interpolated: 23:50->24:10 is 20 min and the boundary is at 10 min (half),
        // so the wrap height is midway between station A and station B.
        var yA = graph.Y(0, 0);
        var yB = graph.Y(1, 0);
        Assert.AreEqual((yA + yB) / 2, segments[0].End.Y);
    }

    [TestMethod]
    public void AtStationLineCrossingMidnightSplitsAtTheSameTrackHeight()
    {
        var graph = CreateGraph(Time.FromHourAndMinute(23, 50), Time.FromDayHourMinute(1, 0, 10)); // forces full-day mode
        var stop = new StationCall(99, graph.Stations[0]["1"], Time.FromHourAndMinute(23, 55), Time.FromDayHourMinute(1, 0, 5)); // 23:55 -> 24:05

        var segments = graph.TrainAtStationLine(0, 0, stop).ToList();

        Assert.AreEqual(2, segments.Count);
        var y = graph.Y(0, 0);
        // A stop stays on one track, so every endpoint is at that track's height.
        Assert.IsTrue(segments.All(s => s.Start.Y == y && s.End.Y == y));
        Assert.AreEqual(graph.MaxTimeOffset().X, segments[0].End.X, "First part ends at the right edge (24:00).");
        Assert.AreEqual(graph.TimeOffset(TimeSpan.Zero).X, segments[1].Start.X, "Second part starts at the left edge (00:00).");
    }
}
