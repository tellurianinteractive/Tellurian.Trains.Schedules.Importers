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
