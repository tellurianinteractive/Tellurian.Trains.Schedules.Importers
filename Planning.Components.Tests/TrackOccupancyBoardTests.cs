using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the track occupancy board: calls on their tracks, trains stacked when they share a track at the
/// same time, and blocks over midnight wrapping to the start of the axis.
/// </summary>
[TestClass]
public class TrackOccupancyBoardTests
{
    private static readonly TrainCategory Category = new() { Id = 1, Prefix = "G", Name = "G" };

    private Station _station = default!;
    private Timetable _timetable = default!;
    private Plan _plan = default!;
    private int _nextTrain;

    [TestInitialize]
    public void TestInitialize()
    {
        var layout = new Layout { Name = "Test" };
        _station = new Station(1, "Alpha", "A");
        _station.Add(new StationTrack(11, "1"));
        _station.Add(new StationTrack(12, "2"));
        layout.Add(_station);
        _timetable = new Timetable("Test", layout);
        _plan = Plan.Create("Test", _timetable);
        _nextTrain = 0;
    }

    private void AddTrain(string track, Time arrival, Time departure)
    {
        var train = new Train(++_nextTrain, Category, _nextTrain) { Category = Category };
        _ = train.Add(new StationCall(1, _station[track], arrival, departure));
        _timetable.Add(train);
    }

    private TrackOccupancyBoard Board() => TrackOccupancyBoard.Create(_plan, GraphSettings.Default, []);

    private static Time At(int hour, int minute) => Time.FromHourAndMinute(hour, minute);

    private static TrackOccupancyRow RowOf(TrackOccupancyBoard board, string number) =>
        board.Stations.Single().Rows.Single(r => r.Track.Number == number);

    [TestMethod]
    public void ACallIsABlockOnItsTrack()
    {
        AddTrain("1", At(10, 0), At(10, 10));

        var board = Board();

        Assert.HasCount(1, RowOf(board, "1").Blocks);
        Assert.HasCount(0, RowOf(board, "2").Blocks);
    }

    [TestMethod]
    public void TrainsOnTheSameTrackAtTheSameTimeAreStacked()
    {
        AddTrain("1", At(10, 0), At(10, 30));
        AddTrain("1", At(10, 20), At(10, 50));

        var row = RowOf(Board(), "1");

        Assert.AreEqual(2, row.Lanes);
        Assert.AreEqual(2, row.Blocks.Select(b => b.Lane).Distinct().Count(), "The two trains must not share a line.");
    }

    [TestMethod]
    public void ATrainArrivingAsAnotherLeavesSharesItsLine()
    {
        AddTrain("1", At(10, 0), At(10, 30));
        AddTrain("1", At(10, 30), At(11, 0));

        Assert.AreEqual(1, RowOf(Board(), "1").Lanes);
    }

    [TestMethod]
    public void TrainsOnDifferentTracksAreNotStacked()
    {
        AddTrain("1", At(10, 0), At(10, 30));
        AddTrain("2", At(10, 0), At(10, 30));

        var board = Board();

        Assert.AreEqual(1, RowOf(board, "1").Lanes);
        Assert.AreEqual(1, RowOf(board, "2").Lanes);
    }

    [TestMethod]
    public void MovingACallMovesItsBlockToTheOtherTrack()
    {
        AddTrain("1", At(10, 0), At(10, 30));
        var call = RowOf(Board(), "1").Blocks.Single().Call!;

        Assert.IsTrue(call.MoveTo(_station["2"]));
        var board = Board();

        Assert.HasCount(0, RowOf(board, "1").Blocks);
        Assert.HasCount(1, RowOf(board, "2").Blocks);
    }

    [TestMethod]
    public void ACallOverMidnightWrapsToTheStartOfTheAxis()
    {
        AddTrain("1", At(23, 50), Time.FromDayHourMinute(1, 0, 10));

        var block = RowOf(Board(), "1").Blocks.Single();

        Assert.HasCount(2, block.Segments);
        Assert.AreEqual((TimeSpan.FromHours(23) + TimeSpan.FromMinutes(50), TimeSpan.FromHours(24)), block.Segments[0]);
        Assert.AreEqual((TimeSpan.Zero, TimeSpan.FromMinutes(10)), block.Segments[1]);
    }

    [TestMethod]
    public void APassThroughStillHasAVisibleLength()
    {
        AddTrain("1", At(10, 0), At(10, 0));

        var segment = RowOf(Board(), "1").Blocks.Single().Segments.Single();

        Assert.IsGreaterThan(TimeSpan.Zero, segment.End - segment.Start);
    }
}
