using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>A standard has its own Guid; it is written by short name and scale where it is only named (decisions 11 and 12).</summary>
[TestClass]
public class ModuleStandardTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private static readonly Guid FremoId = Guid.Parse("5b6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4b01");
    private static readonly ModuleStandard FremoH0 = new() { Id = FremoId, ShortName = "FREMO H0-RE", Scale = new() { Id = 3, ShortName = "H0", Denominator = 87 } };

    [TestMethod]
    public void TheScaleOfAModuleIsThatOfItsStandard()
    {
        Assert.AreEqual("H0", new Module { Name = "Curve", Standard = FremoH0 }.Scale!.ShortName);
        Assert.IsNull(new Module { Name = "Curve" }.Scale);
        Assert.IsNull(new Module { Name = "Curve", Standard = new() { ShortName = "Own" } }.Scale);
    }

    [TestMethod]
    public void TheScaleOfALayoutIsThatOfItsPreferredStandard()
    {
        Assert.AreEqual(87, new Layout { PreferredStandard = FremoH0 }.Scale!.Denominator);
        Assert.IsNull(new Layout().Scale);
    }

    [TestMethod]
    public void AStandardIsWrittenWithItsIdShortNameAndScale()
    {
        var json = JsonSerializer.Serialize(new Layout { PreferredStandard = FremoH0 }, Options);
        using var document = JsonDocument.Parse(json);
        var standard = document.RootElement.GetProperty("preferredStandard");
        Assert.AreEqual("FREMO H0-RE", standard.GetProperty("shortName").GetString());
        Assert.AreEqual(FremoId, standard.GetProperty("id").GetGuid());
        Assert.AreEqual("H0", standard.GetProperty("scale").GetProperty("shortName").GetString());
        Assert.IsFalse(document.RootElement.TryGetProperty("scale", out _));
        Assert.AreEqual(FremoH0, JsonSerializer.Deserialize<Layout>(json, Options)!.PreferredStandard);
    }

    [TestMethod]
    public void AStandardThatIsOnlyNamedHasNoId()
    {
        var read = JsonSerializer.Deserialize<ModuleStandard>("""{"shortName":"Own standard"}""", Options)!;
        Assert.IsNull(read.Id);
        Assert.AreEqual("Own standard", read.ShortName);
    }

    [TestMethod]
    public void NameOnlyKeepsTheIdTheShortNameAndTheScaleName()
    {
        var full = FremoH0.With(s => { s.Gauges = [new(16.5, "H0", 1435), new(12, "H0m", 1000)]; s.RailTopHeight = 1300; s.Countries = ["SE"]; });

        var named = full.NameOnly;

        Assert.AreEqual(("FREMO H0-RE", "H0"), (named.ShortName, named.Scale!.ShortName));
        Assert.AreEqual(full.Id, named.Id);
        Assert.IsNull(named.Gauges);
        Assert.IsNull(named.Scale.Denominator);
        Assert.AreEqual(16.5, full.NormalGauge);
        Assert.AreEqual("H0m", full.GaugeOf(12.02)!.Name);
    }

    [TestMethod]
    public void DefaultsApplyWhereNothingIsStated()
    {
        var standard = new ModuleStandard { ShortName = "Own" };
        Assert.AreEqual(ModuleStandard.DefaultRailTopHeight, standard.RailTopHeightOrDefault);
        Assert.AreEqual(ModuleStandard.DefaultOperatorSpaceDepth, standard.OperatorSpaceDepthOrDefault);
        Assert.AreEqual(ModuleStandard.DefaultTrackColor, standard.TrackColorOrDefault);
        Assert.IsTrue(standard.AppliesTo("SE"));
        Assert.IsFalse(standard.With(s => s.Countries = ["DE"]).AppliesTo("SE"));
        Assert.IsNull(standard.NormalGauge);
    }

    [TestMethod]
    public void ACopyOwnsItsLists()
    {
        var full = FremoH0.With(s => s.Gauges = [new(16.5)]);
        var copy = full.Copy();
        copy.Gauges!.Add(new(12));
        copy.Scale!.ShortName = "N";

        Assert.HasCount(1, full.Gauges!);
        Assert.AreEqual("H0", full.Scale!.ShortName);
    }
}
