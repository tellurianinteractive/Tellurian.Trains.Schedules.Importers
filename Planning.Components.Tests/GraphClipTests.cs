using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies that train lines are clipped to the visible time window rather than collapsing to the axis
/// origin. An out-of-window time gives <see cref="Offset.Invalid"/> (0,0) from TimeOffset, which would
/// otherwise draw a stray line to the left of the stations. See the graphical schedule clipping fix.
/// </summary>
[TestClass]
public class GraphClipTests
{
    // Two-station line A —10km— B, single track each, with an explicit time window and no trains in the
    // timetable (the line is exercised directly, as in the wrapping tests).
    private static GraphSchedule CreateGraph(TimeSpan windowStart, TimeSpan windowEnd)
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
        var settings = GraphSettings.Default with { DefaultStartTime = windowStart, DefaultEndTime = windowEnd };
        return new GraphSchedule(line, timetable, settings);
    }

    [TestMethod]
    public void SegmentReachingPastTheWindowEndIsClippedToTheEdge()
    {
        var graph = CreateGraph(TimeSpan.FromHours(6), TimeSpan.FromHours(13));
        // Train runs 12:00 -> 14:00; only 12:00 -> 13:00 is inside the window.
        var segments = graph.TrainBetweenStationsLine(0, 0, 1, 0, Time.FromHourAndMinute(12, 0), Time.FromHourAndMinute(14, 0)).ToList();

        Assert.AreEqual(1, segments.Count);
        Assert.AreEqual(graph.TimeOffset(TimeSpan.FromHours(12)).X, segments[0].Start.X, "Starts at 12:00.");
        Assert.AreEqual(graph.MaxTimeOffset().X, segments[0].End.X, "Ends clipped at the window end (13:00), not at 0.");
        Assert.AreNotEqual(0, segments[0].End.X, "The clipped end must not collapse to the axis origin.");

        // 12:00 -> 14:00 is two hours and the window ends one hour in, so the cross-axis position at the
        // clip is interpolated halfway between station A and station B.
        var yA = graph.Y(0, 0);
        var yB = graph.Y(1, 0);
        Assert.AreEqual((yA + yB) / 2, segments[0].End.Y);
    }

    [TestMethod]
    public void SegmentWhollyOutsideTheWindowIsDropped()
    {
        var graph = CreateGraph(TimeSpan.FromHours(6), TimeSpan.FromHours(13));
        var segments = graph.TrainBetweenStationsLine(0, 0, 1, 0, Time.FromHourAndMinute(14, 0), Time.FromHourAndMinute(15, 0)).ToList();
        Assert.AreEqual(0, segments.Count, "A line entirely after the window end draws nothing.");
    }

    [TestMethod]
    public void SegmentFullyInsideTheWindowIsUnchanged()
    {
        var graph = CreateGraph(TimeSpan.FromHours(6), TimeSpan.FromHours(20));
        var segments = graph.TrainBetweenStationsLine(0, 0, 1, 0, Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 30)).ToList();
        Assert.AreEqual(1, segments.Count);
        Assert.AreEqual(graph.TimeOffset(TimeSpan.FromHours(8)).X, segments[0].Start.X);
        Assert.AreEqual(graph.TimeOffset(Time.FromHourAndMinute(8, 30).Value).X, segments[0].End.X);
        Assert.AreEqual(graph.Y(0, 0), segments[0].Start.Y);
        Assert.AreEqual(graph.Y(1, 0), segments[0].End.Y);
    }
}
