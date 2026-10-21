using Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers a station whose dispatch list is divided by direction: which list each clearance lands on.
/// </summary>
[TestClass]
public class DispatchDirectionTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    private const string West = "Munkeröd";
    private const string Middle = "Slokärr";
    private const string East = "Stilkøbing";

    // Three manned stations in a row with one train over the whole of it, running west to east.
    private static (Timetable Timetable, Station Middle) CreateTimetable(int dwellMinutes, bool split)
    {
        var layout = new Layout { Name = "Test" };
        var west = layout.Add(NewStation(1, West, "Mkd"));
        var middle = (Station)layout.Add(NewStation(2, Middle, "Slk"));
        var east = layout.Add(NewStation(3, East, "Stk"));
        layout.Add(new TrackStretch(1, west, middle, 10));
        layout.Add(new TrackStretch(2, middle, east, 10));
        middle.SplitDispatchByDirection = split;

        var timetable = new Timetable("Test", layout);
        var train = new Train(1, 1234);
        var start = Time.FromHourAndMinute(12, 00);
        train.Add(new StationCall(10, west["1"], start.AddMinutes(-60), start));
        var arrival = start.AddMinutes(10);
        var departure = arrival.AddMinutes(dwellMinutes);
        var middleCall = train.Add(new StationCall(11, middle["1"], arrival, departure));
        var end = departure.AddMinutes(20);
        var lastCall = train.Add(new StationCall(12, east["1"], end, end.AddMinutes(20)));
        middleCall.IsArrival = dwellMinutes > 0;
        middleCall.IsDeparture = dwellMinutes > 0;
        lastCall.IsArrival = true;
        timetable.Add(train);
        return (timetable, middle);
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    [TestMethod]
    public void AStationNotDividedHasOneList()
    {
        var (timetable, middle) = CreateTimetable(3, split: false);

        var lists = DispatchList.CreateAll(middle, timetable.Trains, Settings);

        Assert.HasCount(1, lists);
        Assert.IsNull(lists[0].Direction);
    }

    [TestMethod]
    public void ADividedStationHasOneListPerDirection()
    {
        var (timetable, middle) = CreateTimetable(3, split: true);

        var lists = DispatchList.CreateAll(middle, timetable.Trains, Settings);

        Assert.HasCount(2, lists);
        Assert.AreEqual(West, lists[0].Direction);
        Assert.AreEqual(East, lists[1].Direction);
    }

    [TestMethod]
    public void AStandingTrainIsClearedInFromWhereItCameAndOutToWhereItGoes()
    {
        var (timetable, middle) = CreateTimetable(3, split: true);

        var lists = DispatchList.CreateAll(middle, timetable.Trains, Settings);

        Assert.AreEqual(DispatchRowKind.Arrival, lists[0].Rows.Single().Kind);
        Assert.AreEqual(DispatchRowKind.Departure, lists[1].Rows.Single().Kind);
    }

    [TestMethod]
    public void ATrainRunningThroughIsArrivalOnOneListAndDepartureOnTheOther()
    {
        var (timetable, middle) = CreateTimetable(0, split: true);

        var lists = DispatchList.CreateAll(middle, timetable.Trains, Settings);

        var arrival = lists[0].Rows.Single();
        var departure = lists[1].Rows.Single();
        Assert.AreEqual(DispatchRowKind.Arrival, arrival.Kind);
        Assert.AreEqual(DispatchRowKind.Departure, departure.Kind);
        Assert.AreEqual(arrival.Time, departure.Time);
        Assert.IsFalse(arrival.IsSoleRow);
        Assert.IsFalse(departure.IsSoleRow);
    }

    [TestMethod]
    public void AStationAtTheEndOfTheLineHasOneListHoweverItIsSet()
    {
        var (timetable, _) = CreateTimetable(3, split: true);
        var west = (Station)timetable.Layout.OperationLocations.Single(location => location.Name == West);
        west.SplitDispatchByDirection = true;

        var lists = DispatchList.CreateAll(west, timetable.Trains, Settings);

        Assert.HasCount(1, lists);
    }
}
