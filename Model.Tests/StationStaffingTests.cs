using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers how a manned station's staffing is counted as participants, and that it survives being saved.
/// </summary>
[TestClass]
public class StationStaffingTests
{
    private static Station Manned(int posts, int shifts = 1)
    {
        var station = new Station(1, "Alvesta", "Av") { IsManned = true, ShiftCount = shifts };
        for (var i = 0; i < posts; i++) station.StaffPosts.Add(new StaffPost { Roles = StaffRolesExtensions.SoleOperator });
        return station;
    }

    [TestMethod]
    public void EachPostIsOneParticipant()
    {
        Assert.AreEqual(3, Manned(3).ParticipantsNeeded);
    }

    [TestMethod]
    public void EveryShiftNeedsTheWholeStaffAgain()
    {
        Assert.AreEqual(6, Manned(2, shifts: 3).ParticipantsNeeded);
    }

    [TestMethod]
    public void ShiftsAreKeptBetweenOneAndThree()
    {
        Assert.AreEqual(2, Manned(2, shifts: 0).ParticipantsNeeded);
        Assert.AreEqual(6, Manned(2, shifts: 9).ParticipantsNeeded);
    }

    [TestMethod]
    public void AnUnmannedStationNeedsNoParticipantsButKeepsItsPosts()
    {
        var station = Manned(2);
        station.IsManned = false;

        Assert.AreEqual(0, station.ParticipantsNeeded);
        Assert.HasCount(2, station.StaffPosts);
        station.IsManned = true;
        Assert.AreEqual(2, station.ParticipantsNeeded);
    }

    [TestMethod]
    public void TheLayoutAddsUpItsStations()
    {
        var layout = new Layout { Name = "Test" };
        layout.Add(Manned(2, shifts: 2));
        var other = Manned(1);
        other.Id = 2;
        other.Name = "Nässjö";
        other.Signature = "Nä";
        layout.Add(other);

        Assert.AreEqual(5, layout.ParticipantsNeeded);
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
        var options = new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve, MaxDepth = 256 };
        var station = Manned(2, shifts: 2);
        station.StaffPosts[1].Roles = StaffRoles.Trainee;

        var restored = JsonSerializer.Deserialize<Station>(JsonSerializer.Serialize(station, options), options)!;

        Assert.AreEqual(2, restored.ShiftCount);
        CollectionAssert.AreEqual(
            new[] { StaffRolesExtensions.SoleOperator, StaffRoles.Trainee },
            restored.StaffPosts.Select(post => post.Roles).ToArray());
    }
}
