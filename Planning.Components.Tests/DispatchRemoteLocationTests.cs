using Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// A dispatcher who works a junction from afar clears its trains too, so they are in the same list:
/// Alpha (manned) works the junction Bj, where the lines from Alpha and Delta join towards Charlie.
/// </summary>
[TestClass]
public class DispatchRemoteLocationTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    private static (Timetable timetable, Station a, Station c) CreateJunction()
    {
        var layout = new Layout { Name = "Test" };
        var a = (Station)layout.Add(new Station(1, "Alpha", "A") { IsManned = true, PhoneNumber = 11 });
        var b = layout.Add(new SignalControlledLocation(2, "Bravo junction", "Bj"));
        var c = (Station)layout.Add(new Station(3, "Charlie", "C") { IsManned = true, PhoneNumber = 13 });
        var d = (Station)layout.Add(new Station(4, "Delta", "D") { IsManned = true, PhoneNumber = 14 });
        foreach (var location in new[] { a, b, c, d }) location.Add(new StationTrack(location.Id * 10, "1"));
        b.ControlledBy = a;
        layout.Add(new TrackStretch(1, a, b, 10));
        layout.Add(new TrackStretch(2, b, c, 10));
        layout.Add(new TrackStretch(3, d, b, 10));
        layout.DispatchStretches = layout.CreateDispatchStretches();

        // Delta to Charlie over the junction, never touching Alpha; and Alpha's own train to Charlie.
        var timetable = new Timetable("Test", layout);
        timetable.Add(Run(1, 101, Time.FromHourAndMinute(12, 00), d, b, c));
        timetable.Add(Run(2, 202, Time.FromHourAndMinute(12, 30), a, b, c));
        return (timetable, a, c);
    }

    private static Train Run(int id, int number, Time start, params OperationLocation[] locations)
    {
        var train = new Train(id, number);
        for (var i = 0; i < locations.Length; i++)
        {
            var time = start.AddMinutes(10 * i);
            train.Add(new StationCall(id * 10 + i, locations[i]["1"], time, time));
        }
        return train;
    }

    [TestMethod]
    public void TheControllersListHasTheJunctionsTrainsMarkedWithItsSignature()
    {
        var (timetable, a, _) = CreateJunction();

        var list = DispatchList.Create(a, timetable.Trains, Settings);
        var remote = list.Rows.Where(row => row.IsRemote).ToList();

        Assert.IsNotEmpty(remote);
        Assert.IsTrue(remote.All(row => row.TrackText == "Bj 1"));
        Assert.IsTrue(remote.All(row => row.TrainIdentity.EndsWith("101", StringComparison.Ordinal)), "The Delta-Charlie train never calls at Alpha, but Alpha sets its route.");
        Assert.AreEqual("Alpha + Bravo junction", list.Title);
    }

    [TestMethod]
    public void AJunctionWithoutRowsIsLeftOutOfTheHeading()
    {
        var (timetable, a, _) = CreateJunction();
        var ownTrainsOnly = timetable.Trains.Where(train => train.Number == 202);

        var list = DispatchList.Create(a, ownTrainsOnly, Settings);

        Assert.IsFalse(list.Rows.Any(row => row.IsRemote));
        Assert.IsEmpty(list.RemoteLocations);
        Assert.AreEqual("Alpha", list.Title);
    }

    [TestMethod]
    public void ATrainCallingAtTheStationHasNoRowsAtTheJunction()
    {
        var (timetable, a, _) = CreateJunction();

        var rows = DispatchList.Create(a, timetable.Trains, Settings).Rows
            .Where(row => row.TrainIdentity.EndsWith("202", StringComparison.Ordinal)).ToList();

        Assert.IsNotEmpty(rows, "Alpha's own train departs from Alpha.");
        Assert.IsFalse(rows.Any(row => row.IsRemote), "It is already cleared at Alpha, so its junction rows would only repeat it.");
    }

    [TestMethod]
    public void TheJunctionsRowsFallInTimeOrderAmongTheStationsOwn()
    {
        var (timetable, a, _) = CreateJunction();

        var times = DispatchList.Create(a, timetable.Trains, Settings).Rows.Select(row => row.Time).ToList();

        Assert.AreSequenceEqual(times.Order().ToArray(), times.ToArray());
    }

    [TestMethod]
    public void TheControllerRingsTheDispatchersBeyondTheJunction()
    {
        var (timetable, a, c) = CreateJunction();

        Assert.AreSequenceEqual(new[] { "Charlie", "Delta" }, DispatchList.Create(a, timetable.Trains, Settings).Neighbours.Select(n => n.Name).ToArray());
        Assert.AreSequenceEqual(new[] { "Alpha" }, DispatchList.Create(c, timetable.Trains, Settings).Neighbours.Select(n => n.Name).ToArray());
    }

    [TestMethod]
    public void AnotherStationsListHasNoRowsAtTheJunction()
    {
        var (timetable, _, c) = CreateJunction();

        var list = DispatchList.Create(c, timetable.Trains, Settings);

        Assert.IsFalse(list.Rows.Any(row => row.IsRemote));
        Assert.AreEqual("Charlie", list.Title);
    }
}
