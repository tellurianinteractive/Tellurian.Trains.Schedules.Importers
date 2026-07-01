using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Planning.Components.Reporting;
using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public sealed class TimetableTableTests
{
    private const string JsonFilePath = @"C:\Users\Stefan\Downloads\Givskud-Modern-2025.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    private Plan _plan = default!;

    [TestInitialize]
    public void Setup()
    {
        if (!File.Exists(JsonFilePath))
            Assert.Inconclusive($"Test data file not found: {JsonFilePath}");
        _plan = JsonSerializer.Deserialize<Plan>(File.ReadAllText(JsonFilePath), JsonOptions)!;
    }

    // A train with a registered call at a SignalControlledLocation must show |
    // in that station's row — never an empty cell. This covers:
    //   - Junction SCLs that are first/last on a stretch (km=0 or km=max)
    //   - Terminal trains whose last stretch call is at the SCL
    [TestMethod]
    public void SignalControlledLocation_ShowsPipe_WhenTrainCallsThereAsTerminal()
    {
        var trains = _plan.Timetable.Trains
            .ToDictionary(t => t.Identity, t => t);

        var failures = new List<string>();

        foreach (var stretch in _plan.Timetable.Layout.TimetableStretches)
        {
            var stretchStationList = stretch.Stations.ToList();

            foreach (var direction in new[] { TrainGraphDirection.Upward, TrainGraphDirection.Downward })
            {
                var table = TimetableTable.Create(stretch, _plan.Timetable.Trains, direction);
                var tableStations = direction == TrainGraphDirection.Upward
                    ? stretchStationList
                    : [.. stretchStationList.AsEnumerable().Reverse()];

                // The ordinary stretch rows align one-to-one with the stretch stations; connection
                // rows borrowed from neighbouring stretches are excluded so the indices stay in step.
                var stationRows = table.Rows.Where(r => r.Kind == TimetableRowKind.Normal).ToList();

                for (var rowIdx = 0; rowIdx < stationRows.Count; rowIdx++)
                {
                    if (tableStations[rowIdx] is not SignalControlledLocation sclStation) continue;

                    for (var colIdx = 0; colIdx < table.Columns.Count; colIdx++)
                    {
                        if (!trains.TryGetValue(table.Columns[colIdx].Header, out var train)) continue;

                        var hasCallAtScl = train.Calls.Any(c => c.OperationLocation.Equals(sclStation));
                        if (!hasCallAtScl) continue;

                        var cell = stationRows[rowIdx].Cells[colIdx];
                        if (!cell.IsNotStopping)
                            failures.Add(
                                $"Stretch {stretch.Number} {direction}: train {train.Identity} calls at SCL " +
                                $"'{sclStation.Name}' (row {rowIdx}) but cell is empty");
                    }
                }
            }
        }

        if (failures.Count > 0)
            Assert.Fail(string.Join("\n", failures));
    }

    // Stretch 1 (Ingerslev–…–Munkeröd) trains branch at Friedensweg onto stretch 2 and terminate at
    // Padby or Stilkøbing. Upward those are terminal connections appended below the stretch's own rows.
    [TestMethod]
    public void Stretch1_Upward_AppendsTerminalConnectionsBelow()
    {
        var stretch = _plan.Timetable.Layout.TimetableStretches.Single(s => s.Number == "1");
        var table = TimetableTable.Create(stretch, _plan.Timetable.Trains, TrainGraphDirection.Upward);

        Assert.IsFalse(table.Rows.Any(r => r.Kind == TimetableRowKind.OriginConnection),
            "Upward stretch 1 should have no origin connections (all trains originate on the stretch).");

        var connections = table.Rows.Where(r => r.Kind == TimetableRowKind.TerminalConnection).ToList();
        CollectionAssert.AreEqual(new[] { "Padby", "Stilkøbing" }, connections.Select(r => r.StationName).ToArray());

        // Connection rows come after every ordinary stretch row.
        var lastNormal = table.Rows.ToList().FindLastIndex(r => r.Kind == TimetableRowKind.Normal);
        var firstTerminal = table.Rows.ToList().FindIndex(r => r.Kind == TimetableRowKind.TerminalConnection);
        Assert.IsTrue(firstTerminal > lastNormal, "Terminal connections must sit below the ordinary rows.");

        // Terminal connection rows show only arrivals — never a departure (no "From Padby" line below).
        Assert.IsTrue(connections.SelectMany(r => r.Cells).All(c => c.Departure is null));

        // EN 482 terminates at Stilkøbing but stops at Padby on the way, so its Padby arrival shows in the
        // Padby row as well as its Stilkøbing arrival.
        var en482 = table.Columns.ToList().FindIndex(c => c.Header == "EN 482");
        Assert.IsTrue(en482 >= 0, "EN 482 should be a column in upward stretch 1.");
        var padby = connections.Single(r => r.StationName == "Padby");
        var stilkobing = connections.Single(r => r.StationName == "Stilkøbing");
        Assert.IsTrue(padby.Cells[en482].Arrival is not null, "Through-train EN 482 should show its Padby arrival.");
        Assert.IsTrue(stilkobing.Cells[en482].Arrival is not null, "EN 482 should show its Stilkøbing arrival.");
    }

    // The opposite direction: trains originate at Stilkøbing/Padby, shown as departure connections
    // above the stretch's own rows, furthest origin first.
    [TestMethod]
    public void Stretch1_Downward_PrependsOriginConnectionsAbove()
    {
        var stretch = _plan.Timetable.Layout.TimetableStretches.Single(s => s.Number == "1");
        var table = TimetableTable.Create(stretch, _plan.Timetable.Trains, TrainGraphDirection.Downward);

        Assert.IsFalse(table.Rows.Any(r => r.Kind == TimetableRowKind.TerminalConnection),
            "Downward stretch 1 should have no terminal connections (all trains terminate on the stretch).");

        var connections = table.Rows.Where(r => r.Kind == TimetableRowKind.OriginConnection).ToList();
        CollectionAssert.AreEqual(new[] { "Stilkøbing", "Padby" }, connections.Select(r => r.StationName).ToArray());

        var firstNormal = table.Rows.ToList().FindIndex(r => r.Kind == TimetableRowKind.Normal);
        var lastOrigin = table.Rows.ToList().FindLastIndex(r => r.Kind == TimetableRowKind.OriginConnection);
        Assert.IsTrue(lastOrigin < firstNormal, "Origin connections must sit above the ordinary rows.");

        // Origin connection rows show only departures — never an arrival (no "To Padby" line above).
        Assert.IsTrue(connections.SelectMany(r => r.Cells).All(c => c.Arrival is null));

        // EC 371 originates at Stilkøbing but stops at Padby on the way, so its Padby departure shows in
        // the Padby row as well as its Stilkøbing departure.
        var ec371 = table.Columns.ToList().FindIndex(c => c.Header == "EC 371");
        Assert.IsTrue(ec371 >= 0, "EC 371 should be a column in downward stretch 1.");
        var padby = connections.Single(r => r.StationName == "Padby");
        var stilkobing = connections.Single(r => r.StationName == "Stilkøbing");
        Assert.IsTrue(padby.Cells[ec371].Departure is not null, "Through-train EC 371 should show its Padby departure.");
        Assert.IsTrue(stilkobing.Cells[ec371].Departure is not null, "EC 371 should show its Stilkøbing departure.");
    }

    // Stretch 2 connects to stretch 1 (Ingerslev, Rotebro) and stretch 3 (Grenåbro). Connection rows are
    // grouped by the table number they come from: origins above run table 1 then table 3 (Ingerslev,
    // Rotebro, Grenåbro); terminals below are the exact reverse (Grenåbro, Rotebro, Ingerslev).
    [TestMethod]
    public void Stretch2_GroupsConnectionsByTableNumber()
    {
        var stretch = _plan.Timetable.Layout.TimetableStretches.Single(s => s.Number == "2");

        var upward = TimetableTable.Create(stretch, _plan.Timetable.Trains, TrainGraphDirection.Upward);
        var origins = upward.Rows.Where(r => r.Kind == TimetableRowKind.OriginConnection).Select(r => r.StationName).ToArray();
        CollectionAssert.AreEqual(new[] { "Ingerslev", "Rotebro", "Grenåbro" }, origins);

        var downward = TimetableTable.Create(stretch, _plan.Timetable.Trains, TrainGraphDirection.Downward);
        var terminals = downward.Rows.Where(r => r.Kind == TimetableRowKind.TerminalConnection).Select(r => r.StationName).ToArray();
        CollectionAssert.AreEqual(new[] { "Grenåbro", "Rotebro", "Ingerslev" }, terminals);
    }

    // A train that continues to/from a connection must read as a through-stop at the boundary station,
    // showing both arrival and departure — not just the one side it showed before connections existed.
    [TestMethod]
    public void Stretch2_BoundaryStation_ShowsBothTimesForThroughTrains()
    {
        var stretch = _plan.Timetable.Layout.TimetableStretches.Single(s => s.Number == "2");

        // L 710 comes from connection Grenåbro and enters stretch 2 at Munckerup, so Munckerup shows its
        // arrival as well as its departure.
        var upward = TimetableTable.Create(stretch, _plan.Timetable.Trains, TrainGraphDirection.Upward);
        AssertBoth(upward, station: "Munckerup", train: "L 710");

        // L 729 leaves stretch 2 at Munckerup toward connection Grenåbro, so Munckerup shows its departure
        // as well as its arrival.
        var downward = TimetableTable.Create(stretch, _plan.Timetable.Trains, TrainGraphDirection.Downward);
        AssertBoth(downward, station: "Munckerup", train: "L 729");

        static void AssertBoth(TimetableTable table, string station, string train)
        {
            var col = table.Columns.ToList().FindIndex(c => c.Header == train);
            Assert.IsTrue(col >= 0, $"{train} should be a column.");
            var row = table.Rows.Single(r => r.Kind == TimetableRowKind.Normal && r.StationName == station);
            Assert.IsTrue(row.Cells[col].HasArrivalAndDeparture,
                $"{train} at boundary {station} should show both arrival and departure.");
        }
    }
}
