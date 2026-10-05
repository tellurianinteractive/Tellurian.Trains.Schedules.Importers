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
        // Not the station the train starts at: a flow never states that as a destination.
        var station = train.Calls.OrderBy(c => c.SortTime).Last().OperationLocation;
        var description = timetable.Add(new CargoFlowOptions
        {
            OnlyWagonClasses = "U,Z",
            Destinations = { new Destination { Location = station } },
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
    public void CreateCargoFlowDefaultsToOneAfterHighestPosition()
    {
        var (_, train, description) = Arrange();
        var calls = train.Calls.OrderBy(c => c.SortTime).ToList();
        train.CreateCargoFlow(1, calls.First(), calls.Last(), description, positionInTrain: 3);
        train.CreateCargoFlow(2, calls.First(), calls.Last(), description, positionInTrain: 1);

        var cargoFlow = train.CreateCargoFlow(3, calls.First(), calls.Last(), description);

        Assert.AreEqual(4, cargoFlow.PositionInTrain);
    }

    [TestMethod]
    public void CopyCargoFlowTakesSettingsOfLastFlowAtNextPosition()
    {
        var (_, train, description) = Arrange();
        var calls = train.Calls.OrderBy(c => c.SortTime).ToList();
        train.CreateCargoFlow(1, calls.First(), calls.Last(), description, positionInTrain: 1);
        var source = train.CreateCargoFlow(2, calls.First(), calls.Last(), description, positionInTrain: 2);
        source.BringsNoWagonsFromHere = true;
        source.HasCoupleNote = false;
        source.AlsoShuntAfterArrival = true;

        Assert.AreSame(source, train.LastCargoFlow);
        var copy = train.CopyCargoFlow(3, train.LastCargoFlow!);

        Assert.AreEqual(3, copy.PositionInTrain);
        Assert.AreEqual(3, copy.Id);
        Assert.AreSame(source.From, copy.From);
        Assert.AreSame(source.To, copy.To);
        Assert.AreSame(description, copy.CargoFlowOptions);
        Assert.IsTrue(copy.BringsNoWagonsFromHere);
        Assert.IsFalse(copy.HasCoupleNote);
        Assert.IsTrue(copy.AlsoShuntAfterArrival);
        Assert.HasCount(3, train.CargoFlows);
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

        StringAssert.Contains(note.ToText, description.Destinations.Single().Location.Name);
    }

    [TestMethod]
    public void DepartureNoteNamesThePlacesWithoutTheirLimits()
    {
        var (_, train, description) = Arrange();
        var destination = description.Destinations.Single();
        destination.MaxNumberOfAxles = 16;
        destination.MaxNumberOfWagons = 12;
        var calls = train.Calls.OrderBy(c => c.SortTime).ToList();
        var cargoFlow = train.CreateCargoFlow(1, calls.First(), calls.Last(), description);

        var note = cargoFlow.DepartureNotes.OfType<CargoFlowDestinationNote>().Single();

        StringAssert.Contains(note.ToText, destination.Location.Name, "The place is still what the note names.");
        // The limits belong to the cargo block's own column; in a sentence they read as part of the
        // destination that happens to precede them.
        Assert.IsFalse(note.ToText.Contains("16", StringComparison.Ordinal), $"Note text: '{note.ToText}'.");
        Assert.IsFalse(note.ToText.Contains("12", StringComparison.Ordinal), $"Note text: '{note.ToText}'.");
    }

    [TestMethod]
    public void TheFromStationIsNotStatedAsADestination()
    {
        var (_, train, description) = Arrange();
        var calls = train.Calls.OrderBy(c => c.SortTime).ToList();
        var from = calls.First().OperationLocation;
        var other = description.Destinations.Single().Location;
        Assert.AreNotEqual(from, other, "The test needs a destination other than the from-station.");
        description.Destinations.Add(new Destination { Location = from });
        var cargoFlow = train.CreateCargoFlow(1, calls.First(), calls.Last(), description);

        var note = cargoFlow.DepartureNotes.OfType<CargoFlowDestinationNote>().Single();

        CollectionAssert.AreEqual(new[] { other }, cargoFlow.StatedDestinations.Select(d => d.Location).ToArray());
        StringAssert.Contains(note.ToText, other.Name);
        Assert.IsFalse(note.ToText.Contains(from.Name, StringComparison.Ordinal), $"Note text: '{note.ToText}'.");
    }

    [TestMethod]
    public void TheFromStationWithAQualifierIsStillStated()
    {
        var (_, train, description) = Arrange();
        var calls = train.Calls.OrderBy(c => c.SortTime).ToList();
        var from = calls.First().OperationLocation;
        description.Destinations.Add(new Destination { Location = from, AndBeyond = true });
        var cargoFlow = train.CreateCargoFlow(1, calls.First(), calls.Last(), description);

        Assert.HasCount(2, cargoFlow.StatedDestinations.ToList(), "\"And beyond\" says where the wagons go.");
    }

    [TestMethod]
    public void AFlowOnlyToItsFromStationHasNoNotes()
    {
        var (_, train, description) = Arrange();
        var calls = train.Calls.OrderBy(c => c.SortTime).ToList();
        description.Destinations.Clear();
        description.Destinations.Add(new Destination { Location = calls.First().OperationLocation });
        var cargoFlow = train.CreateCargoFlow(1, calls.First(), calls.Last(), description);

        Assert.IsFalse(cargoFlow.HasStatedDestinations);
        Assert.IsEmpty(cargoFlow.DepartureNotes.ToList(), "An empty \"brings wagons to\" note says nothing.");
        Assert.IsEmpty(cargoFlow.ArrivalNotes.ToList());
    }
}
