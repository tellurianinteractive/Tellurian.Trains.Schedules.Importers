using System.Text.Json;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers a train category's stop pattern: what it is deduced from when a plan is read, what it is
/// stored as, and which stops it reports. See <c>StopPatternRules</c>.
/// </summary>
[TestClass]
public class StopPatternTests
{
    private static readonly JsonSerializerOptions Options = PlanJson.CreateOptions();

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", Content = TrainContent.Passenger };
    private static TrainCategory Shunting => new() { Id = 2, Name = "Shunting", Prefix = "V", Content = TrainContent.Cargo, IsShunting = true };

    // Four stations in a row, with one train of the given category calling at each of them and stopping
    // where the signatures say. The train's ends are always stops, as they are for any train.
    private static Plan CreatePlan(TrainCategory category, params string[] stopSignatures)
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var timetable = new Timetable("Test", layout);
        timetable.TrainCategories.Add(category);
        var train = new Train(1, category, 100) { Sessions = Sessions.All };
        var time = Time.FromHourAndMinute(8, 0);
        var id = 1;
        foreach (var (name, signature) in new[] { ("Ytterby", "Yb"), ("Kode", "Kd"), ("Stora Höga", "Sth"), ("Stenungsund", "Sts") })
        {
            var station = new Station(id, name, signature);
            var track = new StationTrack(id, "1");
            station.Add(track);
            layout.OperationLocations.Add(station);
            var call = new StationCall(id, track, time, time.AddMinutes(2));
            train.Add(call);
            var stops = stopSignatures.Contains(signature);
            call.IsArrival = stops;
            call.IsDeparture = stops;
            time = time.AddMinutes(20);
            id++;
        }
        // The ends are stops whatever was asked for, which is what ApplyStopRules would set anyway.
        train.Calls[0].IsDeparture = true;
        train.Calls[^1].IsArrival = true;
        timetable.Add(train);
        return new Plan("Test", timetable);
    }

    private static TrainCategory CategoryOf(Plan plan) => plan.Timetable.TrainCategories.First();

    private static int IdOf(Plan plan, string signature) =>
        plan.Layout.OperationLocations.First(l => l.Signature == signature).Id;

    // ---- What a pattern is deduced from ---------------------------------------------------------

    [TestMethod]
    public void ACategoryWithNoPatternIsGivenTheOneItsTrainsRun()
    {
        var plan = CreatePlan(Passenger, "Kd");

        Assert.AreEqual(1, plan.DeduceStopPatterns());

        var category = CategoryOf(plan);
        CollectionAssert.AreEquivalent(
            new[] { IdOf(plan, "Yb"), IdOf(plan, "Kd"), IdOf(plan, "Sts") },
            category.StopLocationIds.ToArray(),
            "Where the trains stop, the two ends included.");
        Assert.IsFalse(category.StopLocationIds.Contains(IdOf(plan, "Sth")), "Stora Höga is run through.");
    }

    [TestMethod]
    public void DeducingIsIdempotentAndNeverTakesAPatternAway()
    {
        var plan = CreatePlan(Passenger, "Kd");
        var category = CategoryOf(plan);
        category.StopLocationIds.Add(IdOf(plan, "Sth"));

        Assert.AreEqual(0, plan.DeduceStopPatterns(), "A category that has a pattern is left alone.");
        CollectionAssert.AreEqual(new[] { IdOf(plan, "Sth") }, category.StopLocationIds.ToArray());
    }

    [TestMethod]
    public void ACategoryWhoseTrainsStopNowhereIsGivenNoPattern()
    {
        var plan = CreatePlan(Passenger);
        foreach (var call in plan.Timetable.Trains.First().Calls) { call.IsArrival = false; call.IsDeparture = false; }

        Assert.AreEqual(0, plan.DeduceStopPatterns());
        Assert.IsFalse(CategoryOf(plan).HasStopPattern, "An empty pattern is no pattern, not a pattern of nowhere.");
    }

    [TestMethod]
    public void AShuntingCategoryIsGivenNoPattern()
    {
        // A task works at one place and travels nowhere, so it has no route for a pattern to shape.
        var plan = CreatePlan(Shunting, "Kd");

        Assert.AreEqual(0, plan.DeduceStopPatterns());
        Assert.IsFalse(CategoryOf(plan).HasStopPattern);
    }

    [TestMethod]
    public void FillingInFromTheTrainsReplacesThePatternThatIsThere()
    {
        var plan = CreatePlan(Passenger, "Kd");
        var category = CategoryOf(plan);
        category.StopLocationIds.Add(IdOf(plan, "Sth"));

        Assert.IsTrue(plan.DeduceStopPattern(category), "The planner's own fill-in replaces what is there.");
        Assert.IsFalse(category.StopLocationIds.Contains(IdOf(plan, "Sth")));
        Assert.IsTrue(category.StopLocationIds.Contains(IdOf(plan, "Kd")));
    }

    // ---- What it is stored as -------------------------------------------------------------------

    [TestMethod]
    public void APatternSurvivesASaveAsAPlainArrayOfIds()
    {
        var plan = CreatePlan(Passenger, "Kd");
        plan.DeduceStopPatterns();
        var expected = CategoryOf(plan).StopLocationIds.ToArray();

        var json = JsonSerializer.Serialize(plan, Options);
        var restored = JsonSerializer.Deserialize<Plan>(json, Options);

        Assert.IsNotNull(restored);
        CollectionAssert.AreEqual(expected, CategoryOf(restored).StopLocationIds.ToArray());
        Assert.Contains("\"StopLocationIds\":[", json, StringComparison.Ordinal);
    }

    [TestMethod]
    public void ACategoryWithNoPatternWritesNothingAndIsDeducedOnTheWayBackIn()
    {
        var plan = CreatePlan(Passenger, "Kd");

        var json = JsonSerializer.Serialize(plan, Options);
        Assert.DoesNotContain("StopLocationIds", json, StringComparison.Ordinal,
            "An empty list says exactly what its absence does.");

        var restored = JsonSerializer.Deserialize<Plan>(json, Options);
        Assert.IsNotNull(restored);
        Assert.IsTrue(CategoryOf(restored).HasStopPattern, "Reading the plan fills it in from the trains.");
    }

    // ---- What it reports ------------------------------------------------------------------------

    [TestMethod]
    public void AStopOutsideThePatternIsReported()
    {
        var plan = CreatePlan(Passenger, "Kd", "Sth");
        var category = CategoryOf(plan);
        category.StopLocationIds.Add(IdOf(plan, "Kd"));

        var errors = plan.Timetable.Trains.First().CheckStopPattern().ToArray();

        Assert.HasCount(1, errors);
        Assert.AreEqual(ValidationErrorType.StopOutsideStopPattern, errors[0].ErrorType);
        Assert.AreEqual(Severity.Warning, errors[0].Severity);
        Assert.Contains("Stora Höga", errors[0].Message.Text, StringComparison.Ordinal);
    }

    [TestMethod]
    public void TheEndsAndThePassThroughsAreNotReported()
    {
        // The pattern names an intermediate location only: neither the train's two ends, which are stops
        // because it starts and finishes there, nor the location it runs through, is a fault.
        var plan = CreatePlan(Passenger, "Kd");
        CategoryOf(plan).StopLocationIds.Add(IdOf(plan, "Kd"));

        Assert.IsEmpty(plan.Timetable.Trains.First().CheckStopPattern());
    }

    [TestMethod]
    public void ACategoryWithNoPatternReportsNothing()
    {
        var plan = CreatePlan(Passenger, "Kd", "Sth");

        Assert.IsEmpty(plan.Timetable.Trains.First().CheckStopPattern());
    }

    [TestMethod]
    public void ThePatternThatWasDeducedReportsNoneOfTheTrainsItWasDeducedFrom()
    {
        var plan = CreatePlan(Passenger, "Kd", "Sth");
        plan.DeduceStopPatterns();

        Assert.IsEmpty(plan.GetValidationErrors(new ValidationSettings())
            .Where(e => e.ErrorType == ValidationErrorType.StopOutsideStopPattern));
    }

    [TestMethod]
    public void TheCheckIsTurnedOffWithItsOwnSetting()
    {
        var plan = CreatePlan(Passenger, "Kd", "Sth");
        CategoryOf(plan).StopLocationIds.Add(IdOf(plan, "Kd"));

        Assert.IsNotEmpty(Errors(new ValidationSettings()));
        Assert.IsEmpty(Errors(new ValidationSettings { ValidateStopPatterns = false }));

        IEnumerable<ValidationError> Errors(ValidationSettings settings) =>
            plan.GetValidationErrors(settings).Where(e => e.ErrorType == ValidationErrorType.StopOutsideStopPattern);
    }
}
