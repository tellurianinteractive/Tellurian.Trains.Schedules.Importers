using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Trains.Schedules.Planning.Components.Reporting;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers which passenger tickets a timetable yields and who is printed as selling them: a ticket exists
/// between every pair of locations that exchanges passengers, in both directions, and its seller is the
/// passenger operator departing most often from where it is sold.
/// </summary>
[TestClass]
public class PassengerTicketTests
{
    private const string Alvesta = "Alvesta";
    private const string Nassjo = "Nässjö";
    private const string Quarry = "Grustaget";

    // Returns the resource key itself, so a test can tell which text a ticket asked for without
    // depending on the wording of any one language.
    private static readonly Translator Translator = key => key ?? "";

    private static readonly Company Sj = new(1, "Statens Järnvägar", "SJ");
    private static readonly Company Dsb = new(2, "Danske Statsbaner", "DSB");
    private static readonly Company GreenCargo = new(3, "Green Cargo", "GC");

    private static readonly TrainCategory Passenger = new() { Id = 1, Name = "Passenger", Prefix = "P", Content = TrainContent.Passenger };
    private static readonly TrainCategory Freight = new() { Id = 2, Name = "Freight", Prefix = "G", Content = TrainContent.Cargo };

    private int _nextCallId;

    [TestMethod]
    public void TicketsRunBothWaysBetweenEveryPairOfLocationsThatTakesPassengers()
    {
        var timetable = CreateTimetable();

        var tickets = timetable.ToPassengerTickets();

        CollectionAssert.AreEquivalent(
            new[] { $"{Alvesta}-{Nassjo}", $"{Nassjo}-{Alvesta}" },
            tickets.Select(ticket => $"{ticket.Origin}-{ticket.Destination}").ToArray(),
            "Both directions are sold, each at its own origin, and nothing is sold to or from a location that takes no passengers.");
    }

    [TestMethod]
    public void NoTicketsWhereOnlyOneLocationTakesPassengers()
    {
        var timetable = CreateTimetable();
        LocationNamed(timetable, Nassjo).HasPassengerExchange = false;

        Assert.AreEqual(0, timetable.ToPassengerTickets().Count,
            "One location on its own has nowhere to travel to, so there is nothing to sell.");
    }

    [TestMethod]
    public void SellerIsThePassengerOperatorWithMostDeparturesFromTheOrigin()
    {
        var timetable = CreateTimetable();
        AddTrain(timetable, 1, Passenger, Sj, Alvesta, Nassjo, 8);
        AddTrain(timetable, 2, Passenger, Sj, Alvesta, Nassjo, 10);
        AddTrain(timetable, 3, Passenger, Dsb, Alvesta, Nassjo, 12);

        Assert.AreEqual(Sj, TicketFrom(timetable, Alvesta).SoldBy);
    }

    [TestMethod]
    public void FreightDeparturesDoNotDecideTheSeller()
    {
        var timetable = CreateTimetable();
        AddTrain(timetable, 1, Passenger, Sj, Alvesta, Nassjo, 8);
        AddTrain(timetable, 5001, Freight, GreenCargo, Alvesta, Nassjo, 9);
        AddTrain(timetable, 5003, Freight, GreenCargo, Alvesta, Nassjo, 11);
        AddTrain(timetable, 5005, Freight, GreenCargo, Alvesta, Nassjo, 13);

        Assert.AreEqual(Sj, TicketFrom(timetable, Alvesta).SoldBy,
            "It is a passenger ticket, so an operator that carries nobody cannot be the one selling it.");
    }

    [TestMethod]
    public void ArrivingPassengerTrainsDoNotMakeAnOperatorTheSeller()
    {
        var timetable = CreateTimetable();
        // DSB only terminates at Alvesta; SJ departs from there.
        AddTrain(timetable, 1, Passenger, Sj, Alvesta, Nassjo, 8);
        AddTrain(timetable, 2, Passenger, Dsb, Nassjo, Alvesta, 9);
        AddTrain(timetable, 4, Passenger, Dsb, Nassjo, Alvesta, 11);

        Assert.AreEqual(Sj, TicketFrom(timetable, Alvesta).SoldBy);
    }

    [TestMethod]
    public void TicketWithoutASellerStillSaysWhereItWasSold()
    {
        var timetable = CreateTimetable();
        AddTrain(timetable, 5001, Freight, GreenCargo, Alvesta, Nassjo, 9);

        var ticket = TicketFrom(timetable, Alvesta);

        Assert.IsNull(ticket.SoldBy, "No passenger train departs Alvesta, so no company sells there.");
        Assert.AreEqual("SoldAt", ticket.SoldByText(Translator));
    }

    [TestMethod]
    public void TicketWithASellerNamesTheCompanyAndTheLocation()
    {
        var timetable = CreateTimetable();
        AddTrain(timetable, 1, Passenger, Sj, Alvesta, Nassjo, 8);

        Assert.AreEqual("SoldByAt", TicketFrom(timetable, Alvesta).SoldByText(Translator));
    }

    [TestMethod]
    public void ValidityIsTheMeetingDays()
    {
        var timetable = CreateTimetable();
        var general = timetable.Layout.Settings.General;
        general.ValidFrom = new DateOnly(2026, 8, 14);
        general.ValidTo = new DateOnly(2026, 8, 16);

        var ticket = TicketFrom(timetable, Alvesta);

        Assert.AreEqual(general.ValidFrom, ticket.ValidFrom);
        Assert.AreEqual(general.ValidTo, ticket.ValidTo);
    }

    [TestMethod]
    public void ValidityIsOmittedWhenNoMeetingIsBooked()
    {
        var timetable = CreateTimetable();

        Assert.AreEqual("", TicketFrom(timetable, Alvesta).Validity(Translator),
            "A layout without meeting dates prints no validity line rather than a placeholder date.");
    }

    private static TicketData TicketFrom(Timetable timetable, string origin) =>
        timetable.ToPassengerTickets().First(ticket => ticket.Origin == origin);

    private static OperationLocation LocationNamed(Timetable timetable, string name) =>
        timetable.Layout.OperationLocations.First(location => location.Name == name);

    // Two stations that take passengers and a quarry that does not, so a location outside passenger
    // traffic can be shown to stay out of the tickets.
    private static Timetable CreateTimetable()
    {
        var layout = new Layout { Name = "Test" };
        layout.Add(Sj);
        layout.Add(Dsb);
        layout.Add(GreenCargo);
        var alvesta = layout.Add(NewStation(1, Alvesta, "Av"));
        var nassjo = layout.Add(NewStation(2, Nassjo, "Nä"));
        var quarry = layout.Add(NewStation(3, Quarry, "Gt"));
        quarry.HasPassengerExchange = false;
        layout.Add(new TrackStretch(1, alvesta, nassjo, 10));
        layout.Add(new TrackStretch(2, nassjo, quarry, 10));
        return new Timetable("Test", layout);
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature);
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    // A train running non-stop from one location to another, departing on the hour.
    private void AddTrain(Timetable timetable, int number, TrainCategory category, Company company, string from, string to, int departureHour)
    {
        var train = new Train(number, category, number) { Company = company };
        var departure = Time.FromHourAndMinute(departureHour, 0);
        train.Add(new StationCall(++_nextCallId, LocationNamed(timetable, from)["1"], departure.AddMinutes(-10), departure));
        var arrival = train.Add(new StationCall(++_nextCallId, LocationNamed(timetable, to)["1"], departure.AddMinutes(30), departure.AddMinutes(40)));
        arrival.IsArrival = true;
        timetable.Add(train);
    }
}
