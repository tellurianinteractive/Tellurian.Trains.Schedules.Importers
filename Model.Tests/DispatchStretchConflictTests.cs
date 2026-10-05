using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Stretch conflicts judged per dispatch stretch: Alpha (manned) - M - Beta (manned), where M is what
/// each test makes it — an unmanned station nobody controls, a block post, or a controlled station.
/// </summary>
[TestClass]
public class DispatchStretchConflictTests
{
    private static readonly ValidationSettings Settings = new();
    private static TrainCategory Category => new() { Id = 1, Name = "P", Prefix = "P" };

    private sealed record Line(Plan Plan, Station A, OperationLocation M, Station B);

    private static Line NewLine(OperationLocation middle, int middleTracks = 1, int tracksCount = 1, bool withDispatchStretches = true)
    {
        var layout = new Layout { Name = "Test" };
        var a = (Station)layout.Add(new Station(1, "Alpha", "A") { IsManned = true });
        var m = layout.Add(middle);
        var b = (Station)layout.Add(new Station(3, "Beta", "B") { IsManned = true });
        for (var t = 1; t <= 4; t++)
        {
            a.Add(new StationTrack(10 + t, t.ToString()));
            b.Add(new StationTrack(30 + t, t.ToString()));
        }
        for (var t = 1; t <= middleTracks; t++) m.Add(new StationTrack(20 + t, t.ToString()));
        layout.Add(new TrackStretch(1, a, m, 10, tracksCount));
        layout.Add(new TrackStretch(2, m, b, 10, tracksCount));
        if (withDispatchStretches) layout.DispatchStretches = layout.CreateDispatchStretches();
        return new Line(Plan.Create("Test", new Timetable("Test", layout)), a, m, b);
    }

    private static Station Unmanned => new(2, "Middle", "M");
    private static Station ControlledFromAlpha => new(2, "Middle", "M") { ControlledBy = new Station(1, "Alpha", "A") { IsManned = true } };
    private static SignalControlledLocation BlockPost => new(2, "Middle", "M");

    // A train calling at the given locations, with (arrival, departure) in minutes after 12:00. Each train
    // has a track of its own at every location that has enough, so only the line itself can conflict.
    private static Train Run(Line line, int number, params (OperationLocation location, int arrival, int departure)[] calls)
    {
        var train = new Train(number, Category, number) { Category = Category, Sessions = Sessions.All };
        var id = number * 10;
        foreach (var (location, arrival, departure) in calls)
        {
            var track = location.Tracks.OrderBy(t => t.Number).ElementAt((number - 1) % location.Tracks.Count);
            var noon = Time.FromHourAndMinute(12, 00);
            _ = train.Add(new StationCall(++id, track, noon.AddMinutes(arrival), noon.AddMinutes(departure)));
        }
        line.Plan.Timetable.Add(train);
        return train;
    }

    // Two trains in opposite directions that each stand at M 12:10-12:15, so on neither track stretch are
    // they ever both at once: they meet at M.
    private static void MeetAtMiddle(Line line)
    {
        Run(line, 1, (line.A, 0, 0), (line.M, 10, 15), (line.B, 25, 25));
        Run(line, 2, (line.B, 0, 0), (line.M, 10, 15), (line.A, 25, 25));
    }

    // Two trains in the same direction, the second leaving Alpha as the first passes M.
    private static void FollowThroughMiddle(Line line)
    {
        Run(line, 1, (line.A, 0, 0), (line.M, 10, 10), (line.B, 20, 20));
        Run(line, 2, (line.A, 10, 10), (line.M, 20, 20), (line.B, 30, 30));
    }

    private static List<ValidationError> StretchConflicts(Line line) =>
        [.. line.Plan.GetValidationErrors(Settings).Where(e => e.ErrorType == ValidationErrorType.TrackStretchConflict)];

    [TestMethod]
    public void TrainsCannotMeetAtAnUncontrolledUnmannedStation()
    {
        var line = NewLine(Unmanned, middleTracks: 2);
        MeetAtMiddle(line);

        Assert.HasCount(1, StretchConflicts(line), "Nobody can let two trains meet where nobody controls the location.");
    }

    [TestMethod]
    public void TrainsCanMeetAtAControlledStationWithTwoTracks()
    {
        var line = NewLine(ControlledFromAlpha, middleTracks: 2);
        MeetAtMiddle(line);

        Assert.IsEmpty(StretchConflicts(line));
    }

    [TestMethod]
    public void AMeetAtAControlledStationWithOneTrackIsATrackConflict()
    {
        // A controlled station is a dispatch endpoint, so the line on either side is judged apart; the
        // meet fails on its single track instead.
        var line = NewLine(ControlledFromAlpha, middleTracks: 1);
        MeetAtMiddle(line);

        Assert.IsEmpty(StretchConflicts(line));
        Assert.IsNotEmpty(line.Plan.GetValidationErrors(Settings).Where(e => e.ErrorType == ValidationErrorType.StationTrackConflict));
    }

    [TestMethod]
    public void TrainsCannotMeetAtABlockPost()
    {
        var line = NewLine(BlockPost);
        MeetAtMiddle(line);

        Assert.IsNotEmpty(StretchConflicts(line), "A block post divides the line, but only for trains in the same direction.");
    }

    [TestMethod]
    public void TrainsCanFollowEachOtherThroughABlockPost()
    {
        var line = NewLine(BlockPost);
        FollowThroughMiddle(line);

        Assert.IsEmpty(StretchConflicts(line));
    }

    [TestMethod]
    public void TrainsCannotFollowEachOtherPastAnUncontrolledStation()
    {
        var line = NewLine(Unmanned, middleTracks: 2);
        FollowThroughMiddle(line);

        Assert.HasCount(1, StretchConflicts(line), "The whole dispatch stretch is one section: one train at a time.");
    }

    [TestMethod]
    public void TrainWithoutACallAtTheBlockPostOccupiesBothSections()
    {
        var line = NewLine(BlockPost);
        Run(line, 1, (line.A, 0, 0), (line.B, 20, 20));
        Run(line, 2, (line.A, 10, 10), (line.M, 20, 20), (line.B, 30, 30));

        Assert.IsNotEmpty(StretchConflicts(line), "Without a time at the block post, train 1 holds the whole line until it arrives.");
    }

    [TestMethod]
    public void TrainsOnDisjointSessionsNeverMeet()
    {
        var line = NewLine(Unmanned, middleTracks: 2);
        MeetAtMiddle(line);
        line.Plan.Timetable.Trains.First().Sessions = Sessions.FromSessionNumbers(1, 3, 5);
        line.Plan.Timetable.Trains.Last().Sessions = Sessions.FromSessionNumbers(2, 4, 6);

        Assert.IsEmpty(StretchConflicts(line));
    }

    [TestMethod]
    public void OpposingTrainsCanPassOnDoubleTrack()
    {
        var line = NewLine(Unmanned, middleTracks: 2, tracksCount: 2);
        MeetAtMiddle(line);

        Assert.IsEmpty(StretchConflicts(line));
    }

    [TestMethod]
    public void TrackStretchesAreJudgedOneByOneWithoutDispatchStretches()
    {
        var line = NewLine(Unmanned, middleTracks: 2, withDispatchStretches: false);
        MeetAtMiddle(line);

        Assert.IsEmpty(StretchConflicts(line), "Without dispatch stretches, the trains are never on the same track stretch at once.");
    }

    [TestMethod]
    public void OnlyUnmannedLocationsCanBeControlled()
    {
        Assert.IsTrue(Unmanned.CanBeControlled);
        Assert.IsTrue(BlockPost.CanBeControlled);
        Assert.IsTrue(new IndustrialArea(9, "Works", "W").CanBeControlled);
        Assert.IsFalse(new Station(9, "Manned", "Mn") { IsManned = true }.CanBeControlled);
        Assert.IsFalse(new Station(9, "Shadow", "Sh") { IsShadow = true }.CanBeControlled);
        Assert.IsFalse(new OtherLocation(9, "Halt", "H").CanBeControlled);
    }

    [TestMethod]
    public void ControllerIsIgnoredWhereTheLocationCannotBeControlled()
    {
        var manned = new Station(9, "Manned", "Mn") { IsManned = true, ControlledBy = new Station(1, "Alpha", "A") };

        Assert.IsFalse(manned.IsControlPoint);
    }
}
