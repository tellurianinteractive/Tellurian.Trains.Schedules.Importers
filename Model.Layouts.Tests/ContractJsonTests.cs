using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// The contract as JSON, written the way its users write it: camelCase property names, and enum values by their name
/// in the code. The kind of a location is its type, and in JSON the type discriminator <c>"kind"</c>.
/// </summary>
[TestClass]
public class ContractJsonTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [TestMethod]
    public void TheKindIsWrittenFirstAndEachKindHasOnlyItsOwnProperties()
    {
        OperationLocation location = new SignalControlledLocation(1, "Kvarntorp", "Kvt") { CountryCode = "SE", TrainsCanCross = true, Centre = 1.2 };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(location, Options));
        var names = document.RootElement.EnumerateObject().Select(p => p.Name).ToList();

        Assert.AreEqual("kind", names[0]);
        Assert.AreEqual("SignalControlled", document.RootElement.GetProperty("kind").GetString());
        Assert.IsTrue(document.RootElement.GetProperty("trainsCanCross").GetBoolean());
        Assert.AreEqual("SE", document.RootElement.GetProperty("countryCode").GetString());
        CollectionAssert.DoesNotContain(names, "isShadow", "only a station can be a shadow station");
        CollectionAssert.DoesNotContain(names, "isManned", "only a station can be manned");
        CollectionAssert.DoesNotContain(names, "layout", "the parent is never written");
    }

    [TestMethod]
    [DataRow("Station", typeof(Station))]
    [DataRow("SignalControlled", typeof(SignalControlledLocation))]
    [DataRow("IndustrialArea", typeof(IndustrialArea))]
    [DataRow("Other", typeof(OtherLocation))]
    public void TheKindDecidesTheTypeRead(string kind, Type type)
    {
        var location = JsonSerializer.Deserialize<OperationLocation>($$"""{"kind":"{{kind}}","id":7,"name":"Tellby","signature":"Tby"}""", Options)!;

        Assert.IsInstanceOfType(location, type);
        Assert.AreEqual(Enum.Parse<OperationLocationKind>(kind), location.Kind);
        Assert.AreEqual(7, location.Id);
    }

    [TestMethod]
    public void ATrackIsWrittenWithTheNamesOfTheContract()
    {
        var json = JsonSerializer.Serialize(new StationTrack(1, "1") { Direction = TrackDirection.LeftToRight, SpeedLimit = 40, Usage = "Loading" }, Options);
        using var document = JsonDocument.Parse(json);

        Assert.AreEqual("1", document.RootElement.GetProperty("designation").GetString());
        Assert.AreEqual("LeftToRight", document.RootElement.GetProperty("direction").GetString());
        Assert.AreEqual(40, document.RootElement.GetProperty("speedLimit").GetInt32());
        Assert.IsFalse(document.RootElement.TryGetProperty("station", out _), "the parent is never written");
    }

    [TestMethod]
    public void UnknownPropertiesAreKept()
    {
        const string Json = """{"kind":"Station","name":"Tellby","signature":"Tby","difficulty":3}""";

        var location = JsonSerializer.Deserialize<OperationLocation>(Json, Options)!;

        Assert.AreEqual(3, JsonDocument.Parse(JsonSerializer.Serialize(location, Options)).RootElement.GetProperty("difficulty").GetInt32());
    }

    [TestMethod]
    public void AStationWithItsDecisionsRoundTrips()
    {
        OperationLocation tellby = new Station(1, "Tellby", "Tby")
        {
            IsManned = true,
            Instructions = "Shunt on track 3 only.",
            ShiftCount = 2,
            StartLabels = Timetables.StartLabelPrinting.First,
            Tracks = { new StationTrack(1, "1") { IsMain = true, Usage = "Passenger", PlatformLength = 2.4 } },
        };

        var read = (Station)JsonSerializer.Deserialize<OperationLocation>(JsonSerializer.Serialize(tellby, Options), Options)!;

        Assert.IsTrue(read.IsManned);
        Assert.AreEqual("Shunt on track 3 only.", read.Instructions);
        Assert.AreEqual(2, read.ShiftCount);
        Assert.AreEqual(Timetables.StartLabelPrinting.First, read.StartLabels);
        Assert.AreEqual("Passenger", read.Tracks.Single().Usage);
        Assert.AreEqual(2.4, read.Tracks.Single().PlatformLength);
    }

    [TestMethod]
    public void ALayoutWithItsModulesRoundTrips()
    {
        var tellby = new Station(1, "Tellby", "Tby") { IsManned = true };
        var layout = new Layout
        {
            SourceId = Guid.NewGuid(),
            SourceRevision = 7,
            OperationLocations = { tellby },
            Modules =
            {
                new()
                {
                    Id = 1,
                    Module = new() { Name = "Tellby", Prefix = "SF", Number = 12, HasFremoDesignation = true, ModuleOwners = "Stefan, Module Club", OperationLocation = tellby },
                },
                new() { Id = 2, Module = new() { Name = "Curve", PackageLabel = "Box 1", Tracks = { new StationTrack(2, "1") } } },
            },
        };

        var json = JsonSerializer.Serialize(layout, Options);
        var read = JsonSerializer.Deserialize<Layout>(json, Options)!;
        read.RestoreReferences();
        var modules = read.Modules.ToList();

        Assert.AreEqual(layout.SourceId, read.SourceId);
        Assert.AreEqual(7, read.SourceRevision);
        Assert.AreEqual(1, JsonNode.Parse(json)!["modules"]![0]!["module"]!["operationLocationId"]!.GetValue<int>(), "in a layout a module refers to its location by id");
        Assert.AreEqual("SF 0012 Tellby", modules[0].Module.DisplayName);
        Assert.AreSame(read.OperationLocations.Single(), modules[0].Module.OperationLocation);
        Assert.IsTrue(((Station)modules[0].Module.OperationLocation!).IsManned);
        Assert.AreEqual("Box 1", modules[1].Module.PackageLabel);
        Assert.IsNull(modules[1].Module.OperationLocation);
        Assert.AreEqual("1", modules[1].Module.Tracks.Single().Designation, "a plain-line module keeps its own tracks");
    }

    [TestMethod]
    public void AModuleOnItsOwnHasItsLocationNestedWithTheTracks()
    {
        var tellby = new Station(1, "Tellby", "Tby") { IsManned = true };
        tellby.Tracks.Add(new StationTrack(1, "1"));
        var module = new Module { Name = "Tellby", Prefix = "SF", Number = 12, OperationLocation = tellby, Tracks = tellby.Tracks };

        var json = JsonNode.Parse(JsonSerializer.Serialize(module, Options))!.AsObject();
        var read = JsonSerializer.Deserialize<Module>(json.ToJsonString(), Options)!;

        Assert.IsNull(json["operationLocationId"], "on its own a module nests its location");
        Assert.IsNull(json["tracks"], "the tracks are written with the location that owns them");
        Assert.AreEqual("1", json["operationLocation"]!["tracks"]![0]!["designation"]!.GetValue<string>());
        Assert.IsTrue(((Station)read.OperationLocation!).IsManned);
        Assert.AreSame(read.OperationLocation.Tracks, read.Tracks, "the module's tracks are its location's");
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void TracksWrittenOnTheModuleBecomeItsLocationsWhenTheLocationHasNone(bool locationFirst)
    {
        var location = """ "operationLocation": { "kind": "Station", "id": 0, "name": "Tellby", "signature": "Tby", "tracks": [] } """;
        var tracks = """ "tracks": [ { "id": 1, "designation": "1" }, { "id": 2, "designation": "2" } ] """;
        var json = $$"""{ "name": "Tellby", {{(locationFirst ? location + "," + tracks : tracks + "," + location)}} }""";

        var module = JsonSerializer.Deserialize<Module>(json, Options)!;
        var written = JsonNode.Parse(JsonSerializer.Serialize(module, Options))!;

        Assert.HasCount(2, module.OperationLocation!.Tracks);
        Assert.AreSame(module.OperationLocation.Tracks, module.Tracks);
        Assert.HasCount(2, written["operationLocation"]!["tracks"]!.AsArray(), "written again, the tracks are with the location");
    }

    [TestMethod]
    public void AStretchIsWrittenWithTheIdsOfItsEnds()
    {
        var layout = new Layout();
        var tellby = new Station(1, "Tellby", "Tby") { Layout = layout };
        var ytterby = new Station(2, "Ytterby", "Yb") { Layout = layout };
        var stretch = new TrackStretch(3, tellby, ytterby, 12.5) { StartSide = ExitSide.Right, EndSide = ExitSide.Left, ModuleIds = [4, 5] };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(stretch, Options));

        Assert.AreEqual(1, document.RootElement.GetProperty("startId").GetInt32());
        Assert.AreEqual(2, document.RootElement.GetProperty("endId").GetInt32());
        Assert.AreEqual("Right", document.RootElement.GetProperty("startSide").GetString());
        Assert.IsFalse(document.RootElement.TryGetProperty("start", out _), "the end itself is not written");
    }

    [TestMethod]
    public void AnEarlierLocationIsNormalisedBeforeItIsRead()
    {
        var node = System.Text.Json.Nodes.JsonNode.Parse("""
            { "module": { "operationLocation": { "name": "Tellby", "signature": "Tby", "kind": "signalControlled", "trainsCanCross": true } },
              "early": { "operationLocations": [ { "name": "Ytterby", "signature": "Yb" } ] } }
            """)!;

        OperationLocationJson.Normalise(node);

        var location = node["module"]!["operationLocation"]!.AsObject();
        Assert.AreEqual("kind", location.First().Key);
        Assert.AreEqual("SignalControlled", location["kind"]!.GetValue<string>());
        Assert.IsInstanceOfType<SignalControlledLocation>(location.Deserialize<OperationLocation>(Options));
        Assert.AreEqual("Station", node["early"]!["operationLocations"]![0]!["kind"]!.GetValue<string>());
    }
}
