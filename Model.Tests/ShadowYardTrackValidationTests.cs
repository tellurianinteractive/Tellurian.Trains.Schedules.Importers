using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers <see cref="ValidationSettings.ExcludeShadowYardsFromStationTracks"/>: the tracks of shadow yards can be
/// left out of the station track validation, while the tracks of other stations are still checked.
/// </summary>
[TestClass]
public class ShadowYardTrackValidationTests
{
    private static readonly TrainCategory Freight = new() { Id = 1, Name = "Freight", Prefix = "G", Content = TrainContent.Cargo };

    // Two trains from Kode to the destination, both standing on its track 1 from 09:00 to 09:30. At Kode
    // they leave from track 1 at overlapping times too, unless kept apart.
    private static Plan CreatePlan(bool destinationIsShadow, bool conflictAtKode = false)
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var timetable = new Timetable("Test", layout);
        timetable.TrainCategories.Add(Freight);
        var kode = new Station(1, "Kode", "Kd");
        kode.Add(new StationTrack(1, "1"));
        kode.Add(new StationTrack(2, "2"));
        var destination = new Station(2, "Mohult", "Moh") { IsShadow = destinationIsShadow };
        destination.Add(new StationTrack(3, "1"));
        layout.OperationLocations.Add(kode);
        layout.OperationLocations.Add(destination);
        for (var n = 0; n < 2; n++)
        {
            var train = new Train(n + 1, Freight, 100 + n) { Sessions = Sessions.All };
            var departureTrack = conflictAtKode ? kode.Tracks[0] : kode.Tracks[n];
            var departure = new StationCall(n * 2 + 1, departureTrack, Time.FromHourAndMinute(8, 30 + n), Time.FromHourAndMinute(8, 40 + n)) { IsArrival = false };
            var arrival = new StationCall(n * 2 + 2, destination.Tracks[0], Time.FromHourAndMinute(9, 0 + n), Time.FromHourAndMinute(9, 30)) { IsDeparture = false };
            train.Add(departure);
            train.Add(arrival);
            timetable.Add(train);
        }
        return new Plan("Test", timetable);
    }

    private static List<ValidationError> TrackConflicts(Plan plan, bool excludeShadowYards) =>
        [.. plan.GetTimetableValidationErrors(new ValidationSettings { ExcludeShadowYardsFromStationTracks = excludeShadowYards })
            .Where(e => e.ErrorType == ValidationErrorType.StationTrackConflict)];

    [TestMethod]
    public void AShadowYardIsCheckedByDefault() =>
        Assert.IsNotEmpty(TrackConflicts(CreatePlan(destinationIsShadow: true), excludeShadowYards: false));

    [TestMethod]
    public void AShadowYardIsNotCheckedWhenExcluded() =>
        Assert.IsEmpty(TrackConflicts(CreatePlan(destinationIsShadow: true), excludeShadowYards: true));

    [TestMethod]
    public void AnOrdinaryStationIsCheckedWhenShadowYardsAreExcluded() =>
        Assert.IsNotEmpty(TrackConflicts(CreatePlan(destinationIsShadow: false), excludeShadowYards: true));

    [TestMethod]
    public void OtherStationsAreStillCheckedWhenShadowYardsAreExcluded()
    {
        var conflicts = TrackConflicts(CreatePlan(destinationIsShadow: true, conflictAtKode: true), excludeShadowYards: true);

        Assert.IsNotEmpty(conflicts);
        Assert.IsTrue(conflicts.All(c => c.Message.Text.Contains("Kode", StringComparison.Ordinal) || c.Message.Text.Contains("Kd", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ExcludingShadowYardsIsOffByDefault() =>
        Assert.IsFalse(new ValidationSettings().ExcludeShadowYardsFromStationTracks);
}
