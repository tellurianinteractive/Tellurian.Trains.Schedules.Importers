using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies that a configured break time limits the time axis to the first half (start–break) or the
/// last half (break–end) when that half is selected, and that the break is ignored when it is unset
/// or falls outside the operating window. See <see cref="GraphHalf"/> and <see cref="GraphSchedule"/>.
/// </summary>
[TestClass]
public class GraphHalfTests
{
    private static readonly TimeSpan Start = TimeSpan.FromHours(6);
    private static readonly TimeSpan End = TimeSpan.FromHours(20);
    private static readonly TimeSpan Break = TimeSpan.FromHours(13);

    // Two-station line A —10m— B, single track each, no trains (so the axis keeps the operating window).
    private static GraphSchedule CreateGraph(GraphHalf half, TimeSpan? breakTime)
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
        var settings = GraphSettings.Default with
        {
            DefaultStartTime = Start,
            DefaultEndTime = End,
            BreakTime = breakTime,
        };
        return new GraphSchedule(line, timetable, settings, half);
    }

    [TestMethod]
    public void WholeShowsTheFullWindowEvenWithABreak()
    {
        var graph = CreateGraph(GraphHalf.Whole, Break);
        Assert.IsTrue(graph.HasBreak);
        Assert.AreEqual(Start, graph.StartTime);
        Assert.AreEqual(End, graph.EndTime);
    }

    [TestMethod]
    public void FirstHalfRunsFromStartToBreak()
    {
        var graph = CreateGraph(GraphHalf.First, Break);
        Assert.AreEqual(Start, graph.StartTime);
        Assert.AreEqual(Break, graph.EndTime);
    }

    [TestMethod]
    public void LastHalfRunsFromBreakToEnd()
    {
        var graph = CreateGraph(GraphHalf.Last, Break);
        Assert.AreEqual(Break, graph.StartTime);
        Assert.AreEqual(End, graph.EndTime);
    }

    [TestMethod]
    public void HalfIsIgnoredWhenNoBreakIsSet()
    {
        var graph = CreateGraph(GraphHalf.First, breakTime: null);
        Assert.IsFalse(graph.HasBreak);
        Assert.AreEqual(Start, graph.StartTime);
        Assert.AreEqual(End, graph.EndTime);
    }

    [TestMethod]
    public void BreakOutsideTheWindowIsIgnored()
    {
        var graph = CreateGraph(GraphHalf.First, TimeSpan.FromHours(22)); // after the 20:00 end
        Assert.IsFalse(graph.HasBreak);
        Assert.AreEqual(Start, graph.StartTime);
        Assert.AreEqual(End, graph.EndTime);
    }
}
