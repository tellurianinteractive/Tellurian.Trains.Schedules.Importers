using Microsoft.Extensions.Logging.Abstractions;
using Tellurian.Trains.Schedules.Importers.Services;
using Tellurian.Trains.Schedules.Importers.Xpln;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class TrainSortingTests
{
    // Import the stable, checked-in XPLN file rather than a volatile JSON export; the file is copied
    // into the test output from Importers.Xpln.Tests via a linked item in the project file.
    private const string ScheduleName = "Givskud-Modern-2025";
    private static readonly string OdsFilePath =
        Path.Combine("Test data", $"{ScheduleName}.da-DK.ods");

    private Plan _plan = default!;

    [TestInitialize]
    public async Task Setup()
    {
        var file = new FileInfo(OdsFilePath);
        if (!file.Exists)
            Assert.Inconclusive($"Test data file not found: {file.FullName}");

        var provider = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
        using var importer = new XplnDataImporter(
            file, provider, new CompaniesFromJsonService(), new TrainCategoriesFromCsvService(),
            NullLogger<XplnDataImporter>.Instance);
        var result = await importer.ImportScheduleAsync(ScheduleName);
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");
        _plan = result.Item;
    }

    // Expected first 10 segments on stretch 2 in graphical left-to-right column order, per direction.
    // A train may appear twice when it is split around an overtake (for example 44780 upward). Trains that
    // start mid-stretch (for example 9906/912 at Padby) still belong to the column for their direction.
    private static readonly int[] ExpectedStretch2Upward = [9906, 710, 44780, 44780, 75510, 912, 49214, 530, 7520, 7214];
    private static readonly int[] ExpectedStretch2Downward = [42717, 75513, 41915, 41915, 371, 3709, 4411, 36, 925, 75537];

    [TestMethod]
    public void Stretch2_Upward_FirstTrainsAreSortedCorrectly()
    {
        var actual = SortedNumbers("2", TrainGraphDirection.Upward);
        var prefix = actual.Take(ExpectedStretch2Upward.Length).ToArray();
        CollectionAssert.AreEqual(ExpectedStretch2Upward, prefix,
            $"Actual upward order: {string.Join(", ", actual)}");
    }

    [TestMethod]
    public void Stretch2_Downward_FirstTrainsAreSortedCorrectly()
    {
        var actual = SortedNumbers("2", TrainGraphDirection.Downward);
        var prefix = actual.Take(ExpectedStretch2Downward.Length).ToArray();
        CollectionAssert.AreEqual(ExpectedStretch2Downward, prefix,
            $"Actual downward order: {string.Join(", ", actual)}");
    }

    [TestMethod]
    public void Stretch2_Upward_DiagnoseStretchCalls()
    {
        var stretch = _plan.Timetable.Layout.TimetableStretches
            .Single(s => s.Number == "2");
        var totalDist = stretch.Stretches.Sum(s => s.Distance);
        var trains = _plan.Timetable.Trains;

        var interesting = new HashSet<int>([710, 44780, 75510, 49214, 7520, 530, 482, 49222, 75530, 3726]);
        var lines = new List<string>();
        lines.Add($"Stretch 2 stations: {string.Join(" -> ", stretch.Stations.Select(s => s.Name))}  totalDist={totalDist}");
        lines.Add("");

        foreach (var train in trains.Where(t => interesting.Contains(t.Number)))
        {
            var calls = StretchCallsList(stretch, train);
            var sortKey = ExtrapolatedAtZero(calls, totalDist, TrainGraphDirection.Upward);
            var callsStr = string.Join(", ", calls.Select(c => $"d={c.d:F1} t={c.t.HHMM()}"));
            lines.Add($"Train {train.Number,6}: sortKey={sortKey?.HHMM() ?? "none",5}  calls=[{callsStr}]");

            // Also show raw arrival/departure for every call that falls on the stretch
            foreach (var sc in train.Calls)
            {
                var dist = stretch.DistanceToStation(sc.OperationLocation);
                if (dist.HasValue)
                    lines.Add($"  raw: d={dist:F1}  arr={sc.Arrival.HHMM()} dep={sc.Departure.HHMM()}  station={sc.OperationLocation.Name}");
            }
        }

        Assert.Inconclusive(string.Join("\n", lines));
    }

    private static List<(double d, Time t)> StretchCallsList(TimetableStretch stretch, Train train)
    {
        var calls = train.Calls.ToArray();
        var result = new List<(double d, Time t)>();
        for (var i = 0; i < calls.Length; i++)
        {
            var dist = stretch.DistanceToStation(calls[i].OperationLocation);
            if (dist is { } d)
            {
                var t = i < calls.Length - 1 ? calls[i].Departure : calls[i].Arrival;
                result.Add((d, t));
            }
        }
        return result;
    }

    private static Time? ExtrapolatedAtZero(List<(double d, Time t)> calls, double totalDist, TrainGraphDirection direction)
    {
        if (calls.Count == 0) return null;
        if (calls.Count == 1) return calls[0].t;

        var d0 = direction == TrainGraphDirection.Upward ? 0.0 : totalDist;
        var byProximity = direction == TrainGraphDirection.Upward
            ? calls.OrderBy(c => c.d).ToList()
            : calls.OrderByDescending(c => c.d).ToList();

        var (d1, t1) = (byProximity[0].d, byProximity[0].t);
        var (d2, t2) = (byProximity[1].d, byProximity[1].t);
        if (Math.Abs(d2 - d1) < 0.001) return t1;

        var minsPerKm = (t2.TotalMinutes - t1.TotalMinutes) / (d2 - d1);
        var extrapolated = t1.TotalMinutes + (d0 - d1) * minsPerKm;
        return Time.FromTimeSpan(TimeSpan.FromMinutes(extrapolated));
    }

    private int[] SortedNumbers(string stretchNumber, TrainGraphDirection direction)
    {
        var stretch = _plan.Timetable.Layout.TimetableStretches
            .Single(s => s.Number == stretchNumber);
        var segments = stretch.SortedTrainSegments(_plan.Timetable.Trains, direction);
        return [.. segments.Select(s => s.Train.Number)];
    }
}
