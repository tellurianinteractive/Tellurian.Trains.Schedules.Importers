namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// Signals at an operation location (<c>ModelSurfaceAnalysis.md</c> 3.3): entered for a station or an industrial area,
/// always at a signal-controlled location, never at an other location.
/// </summary>
[TestClass]
public sealed class OperationLocationSignalsTests
{
    [TestMethod]
    public void EachKindHasSignalsAsItCan()
    {
        Assert.IsFalse(((OperationLocation)new Station(1, "Tellby", "Tby")).HasSignals);
        Assert.IsTrue(((OperationLocation)new Station(1, "Tellby", "Tby") { HasSignals = true }).HasSignals);
        Assert.IsTrue(((OperationLocation)new IndustrialArea(2, "Sockerbruket", "Skb") { HasSignals = true }).HasSignals);
        Assert.IsTrue(((OperationLocation)new SignalControlledLocation(3, "Kvarntorp", "Kvt")).HasSignals, "signal controlled by design");
        Assert.IsFalse(((OperationLocation)new OtherLocation(4, "Hålan", "Hål")).HasSignals);
    }

    [TestMethod]
    public void ANewerRevisionTakesTheSignals()
    {
        var current = new Station(1, "Tellby", "Tby");
        var source = new Station(1, "Tellby", "Tby") { HasSignals = true };

        LayoutUpdate.CopyWhatItIs(source, current);

        Assert.IsTrue(current.HasSignals);
    }

    [TestMethod]
    public void ChangingKindKeepsTheSignalsWhereTheNewKindCanHaveThem()
    {
        var station = new Station(1, "Tellby", "Tby") { HasSignals = true };

        Assert.IsTrue(((IndustrialArea)OperationLocationRules.Conforming(station, OperationLocationKind.IndustrialArea)).HasSignals);
        CollectionAssert.Contains(OperationLocationRules.ClearedBy(station, OperationLocationKind.Other).ToArray(), nameof(Station.HasSignals));
    }
}
