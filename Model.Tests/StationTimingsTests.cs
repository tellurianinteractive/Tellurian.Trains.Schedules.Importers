namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers which operational times apply at which kind of operation location.
/// </summary>
[TestClass]
public class StationTimingsTests
{
    [TestMethod]
    public void A_manned_station_has_both_times()
    {
        var station = new Station(1, "Munkeröd", "Mkd") { IsManned = true };
        Assert.IsTrue(station.HasLocoRunaroundTime);
        Assert.IsTrue(station.HasTrainClearanceTime);
    }

    [TestMethod]
    public void An_unmanned_station_or_shadow_yard_has_a_runaround_time_but_no_clearance_time()
    {
        foreach (var station in new[] { new Station(1, "Rubjerg", "Rbj"), new Station(2, "Shadow", "Sh") { IsShadow = true } })
        {
            Assert.IsTrue(station.HasLocoRunaroundTime, station.Name);
            Assert.IsFalse(station.HasTrainClearanceTime, station.Name);
        }
    }

    [TestMethod]
    public void Other_kinds_of_location_have_neither_time()
    {
        OperationLocation[] locations =
        [
            new SignalControlledLocation(1, "Bro", "Bro"),
            new IndustrialArea(2, "Works", "Wks"),
            new OtherLocation(3, "Halt", "Hlt"),
        ];
        foreach (var location in locations)
        {
            Assert.IsFalse(location.HasLocoRunaroundTime, location.Name);
            Assert.IsFalse(location.HasTrainClearanceTime, location.Name);
        }
    }
}
