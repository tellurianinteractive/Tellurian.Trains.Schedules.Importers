using System.Net;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the rows of a duty page's cargo block: ordered by the station the wagons are coupled at and
/// then by position in the train, with the flows coupled at one station into one position merged into
/// a single row — places first, regions last, and the limits summed.
/// </summary>
[TestClass]
public class TrainPartCargoRowTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    private sealed record Fixture(Timetable Timetable, Train Train, IReadOnlyList<Station> Stations, DriverDutyPart DutyPart);

    // A train calling at Alpha, Beta, Gamma and Delta, worked whole by one duty. Delta has a region.
    private static Fixture CreateFixture()
    {
        var layout = new Layout { Name = "Test" };
        string[] names = ["Alpha", "Beta", "Gamma", "Delta"];
        var stations = new List<Station>();
        for (var i = 0; i < names.Length; i++)
        {
            var station = new Station(i + 1, names[i], names[i][..2]);
            station.Add(new StationTrack((i + 1) * 10, "1"));
            layout.Add(station);
            stations.Add(station);
        }
        for (var i = 1; i < stations.Count; i++) layout.Add(new TrackStretch(i, stations[i - 1], stations[i], 10));
        stations[3].Regions.Add(new Region { Id = 1, Name = "North" });

        var timetable = new Timetable("Test", layout);
        var train = new Train(1, 100) { Sessions = Sessions.All };
        var start = Time.FromHourAndMinute(6, 00);
        for (var c = 0; c < stations.Count; c++)
        {
            var call = train.Add(new StationCall(c + 1, stations[c]["1"], start.AddMinutes(c * 10), start.AddMinutes(c * 10 + 2)));
            call.IsArrival = true;
            call.IsDeparture = true;
        }
        timetable.Add(train);

        var plan = Plan.Create("Test", timetable);
        var duty = new DriverDuty(1, "1") { Plan = plan, Sessions = Sessions.All };
        var dutyPart = new DriverDutyPart { TrainPart = train.AsTrainPart, Duty = duty, SessionsSettings = Settings };
        return new Fixture(timetable, train, stations, dutyPart);
    }

    private static CargoFlowTrainPart AddFlow(Fixture fixture, int fromIndex, int position, params Destination[] destinations)
    {
        var options = fixture.Timetable.Add(new CargoFlowOptions());
        foreach (var destination in destinations) options.Destinations.Add(destination);
        var calls = fixture.Train.CallsInRunOrder;
        var id = fixture.Train.CargoFlows.Count + 1;
        return fixture.Train.CreateCargoFlow(id, calls[fromIndex], calls[^1], options, position);
    }

    private static Destination To(Station station, int wagons = 0, bool andRegions = false) =>
        new() { Location = station, MaxNumberOfWagons = wagons, AndRegions = andRegions };

    private static string ToCell(TrainPartCargoFlow row) => WebUtility.HtmlDecode(row.To("also shunt").Value);

    [TestMethod]
    public void RowsAreOrderedByCoupleStationThenByPosition()
    {
        var fixture = CreateFixture();
        var (beta, gamma, delta) = (fixture.Stations[1], fixture.Stations[2], fixture.Stations[3]);
        var atBeta = AddFlow(fixture, 1, 1, To(gamma));
        var alphaSecond = AddFlow(fixture, 0, 2, To(delta));
        var alphaFirst = AddFlow(fixture, 0, 1, To(beta));

        var rows = fixture.DutyPart.CargoData.Flows;

        Assert.HasCount(3, rows);
        Assert.AreSame(alphaFirst, rows[0].Flows.Single());
        Assert.AreSame(alphaSecond, rows[1].Flows.Single());
        Assert.AreSame(atBeta, rows[2].Flows.Single());
    }

    [TestMethod]
    public void FlowsFromOneStationAtOnePositionAreMergedWithRegionsLast()
    {
        var fixture = CreateFixture();
        var (gamma, delta) = (fixture.Stations[2], fixture.Stations[3]);
        AddFlow(fixture, 1, 1, To(delta, andRegions: true));
        AddFlow(fixture, 1, 1, To(gamma));
        AddFlow(fixture, 1, 2, To(gamma));

        var rows = fixture.DutyPart.CargoData.Flows;

        Assert.HasCount(2, rows);
        Assert.HasCount(2, rows[0].Flows);
        var to = ToCell(rows[0]);
        Assert.IsLessThan(to.IndexOf("Gamma"), to.IndexOf("Delta"), "The places keep the order the flows name them.");
        Assert.IsGreaterThan(to.IndexOf("Gamma"), to.IndexOf("North"), "The regions follow all the places.");
    }

    [TestMethod]
    public void MergedFlowsStateOneSummedLimit()
    {
        var fixture = CreateFixture();
        var (gamma, delta) = (fixture.Stations[2], fixture.Stations[3]);
        AddFlow(fixture, 1, 1, To(gamma, wagons: 4));
        AddFlow(fixture, 1, 1, To(delta, wagons: 3));

        var limit = fixture.DutyPart.CargoData.Flows.Single().MaxLoads.Single();

        Assert.IsNull(limit.Location, "A summed limit is the whole row's and is not named.");
        Assert.AreEqual(7, limit.Capacity.Wagons);
    }

    [TestMethod]
    public void MergedFlowsWithAnUnlimitedDestinationStateNoLimit()
    {
        var fixture = CreateFixture();
        var (gamma, delta) = (fixture.Stations[2], fixture.Stations[3]);
        AddFlow(fixture, 1, 1, To(gamma, wagons: 4));
        AddFlow(fixture, 1, 1, To(delta));

        Assert.IsEmpty(fixture.DutyPart.CargoData.Flows.Single().MaxLoads);
    }
}
