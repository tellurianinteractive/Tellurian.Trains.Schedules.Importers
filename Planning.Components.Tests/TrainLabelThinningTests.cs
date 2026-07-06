using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies that <see cref="GraphScheduleDrawingExtensions.VisibleTrainLabels"/> thins overlapping train
/// identity labels: labels that do not overlap are all kept, and coincident labels from several trains are
/// reduced until the overlaps clear while every train keeps at least one label.
/// </summary>
[TestClass]
public class TrainLabelThinningTests
{
    // A straight line of single-track stations 10 km apart, rendered in horizontal mode.
    private static (TimetableStretch Line, Timetable Timetable) CreateLine(int stationCount)
    {
        var layout = new Layout { Name = "Test" };
        var stations = new Station[stationCount];
        for (var i = 0; i < stationCount; i++)
        {
            var station = new Station(i + 1, $"S{i + 1}", $"S{i + 1}");
            station.Add(new StationTrack((i + 1) * 10 + 1, "1"));
            layout.Add(station);
            stations[i] = station;
        }
        var line = new TimetableStretch(1, "1");
        for (var i = 1; i < stationCount; i++)
        {
            var stretch = new TrackStretch(i, stations[i - 1], stations[i], 10, 1);
            layout.Add(stretch);
            line.AddLast(stretch);
        }
        layout.Add(line);
        return (line, new Timetable("Test", layout));
    }

    private static Train AddTrain(Timetable timetable, TimetableStretch line, int number, params Time[] times)
    {
        var category = new TrainCategory { Id = 1, Prefix = "G", Name = "G" };
        var train = new Train(number, category, number) { Category = category };
        var stations = line.Stations.ToArray();
        for (var i = 0; i < times.Length; i++)
            _ = train.Add(new StationCall(number * 100 + i, stations[i]["1"], times[i], times[i]));
        timetable.Add(train);
        return train;
    }

    [TestMethod]
    public void KeepsEveryLabelWhenNoneOverlap()
    {
        var (line, timetable) = CreateLine(3);
        // One train, two sections placed hours apart, so its two labels sit far enough apart not to overlap.
        AddTrain(timetable, line, 1, Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(12, 0), Time.FromHourAndMinute(16, 0));
        var graph = new GraphSchedule(line, timetable);

        Assert.AreEqual(graph.TrainLabels().Count(), graph.VisibleTrainLabels().Count());
    }

    // Adds a train that runs the line in reverse station order (last station first), so in vertical mode its
    // segments point leftward (decreasing track-axis X) and TrainLabelPath flips them.
    private static Train AddReverseTrain(Timetable timetable, TimetableStretch line, int number, params Time[] times)
    {
        var category = new TrainCategory { Id = 1, Prefix = "G", Name = "G" };
        var train = new Train(number, category, number) { Category = category };
        var stations = line.Stations.Reverse().ToArray();
        for (var i = 0; i < times.Length; i++)
            _ = train.Add(new StationCall(number * 100 + i, stations[i]["1"], times[i], times[i]));
        timetable.Add(train);
        return train;
    }

    // Adds a train calling only the given stations (by index), so meeting/branching scenarios can be built.
    private static Train AddTrainAtStations(Timetable timetable, TimetableStretch line, int number, params (int StationIndex, Time Time)[] calls)
    {
        var category = new TrainCategory { Id = 1, Prefix = "G", Name = "G" };
        var train = new Train(number, category, number) { Category = category };
        var stations = line.Stations.ToArray();
        for (var i = 0; i < calls.Length; i++)
            _ = train.Add(new StationCall(number * 100 + i, stations[calls[i].StationIndex]["1"], calls[i].Time, calls[i].Time));
        timetable.Add(train);
        return train;
    }

    [TestMethod]
    public void EndpointLabelYieldsToAnotherTrainsSoleSurvivor()
    {
        var (line, timetable) = CreateLine(4);
        var t = (int h, int m) => Time.FromHourAndMinute(h, m);
        // Long train with three section labels (S1..S4).
        var through = AddTrain(timetable, line, 1, t(8, 0), t(8, 20), t(8, 40), t(9, 0));
        // A one-section train coincident with the long train's interior (S2–S3): the long train drops that
        // interior label the ordinary way, spending its fair share of removals.
        var interior = AddTrainAtStations(timetable, line, 2, (1, t(8, 20)), (2, t(8, 40)));
        // A one-section train coincident with the long train's last (endpoint) label (S3–S4). The ordinary
        // passes can't clear this — the endpoint is protected and the round-robin gate blocks a second removal —
        // so the last-resort pass must drop the endpoint to free this sole survivor.
        var meeting = AddTrainAtStations(timetable, line, 3, (2, t(8, 40)), (3, t(9, 0)));

        var graph = new GraphSchedule(line, timetable);
        var visible = graph.VisibleTrainLabels().ToList();

        Assert.IsTrue(visible.Any(v => v.Train == interior), "The interior sole-survivor keeps its label.");
        Assert.IsTrue(visible.Any(v => v.Train == meeting), "The meeting sole-survivor keeps its label.");
        Assert.AreEqual(1, visible.Count(v => v.Train == through),
            "The long train yields both its conflicting labels (interior + endpoint), keeping only the clear one.");
    }

    [TestMethod]
    public void VerticalFlippedLabelsArePlacedUprightForBothRenderAndThinning()
    {
        var (line, timetable) = CreateLine(3);
        // A reverse-direction train: in vertical mode every one of its segments points leftward and is flipped.
        AddReverseTrain(timetable, line, 7, Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 20), Time.FromHourAndMinute(8, 40));
        var graph = new GraphSchedule(line, timetable);
        graph.GraphSettings.AxisDirection = TimeAxisDirection.Vertical;

        // The renderer and the thinning both read these exact tuples, so proving the flip is applied here proves
        // the removal algorithm positions the flipped labels exactly where they are drawn.
        var labels = graph.TrainLabels().ToList();
        Assert.AreEqual(2, labels.Count, "A three-station reverse train produces two section labels.");
        foreach (var label in labels)
            Assert.IsTrue(label.End.X >= label.Start.X,
                $"Flipped label segment must point rightward (upright); got Start.X={label.Start.X}, End.X={label.End.X}.");
    }

    [TestMethod]
    public void ThinsCoincidentLabelsToOnePerSectionKeepingEachTrain()
    {
        var (line, timetable) = CreateLine(3);
        var times = new[] { Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(12, 0), Time.FromHourAndMinute(16, 0) };
        // Two trains with identical times draw their labels on top of each other on both sections.
        var first = AddTrain(timetable, line, 1, times);
        var second = AddTrain(timetable, line, 2, times);
        var graph = new GraphSchedule(line, timetable);

        Assert.AreEqual(4, graph.TrainLabels().Count(), "Two trains over two sections produce four raw labels.");

        var visible = graph.VisibleTrainLabels().ToList();
        Assert.AreEqual(2, visible.Count, "Each section keeps one of its two coincident labels.");
        Assert.IsTrue(visible.Any(v => v.Train == first), "The first train keeps at least one label.");
        Assert.IsTrue(visible.Any(v => v.Train == second), "The second train keeps at least one label.");
    }
}
