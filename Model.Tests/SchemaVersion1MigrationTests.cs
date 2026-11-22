using System.Text.Json;
using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// The migration of a plan from schema version 0 to 1 (<c>Documentation/ModelHarmonisation.md</c>, sections 3 and 5),
/// measured against the plan read the way version 0 was read (<see cref="PlanVersion0Reader"/>).
/// </summary>
[TestClass]
public sealed class SchemaVersion1MigrationTests
{
    private const string VarnamoFileName = "Plan.Varnamo-Moduldagar.json";

    [TestMethod]
    public void AMigratedPlanHoldsWhatTheOldReadingDid()
    {
        var json = VarnamoJson();
        var expected = PlanFingerprint.Of(PlanVersion0Reader.Read(json));

        PlanFingerprint.AssertSame(expected, PlanFingerprint.Of(PlanFile.Read(json)));
    }

    [TestMethod]
    public void APlanOfVersion1HoldsTheSameAfterBeingWrittenAndReadAgain()
    {
        var plan = PlanFile.Read(VarnamoJson());
        var expected = PlanFingerprint.Of(plan);

        PlanFingerprint.AssertSame(expected, PlanFingerprint.Of(PlanFile.Read(PlanFile.Write(plan))));
    }

    [TestMethod]
    public void TheMigrationGivesWhatVersion1Writes()
    {
        var json = VarnamoJson();
        var migrated = PlanFingerprint.Of(PlanFile.Read(json));
        var written = PlanFingerprint.Of(PlanFile.Read(PlanFile.Write(PlanVersion0Reader.Read(json))));

        PlanFingerprint.AssertSame(written, migrated);
    }

    [TestMethod]
    public void Version1HasNoReferenceBookkeeping()
    {
        var text = PlanFile.Write(PlanFile.Read(VarnamoJson()));

        Assert.IsFalse(text.Contains("\"$id\"", StringComparison.Ordinal), "$id");
        Assert.IsFalse(text.Contains("\"$ref\"", StringComparison.Ordinal), "$ref");
        Assert.IsFalse(text.Contains("\"$values\"", StringComparison.Ordinal), "$values");
    }

    [TestMethod]
    public void Version1IsCamelCaseWithEnumsByName()
    {
        var plan = JsonNode.Parse(PlanFile.Write(PlanFile.Read(VarnamoJson())))!["plan"]!;
        var layout = plan["timetable"]!["layout"]!;

        Assert.IsNotNull(layout["operationLocations"]);
        Assert.IsNull(layout["OperationLocations"]);
        // Values at their default are left out (schema version 4), so look at those that are written.
        Assert.AreEqual(JsonValueKind.String, layout["operationLocations"]![0]!["kind"]!.GetValueKind());
        var contents = plan["timetable"]!["trainCategories"]!.AsArray().Select(c => c!["content"]).OfType<JsonNode>().ToList();
        Assert.IsNotEmpty(contents, "some category has a content other than the default");
        Assert.IsTrue(contents.All(c => c.GetValueKind() == JsonValueKind.String));
    }

    [TestMethod]
    public void EveryTrainPartHasAnIdOfItsOwn()
    {
        var plan = PlanFile.Read(VarnamoJson());
        var parts = plan.Schedules.SelectMany(s => s.Parts).Concat(plan.DriverDuties.SelectMany(d => d.Parts)).Distinct(ReferenceEqualityComparer.Instance).Cast<ScheduledTrainPart>().ToList();

        Assert.IsTrue(parts.All(p => p.Id > 0), "no part without an id");
        Assert.AreEqual(parts.Count, parts.Select(p => p.Id).Distinct().Count(), "no two parts with the same id");
    }

    [TestMethod]
    public void APartOfADutyAndASchedule_IsTheSameObject()
    {
        var plan = PlanFile.Read(VarnamoJson());
        var scheduleParts = plan.Schedules.SelectMany(s => s.Parts).ToHashSet(ReferenceEqualityComparer.Instance);
        var dutyParts = plan.DriverDuties.SelectMany(d => d.Parts).Distinct(ReferenceEqualityComparer.Instance).Cast<ScheduledTrainPart>().ToList();

        Assert.AreEqual(279, dutyParts.Count);
        Assert.AreEqual(211, dutyParts.Count(scheduleParts.Contains), "the parts of a duty that a vehicle schedule has are that schedule's");
        Assert.AreEqual(68, dutyParts.Count(p => p.Schedule is null), "and the rest belong to no schedule");
    }

    [TestMethod]
    public void AnEarlyCategoryHasItsFlagsAsContent()
    {
        var json = """
            { "$id": "1", "Name": "Plan", "Timetable": { "$id": "2", "Layout": { "$id": "3", "Name": "Plan" },
              "TrainCategories": { "$id": "4", "$values": [ { "$id": "5", "Id": 1, "Name": "Mixed", "IsPassenger": true, "IsFreight": true } ] } } }
            """;

        var plan = PlanFile.Read(json);

        Assert.AreEqual(TrainContent.Passenger | TrainContent.Cargo, plan.Timetable.TrainCategories.Single().Content);
    }

    private static string VarnamoJson() => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", VarnamoFileName));
}
