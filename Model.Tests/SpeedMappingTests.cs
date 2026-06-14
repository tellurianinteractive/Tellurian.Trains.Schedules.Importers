using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class SpeedMappingTests
{
    // Default curve: Slow 60 km/h -> 0.15 m/s, Normal 100 -> 0.25, High 200 -> 0.35.
    private static readonly TimeAndSpeedSettings Settings = new();

    [TestMethod]
    public void MapsExactCurvePoints()
    {
        Assert.AreEqual(0.15, Settings.RealSpeedMetersPerSecond(60), 1e-9);
        Assert.AreEqual(0.25, Settings.RealSpeedMetersPerSecond(100), 1e-9);
        Assert.AreEqual(0.35, Settings.RealSpeedMetersPerSecond(200), 1e-9);
    }

    [TestMethod]
    public void InterpolatesLinearlyBetweenPoints()
    {
        Assert.AreEqual(0.20, Settings.RealSpeedMetersPerSecond(80), 1e-9);
        Assert.AreEqual(0.30, Settings.RealSpeedMetersPerSecond(150), 1e-9);
    }

    [TestMethod]
    public void ClampsBelowSlowestAndAboveFastest()
    {
        Assert.AreEqual(0.15, Settings.RealSpeedMetersPerSecond(40), 1e-9);
        Assert.AreEqual(0.35, Settings.RealSpeedMetersPerSecond(250), 1e-9);
    }

    [TestMethod]
    public void EffectiveScaleSpeedTakesLowerOfTrainAndStretch()
    {
        TestDataFactory.Init();
        var stretch = TestDataFactory.Layout().TrackStretches.First();
        Assert.AreEqual(100, stretch.Speed, "Test relies on the default stretch speed.");

        var category = new TrainCategory { Id = 1, Name = "Express", DefaultSpeed = 120 };
        var fastTrain = new Train(1, category, 1) { Category = category, MaxSpeed = 80 };
        var defaultTrain = new Train(2, category, 2) { Category = category };

        Assert.AreEqual(80, fastTrain.EffectiveScaleSpeed(stretch), "Train MaxSpeed below the stretch limit wins.");
        Assert.AreEqual(100, defaultTrain.EffectiveScaleSpeed(stretch), "Category default (120) is capped by the stretch limit (100).");
    }
}
