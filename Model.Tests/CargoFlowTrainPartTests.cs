using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies the cargo flow occurrence model: a <see cref="CargoFlowTrainPart"/> belongs to a train,
/// defaults to position 1, references a reusable catalogue <see cref="CargoFlowOptions"/> (the same
/// instance survives a JSON round-trip), and produces a destination note.
/// </summary>
[TestClass]
public class CargoFlowTrainPartTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    private static (Timetable Timetable, Train Train, CargoFlowOptions Description) Arrange()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var train = timetable.Trains.First();
        var station = timetable.Layout.OperationLocations.OfType<Station>().First();
        var description = timetable.Add(new CargoFlowOptions
        {
            OnlyWagonClasses = "U,Z",
            Destinations = { new Destination { Station = station } },
        });
        return (timetable, train, description);
    }

    [TestMethod]
    public void CreateCargoFlowDefaultsAndReferencesDescription()
    {
        var (_, train, description) = Arrange();
        var calls = train.Calls.OrderBy(c => c.SortTime).ToList();

        var cargoFlow = train.CreateCargoFlow(1, calls.First(), calls.Last(), description);

        Assert.AreEqual(1, cargoFlow.PositionInTrain, "Position defaults to 1.");
        Assert.IsTrue(cargoFlow.HasCoupleNote);
        Assert.IsTrue(cargoFlow.HasUncoupleNote);
        Assert.AreSame(description, cargoFlow.CargoFlowOptions);
        Assert.AreEqual(description.Id, cargoFlow.CargoFlowOptionsId);
        Assert.HasCount(1, train.CargoFlows);
    }

    [TestMethod]
    public void CargoFlowReferencesSameCatalogueInstanceAfterRoundTrip()
    {
        var (timetable, train, description) = Arrange();
        var calls = train.Calls.OrderBy(c => c.SortTime).ToList();
        train.CreateCargoFlow(1, calls.First(), calls.Last(), description, positionInTrain: 2);
        var plan = Plan.Create("Plan", timetable);

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options)!;

        var restoredTrain = restored.Timetable.Trains.First(t => t.CargoFlows.Count > 0);
        var restoredFlow = restoredTrain.CargoFlows.Single();
        Assert.AreEqual(2, restoredFlow.PositionInTrain);
        // The cargo flow must point at the very catalogue instance, not a copy: editing the description
        // has to affect the flow. ReferenceHandler.Preserve keeps that identity.
        Assert.AreSame(restored.Timetable.CargoFlowOptions.Single(), restoredFlow.CargoFlowOptions);
    }

    [TestMethod]
    public void DepartureNoteListsDestinations()
    {
        var (_, train, description) = Arrange();
        var calls = train.Calls.OrderBy(c => c.SortTime).ToList();
        var cargoFlow = train.CreateCargoFlow(1, calls.First(), calls.Last(), description);

        var note = cargoFlow.DepartureNotes.OfType<CargoFlowDestinationNote>().Single();

        StringAssert.Contains(note.Text, "Göteborg");
    }
}
