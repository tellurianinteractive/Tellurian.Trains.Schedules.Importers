using Tellurian.Trains.Schedules.Importers.Xpln.Extensions;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Tests;

/// <summary>
/// Covers the XPLN-only convention that derives the stop flags from the call times: a call whose arrival
/// equals its departure is a pass-through. The convention lives in <c>XplnTrainExtensions</c> and is applied
/// nowhere but the XPLN import; the model itself never compares the times to decide whether a call is a stop.
/// </summary>
[TestClass]
public class XplnStopConventionTests
{
    [TestMethod]
    public void IntermediateCallWithEqualTimesBecomesPassthrough()
    {
        var train = CreateImportedTrain(("A", "08:00", "08:00"), ("B", "08:20", "08:20"), ("C", "08:40", "08:40"));

        var intermediate = train.Calls[1];
        Assert.IsFalse(intermediate.IsStop, "Equal arrival and departure times mark a pass-through.");
        Assert.IsTrue(intermediate.IsPassthrough);
        Assert.IsFalse(intermediate.IsArrival);
        Assert.IsFalse(intermediate.IsDeparture);
    }

    [TestMethod]
    public void IntermediateCallWithDifferingTimesStaysAStop()
    {
        var train = CreateImportedTrain(("A", "08:00", "08:00"), ("B", "08:20", "08:25"), ("C", "08:40", "08:40"));

        var intermediate = train.Calls[1];
        Assert.IsTrue(intermediate.IsStop, "Differing arrival and departure times mark a stop.");
        Assert.IsTrue(intermediate.IsArrival);
        Assert.IsTrue(intermediate.IsDeparture);
    }

    [TestMethod]
    public void OriginWithEqualTimesKeepsItsStopAndGetsASyntheticDwell()
    {
        var train = CreateImportedTrain(("A", "08:00", "08:00"), ("B", "08:20", "08:20"), ("C", "08:40", "08:40"));

        var origin = train.Calls.First();
        Assert.IsTrue(origin.IsStop, "The origin is exempt from the equal-times rule.");
        Assert.IsTrue(origin.IsDeparture);
        Assert.IsFalse(origin.IsArrival, "The origin is a departure only.");
        Assert.AreEqual("07:50".AsTime(), origin.Arrival, "The origin gets a 10 minute synthetic dwell.");
        Assert.AreEqual("08:00".AsTime(), origin.Departure, "The origin's departure is left untouched.");
    }

    [TestMethod]
    public void TerminusWithEqualTimesKeepsItsStopAndGetsASyntheticDwell()
    {
        var train = CreateImportedTrain(("A", "08:00", "08:00"), ("B", "08:20", "08:20"), ("C", "08:40", "08:40"));

        var terminus = train.Calls.Last();
        Assert.IsTrue(terminus.IsStop, "The terminus is exempt from the equal-times rule.");
        Assert.IsTrue(terminus.IsArrival);
        Assert.IsFalse(terminus.IsDeparture, "The terminus is an arrival only.");
        Assert.AreEqual("08:40".AsTime(), terminus.Arrival, "The terminus' arrival is left untouched.");
        Assert.AreEqual("08:50".AsTime(), terminus.Departure, "The terminus gets a 10 minute synthetic dwell.");
    }

    [TestMethod]
    public void CallAtSignalControlledLocationIsNeverAStop()
    {
        var train = CreateImportedTrain(("A", "08:00", "08:00"), ("B", "08:20", "08:25"), ("C", "08:40", "08:40"));
        train.Calls[1].Track.Station = new SignalControlledLocation(99, "Block", "Bl");

        Assert.IsFalse(train.Calls[1].IsStop, "A train never stops at a signal controlled location, whatever the times say.");
    }

    /// <summary>
    /// Builds a train the way <c>XplnDataImporter</c> does: every call starts as a full stop, then the same
    /// four conventions are applied in the same order.
    /// </summary>
    private static Train CreateImportedTrain(params (string Signature, string Arrival, string Departure)[] calls)
    {
        var train = new Train(1, 1234);
        for (var i = 0; i < calls.Length; i++)
        {
            var (signature, arrival, departure) = calls[i];
            var track = new StationTrack(i + 1, "1") { Station = new Station(i + 1, signature, signature) };
            train.Add(new StationCall(i + 1, track, arrival.AsTime(), departure.AsTime())
            {
                IsArrival = true,
                IsDeparture = true,
            });
        }
        return train
            .WithFixedSingleCallTrain()
            .WithOriginAndTerminusDwell()
            .WithFirstCallDepartureOnlyAndLastCallArrivalOnly()
            .WithPassthroughCalls();
    }
}
