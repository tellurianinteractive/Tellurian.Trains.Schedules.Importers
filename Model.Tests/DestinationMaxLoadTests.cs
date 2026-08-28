namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies how a <see cref="Destination"/> states the most that may be brought to it: both limits
/// stand, neither silences the other, and an unset one contributes nothing at all.
/// </summary>
/// <remarks>
/// The wagon limit used to disappear whenever an axle limit was also set — a rendering rule with nothing
/// behind it, which cost a planner one of the two figures they had entered (D121).
/// </remarks>
[TestClass]
public class DestinationMaxLoadTests
{
    private static Destination Create(int wagons, int axles)
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var station = timetable.Layout.OperationLocations.OfType<Station>().First();
        return new Destination { Location = station, MaxNumberOfWagons = wagons, MaxNumberOfAxles = axles };
    }

    [TestMethod]
    public void BothLimitsStandWhenBothAreSet()
    {
        var destination = Create(wagons: 12, axles: 16);

        Assert.IsTrue(destination.HasMaxLoad);
        Assert.AreEqual(16, destination.MaxLoad.Axles, "The axle limit is kept.");
        Assert.AreEqual(12, destination.MaxLoad.Wagons, "The wagon limit is not silenced by the axle one.");
    }

    [TestMethod]
    public void OnlyTheLimitThatIsSetIsStated()
    {
        var wagonsOnly = Create(wagons: 12, axles: 0);
        Assert.IsNull(wagonsOnly.MaxLoad.Axles, "Zero axles means no axle limit, not a limit of none.");
        Assert.AreEqual(12, wagonsOnly.MaxLoad.Wagons);

        var axlesOnly = Create(wagons: 0, axles: 16);
        Assert.AreEqual(16, axlesOnly.MaxLoad.Axles);
        Assert.IsNull(axlesOnly.MaxLoad.Wagons);
    }

    [TestMethod]
    public void AnUnlimitedDestinationStatesNothing()
    {
        var destination = Create(wagons: 0, axles: 0);

        Assert.IsFalse(destination.HasMaxLoad);
        Assert.IsNull(destination.MaxLoad.Axles);
        Assert.IsNull(destination.MaxLoad.Wagons);
        Assert.AreEqual(string.Empty, destination.MaxLoadText, "An absent limit says 'not restricted' by being absent.");
        Assert.AreEqual(destination.PlaceText, destination.ToText, "With nothing to limit, the text is the place alone.");
    }

    [TestMethod]
    public void TheWordedFormCarriesBothLimits()
    {
        var destination = Create(wagons: 12, axles: 16);

        var text = destination.MaxLoadText;
        StringAssert.Contains(text, "16", "The axle figure is in the worded form.");
        StringAssert.Contains(text, "12", "The wagon figure is in the worded form.");
        StringAssert.Contains(destination.ToText, text, "A note carries the limits after the place.");
        Assert.IsFalse(destination.PlaceText.Contains("16", StringComparison.Ordinal),
            "The place alone carries no limit — that is what a report with a load column prints.");
    }
}
