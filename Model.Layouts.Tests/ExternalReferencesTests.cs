using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>The Module Registry's entry among a thing's external references, read and written by name.</summary>
[TestClass]
public sealed class ExternalReferencesTests
{
    private static Dictionary<string, JsonElement> References(string json) =>
        JsonSerializer.Deserialize(json, LayoutJsonContextAccess.Dictionary)!;

    [TestMethod]
    public void TheRegistrysIdIsReadByItsName()
    {
        var references = References("""{ "moduleRegistry": { "stationId": 4711, "version": "2" }, "other": { "x": 1 } }""");

        Assert.AreEqual(4711, references.ModuleRegistryId(ExternalReferences.StationId));
        Assert.IsNull(references.ModuleRegistryId(ExternalReferences.ModuleId));
        Assert.IsNull(((IReadOnlyDictionary<string, JsonElement>?)null).ModuleRegistryId(ExternalReferences.StationId));
    }

    [TestMethod]
    public void SettingTheRegistrysIdKeepsWhatElseIsThere()
    {
        var references = ExternalReferences.WithModuleRegistryId(References("""{ "moduleRegistry": { "version": "2" }, "other": { "x": 1 } }"""), ExternalReferences.StationId, 12);

        Assert.AreEqual(12, references.ModuleRegistryId(ExternalReferences.StationId));
        Assert.AreEqual("2", references[ExternalReferences.ModuleRegistry].GetProperty("version").GetString());
        Assert.AreEqual(1, references["other"].GetProperty("x").GetInt32());
    }

    [TestMethod]
    public void TwoThingsAreTheSameInTheRegistryByItsIdWhateverElseTheEntrySays()
    {
        var mine = References("""{ "moduleRegistry": { "stationId": 7, "version": "1" } }""");
        var theirs = References("""{ "moduleRegistry": { "version": "2", "stationId": 7 } }""");
        var another = References("""{ "moduleRegistry": { "stationId": 8 } }""");

        Assert.IsTrue(ExternalReferences.ReferToTheSame(mine, theirs, ExternalReferences.StationId));
        Assert.IsFalse(ExternalReferences.ReferToTheSame(mine, another, ExternalReferences.StationId));
    }

    [TestMethod]
    public void OtherSystemsAreComparedByContentNotByText()
    {
        var mine = References("""{ "other": { "a": 1, "b": 2 } }""");
        var theirs = References("""{ "other": {"a":1,"b":2} }""");

        Assert.IsTrue(ExternalReferences.ReferToTheSame(mine, theirs, ExternalReferences.StationId));
        Assert.IsTrue(ExternalReferences.Same(mine, theirs));
    }
}

file static class LayoutJsonContextAccess
{
    public static System.Text.Json.Serialization.Metadata.JsonTypeInfo<Dictionary<string, JsonElement>> Dictionary { get; } =
        (System.Text.Json.Serialization.Metadata.JsonTypeInfo<Dictionary<string, JsonElement>>)Layout.JsonSerializerOptions.GetTypeInfo(typeof(Dictionary<string, JsonElement>));
}
