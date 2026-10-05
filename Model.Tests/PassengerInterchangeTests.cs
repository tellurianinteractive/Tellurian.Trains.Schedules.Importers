using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers the passenger interchanges: which stations count as one, and the arrival note a passenger
/// train gets where it stops at one.
/// </summary>
/// <remarks>
/// Two things decide it — the meeting is worked with passenger tickets, and the station is marked —
/// so most of what is tested here is which half is missing and what happens then.
/// </remarks>
[TestClass]
public class PassengerInterchangeTests
{
    private static readonly TrainCategory Passenger = new() { Id = 1, Name = "Passenger", Prefix = "P", Content = TrainContent.Passenger };
    private static readonly TrainCategory Freight = new() { Id = 2, Name = "Freight", Prefix = "G", Content = TrainContent.Cargo };

    // Pin both cultures to invariant so the note text resolves the neutral (English) Notes resource,
    // independent of the host machine's culture and of the localised Notes.<culture>.resx files.
    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [TestMethod]
    public void AMarkedStationIsAnInterchangeOnlyWhileTicketsAreInUse()
    {
        var (layout, interchange, _) = Layout();

        Assert.IsTrue(interchange.ExchangesTransferringPassengers);
        CollectionAssert.AreEqual(new[] { interchange }, layout.PassengerInterchanges.ToArray());

        layout.Settings.General.UsePassengerTickets = false;

        // The mark is kept — the meeting may take the tickets up again — but nothing is derived from it.
        Assert.IsTrue(interchange.IsPassengerInterchange, "The mark is ignored, not thrown away.");
        Assert.IsFalse(interchange.ExchangesTransferringPassengers);
        Assert.IsEmpty(layout.PassengerInterchanges);
    }

    [TestMethod]
    public void AnUnmarkedStationIsNoInterchangeHoweverManyPassengersItTakes()
    {
        var (layout, _, plain) = Layout();

        Assert.IsTrue(plain.HasPassengerExchange);
        Assert.IsFalse(plain.ExchangesTransferringPassengers);
        Assert.DoesNotContain(plain, layout.PassengerInterchanges);
    }

    [TestMethod]
    public void SomewhereNobodyBoardsOrAlightsIsNowhereToChangeTrains()
    {
        var (layout, interchange, _) = Layout();
        interchange.HasPassengerExchange = false;

        Assert.IsFalse(interchange.CanBePassengerInterchange);
        Assert.IsFalse(interchange.ExchangesTransferringPassengers);
        Assert.IsEmpty(layout.PassengerInterchanges);
    }

    [TestMethod]
    public void OnlyAStationCanBeMarked()
    {
        var halt = new OtherLocation(9, "Hålan", "Hål");
        var quarry = new IndustrialArea(10, "Grustaget", "Gt");

        // Neither has anywhere to change to: a halt is served by one line and a quarry takes nobody.
        Assert.IsFalse(halt.CanBePassengerInterchange);
        Assert.IsFalse(quarry.CanBePassengerInterchange);
    }

    [TestMethod]
    public void AStationNotYetOnALayoutSaysNothingAboutTickets()
    {
        var loose = new Station(99, "Lösa", "Lö") { IsPassengerInterchange = true };

        // The setting lives on the layout, so until there is one there is nothing to say it is in use.
        Assert.IsFalse(loose.ExchangesTransferringPassengers);
    }

    [TestMethod]
    public void APassengerTrainStoppingAtAnInterchangeIsToldToHandThePassengersOver()
    {
        var (_, interchange, plain) = Layout();
        var train = PassengerTrain(
            Stop(1, plain, "08:00", "08:10"),
            Stop(2, interchange, "08:30", "08:40"));

        var note = train.Calls[1].PassengerInterchangeNotes.Single();

        Assert.IsInstanceOfType<ExchangeTransferringPassengersNote>(note);
        Assert.AreEqual("Exchange transferring passengers here.", note.ToText);
        // Handing them over is the first thing that happens once the train has pulled in.
        Assert.IsTrue(note.IsForArrival);
        Assert.IsFalse(note.IsForDeparture);
    }

    [TestMethod]
    public void NothingIsSaidAtTheOtherStationsOnTheRun()
    {
        var (_, interchange, plain) = Layout();
        var train = PassengerTrain(
            Stop(1, plain, "08:00", "08:10"),
            Stop(2, interchange, "08:30", "08:40"));

        Assert.IsEmpty(train.Calls[0].PassengerInterchangeNotes);
    }

    [TestMethod]
    public void ATrainRunningThroughHandsNobodyOver()
    {
        var (_, interchange, plain) = Layout();
        var train = PassengerTrain(
            Stop(1, plain, "08:00", "08:10"),
            PassThrough(2, interchange, "08:30"),
            Stop(3, plain, "09:00", "09:10"));

        Assert.IsEmpty(train.Calls[1].PassengerInterchangeNotes);
    }

    [TestMethod]
    public void AFreightTrainCarriesNobodyToHandOver()
    {
        var (_, interchange, plain) = Layout();
        var train = new Train(2, Freight, 5001) { Category = Freight };
        train.Add(Stop(1, plain, "08:00", "08:10"));
        train.Add(Stop(2, interchange, "08:30", "08:40"));

        Assert.IsEmpty(train.Calls[1].PassengerInterchangeNotes);
    }

    [TestMethod]
    public void NoteIsGoneOnceTheMeetingDropsTheTickets()
    {
        var (layout, interchange, plain) = Layout();
        var train = PassengerTrain(
            Stop(1, plain, "08:00", "08:10"),
            Stop(2, interchange, "08:30", "08:40"));
        Assert.IsNotEmpty(train.Calls[1].PassengerInterchangeNotes, "In force while the tickets are used.");

        layout.Settings.General.UsePassengerTickets = false;

        Assert.IsEmpty(train.Calls[1].PassengerInterchangeNotes);
    }

    [TestMethod]
    public void TheNoteIsOneOfTheDriversNotes()
    {
        var (_, interchange, plain) = Layout();
        var train = PassengerTrain(
            Stop(1, plain, "08:00", "08:10"),
            Stop(2, interchange, "08:30", "08:40"));

        var notes = train.Calls[1].DriverNotes(Sessions.All, SessionsSettings.UseSessions(14));

        Assert.IsInstanceOfType<ExchangeTransferringPassengersNote>(notes.Single());
    }

    // A layout worked with passenger tickets, one station marked as an interchange and one not.
    private static (Layout Layout, Station Interchange, Station Plain) Layout()
    {
        var layout = new Layout { Name = "Test" };
        layout.Settings.General.UsePassengerTickets = true;
        var plain = (Station)layout.Add(NewStation(1, "Alvesta", "Av"));
        var interchange = (Station)layout.Add(NewStation(2, "Nässjö", "Nä"));
        interchange.IsPassengerInterchange = true;
        return (layout, interchange, plain);
    }

    private static Station NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10 + 1, "1"));
        return station;
    }

    private static Train PassengerTrain(params StationCall[] calls)
    {
        var train = new Train(1, Passenger, 101) { Category = Passenger };
        foreach (var call in calls) train.Add(call);
        return train;
    }

    private static StationCall Stop(int id, OperationLocation at, string arrival, string departure) =>
        new(id, at["1"], Time.FromString(arrival), Time.FromString(departure)) { IsArrival = true, IsDeparture = true };

    private static StationCall PassThrough(int id, OperationLocation at, string time) =>
        new(id, at["1"], Time.FromString(time), Time.FromString(time));
}
