using Tellurian.Trains.Schedules.Model.PlanFiles;
using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers how a manned station's staffing is counted as participants, and that it survives being saved.
/// </summary>
[TestClass]
public class StationStaffingTests
{
    // A timetable for each test, which keeps what is decided for its stations.
    private readonly Timetable Timetable = new("Test", new Layout { Name = "Test" });

    private Station Manned(int posts, int shifts = 1, int id = 1)
    {
        var station = (Station)Timetable.Layout.Add(new Station(id, $"Station {id}", $"S{id}") { IsManned = true });
        var planning = Timetable.EnsurePlanningOf(station);
        planning.ShiftCount = shifts;
        for (var i = 0; i < posts; i++) planning.StaffPosts.Add(new StaffPost { Roles = StaffRolesExtensions.SoleOperator });
        return station;
    }

    [TestMethod]
    public void EachPostIsOneParticipant()
    {
        Assert.AreEqual(3, Timetable.ParticipantsNeeded(Manned(3)));
    }

    [TestMethod]
    public void EveryShiftNeedsTheWholeStaffAgain()
    {
        Assert.AreEqual(6, Timetable.ParticipantsNeeded(Manned(2, shifts: 3)));
    }

    [TestMethod]
    public void ShiftsAreKeptBetweenOneAndThree()
    {
        Assert.AreEqual(2, Timetable.ParticipantsNeeded(Manned(2, shifts: 0)));
        Assert.AreEqual(6, Timetable.ParticipantsNeeded(Manned(2, shifts: 9, id: 2)));
    }

    [TestMethod]
    public void AnUnmannedStationNeedsNoParticipantsButKeepsItsPosts()
    {
        var station = Manned(2);
        station.IsManned = false;

        Assert.AreEqual(0, Timetable.ParticipantsNeeded(station));
        Assert.HasCount(2, Timetable.PlanningOf(station).StaffPosts);
        station.IsManned = true;
        Assert.AreEqual(2, Timetable.ParticipantsNeeded(station));
    }

    [TestMethod]
    public void TheLayoutAddsUpItsStations()
    {
        Manned(2, shifts: 2);
        Manned(1, id: 2);

        Assert.AreEqual(5, Timetable.ParticipantsNeeded());
    }

    [TestMethod]
    public void APostFillsSeveralRoles()
    {
        var post = new StaffPost();
        post.Set(StaffRoles.Dispatcher, true);
        post.Set(StaffRoles.Shuntman, true);
        post.Set(StaffRoles.Dispatcher, false);

        Assert.IsFalse(post.Fills(StaffRoles.Dispatcher));
        Assert.IsTrue(post.Fills(StaffRoles.Shuntman));
    }

    [TestMethod]
    public void StaffingSurvivesSaving()
    {
        var station = Manned(2, shifts: 2);
        Timetable.PlanningOf(station).StaffPosts[1].Roles = StaffRoles.Trainee;

        var restored = PlanFile.Read(PlanFile.Write(Plan.Create("Test", Timetable))).Timetable;

        var planning = restored.PlanningOf(restored.Layout.OperationLocations.Single());
        Assert.AreEqual(2, planning.ShiftCount);
        CollectionAssert.AreEqual(
            new[] { StaffRolesExtensions.SoleOperator, StaffRoles.Trainee },
            planning.StaffPosts.Select(post => post.Roles).ToArray());
    }
}

/// <summary>Covers where the shifts of a session fall.</summary>
[TestClass]
public class ShiftTimesTests
{
    private static GeneralSettings Settings(int startHour, int endHour) => new()
    {
        StartTime = TimeSpan.FromHours(startHour),
        EndTime = TimeSpan.FromHours(endHour),
    };

    [TestMethod]
    public void OneShiftIsTheWholeOperatingWindow()
    {
        var shifts = Settings(10, 18).ShiftTimes(1);

        CollectionAssert.AreEqual(new[] { (TimeSpan.FromHours(10), TimeSpan.FromHours(18)) }, shifts.ToArray());
    }

    [TestMethod]
    public void ShiftsAreEqualAndFollowOnFromEachOther()
    {
        var shifts = Settings(10, 19).ShiftTimes(3);

        CollectionAssert.AreEqual(
            new[]
            {
                (TimeSpan.FromHours(10), TimeSpan.FromHours(13)),
                (TimeSpan.FromHours(13), TimeSpan.FromHours(16)),
                (TimeSpan.FromHours(16), TimeSpan.FromHours(19)),
            },
            shifts.ToArray());
    }

    [TestMethod]
    public void BoundariesFallOnWholeFiveMinutes()
    {
        var shifts = Settings(10, 18).ShiftTimes(3);

        Assert.AreEqual(TimeSpan.FromMinutes(10 * 60 + 160), shifts[0].End, "8 h / 3 = 2 h 40 min.");
        Assert.AreEqual(TimeSpan.FromHours(18), shifts[2].End);
    }

    [TestMethod]
    public void RunningOverMidnightStartsAtTheFirstSessionAndLastsADay()
    {
        var settings = new GeneralSettings { RunsOverMidnight = true, FirstSessionStartTime = TimeSpan.FromHours(10) };

        var shifts = settings.ShiftTimes(2);

        Assert.AreEqual(TimeSpan.FromHours(10), shifts[0].Start);
        Assert.AreEqual(TimeSpan.FromHours(22), shifts[0].End);
        Assert.AreEqual(TimeSpan.FromHours(34), shifts[1].End);
    }

    [TestMethod]
    public void WithoutAFirstSessionStartTheWindowStartsTheFirstShift()
    {
        var settings = new GeneralSettings { RunsOverMidnight = true, StartTime = TimeSpan.Zero };

        Assert.AreEqual(TimeSpan.Zero, settings.ShiftTimes(1)[0].Start);
    }

    [TestMethod]
    public void TheNumberOfShiftsIsKeptBetweenOneAndThree()
    {
        Assert.HasCount(1, Settings(10, 18).ShiftTimes(0));
        Assert.HasCount(3, Settings(10, 18).ShiftTimes(7));
    }
}
