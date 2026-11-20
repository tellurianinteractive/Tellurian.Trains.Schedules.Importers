using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>Standard and scale are referred to by short name, with the Module Registry's id beside it.</summary>
[TestClass]
public class ModuleStandardTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private static readonly ModuleStandard FremoH0 = new() { Id = 3, ShortName = "FREMO H0-RE", Scale = new() { Id = 2, ShortName = "H0", Denominator = 87 } };

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
    public void AStandardIsWrittenByShortNameWithTheRegistrysId()
    {
        var json = JsonSerializer.Serialize(new Layout { PreferredStandard = FremoH0 }, Options);
        using var document = JsonDocument.Parse(json);
        var standard = document.RootElement.GetProperty("preferredStandard");
        Assert.AreEqual("FREMO H0-RE", standard.GetProperty("shortName").GetString());
        Assert.AreEqual(3, standard.GetProperty("id").GetInt32());
        Assert.AreEqual("H0", standard.GetProperty("scale").GetProperty("shortName").GetString());
        Assert.IsFalse(document.RootElement.TryGetProperty("scale", out _));
        Assert.AreEqual(FremoH0, JsonSerializer.Deserialize<Layout>(json, Options)!.PreferredStandard);
    }

    [TestMethod]
    public void AStandardWithoutTheRegistryHasNoId()
    {
        var read = JsonSerializer.Deserialize<ModuleStandard>("""{"shortName":"Own standard"}""", Options)!;
        Assert.IsNull(read.Id);
        Assert.AreEqual("Own standard", read.ShortName);
    }
}
