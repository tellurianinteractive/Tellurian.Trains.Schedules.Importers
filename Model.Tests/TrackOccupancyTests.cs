using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers how long a station track is occupied, which is longer than the calling trains say on their
/// own: a traction unit that arrives with one train and leaves with the next stands on the track for the
/// whole time between — unless it is booked to or from parking, when each train occupies only its own
/// window again.
/// </summary>
[TestClass]
public class TrackOccupancyTests
{
    private static readonly TrainCategory Category = new() { Id = 1, Name = "P", Prefix = "P" };

    private StationTrack Track = default!;
    private StationTrack OtherTrack = default!;
    private Train Arriving = default!;
    private Train Leaving = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        TestDataFactory.Init();
        var station = TestDataFactory.Stations.First() as Station;
        Track = station!.Tracks.First();
        OtherTrack = station.Tracks.Last();

        // One train ends here at 12:00, the next leaves at 13:00 — an hour in which no train's own
        // window covers the track, but a unit staying on for the second train is standing on it.
        Arriving = new Train(1, Category, 1) { Category = Category };
        _ = Arriving.Add(new StationCall(1, OtherTrack, Time.FromHourAndMinute(11, 00), Time.FromHourAndMinute(11, 00)));
        _ = Arriving.Add(new StationCall(2, Track, Time.FromHourAndMinute(11, 50), Time.FromHourAndMinute(12, 00)));

        Leaving = new Train(2, Category, 2) { Category = Category };
        _ = Leaving.Add(new StationCall(1, Track, Time.FromHourAndMinute(12, 50), Time.FromHourAndMinute(13, 00)));
        _ = Leaving.Add(new StationCall(2, OtherTrack, Time.FromHourAndMinute(13, 50), Time.FromHourAndMinute(14, 00)));
    }

    private StationCall ArrivalCall => Arriving.Calls[^1];

    // A vehicle schedule working the arriving train and then the leaving one.
    private Schedule Continuing(bool toParking = false, bool fromParking = false)
    {
        var schedule = new Schedule(1);
        var first = Arriving.AsTrainPart(0, 1);
        var second = Leaving.AsTrainPart(0, 1);
        first.TractionOptions = new TractionOptions { ToParking = toParking };
        second.TractionOptions = new TractionOptions { FromParking = fromParking };
        schedule.Add(first);
        schedule.Add(second);
        return schedule;
    }

    [TestMethod]
    public void WithoutVehicleSchedulesACallOccupiesOnlyItsOwnWindow()
    {
        var (from, to) = ArrivalCall.TrackOccupancy(null);

        // The whole call window, ends included: at a terminus the departure time is the driver standing
        // down, and the train is standing on the track for it.
        Assert.AreEqual(Time.FromHourAndMinute(11, 50), from);
        Assert.AreEqual(Time.FromHourAndMinute(12, 00), to);
    }

    [TestMethod]
    public void AUnitStayingOnForTheNextTrainKeepsTheTrackOccupied()
    {
        var (from, to) = ArrivalCall.TrackOccupancy([Continuing()]);

        // Up to where the next train's own window starts, so the two spans are contiguous and together
        // cover the unit's whole stay without either claiming the other's time.
        Assert.AreEqual(Time.FromHourAndMinute(11, 50), from);
        Assert.AreEqual(Time.FromHourAndMinute(12, 50), to);
    }

    [TestMethod]
    public void AUnitBookedToParkingOccupiesOnlyTheTrainsOwnWindow()
    {
        var (_, to) = ArrivalCall.TrackOccupancy([Continuing(toParking: true)]);

        Assert.AreEqual(Time.FromHourAndMinute(12, 00), to,
            "Booked away to parking, the unit is not standing on this track between the two trains.");
    }

    [TestMethod]
    public void AUnitPickedUpFromParkingOccupiesOnlyTheTrainsOwnWindow()
    {
        var (_, to) = ArrivalCall.TrackOccupancy([Continuing(fromParking: true)]);

        Assert.AreEqual(Time.FromHourAndMinute(12, 00), to,
            "Coming from parking, the unit was not on this track waiting for its next train.");
    }

    [TestMethod]
    public void TheExtensionIsOffUntilAskedForButSameVehicleCallsStillDoNotConflict()
    {
        var schedules = new[] { Continuing() };

        // Off by default: a plan that never records parking cannot tell "the unit stayed" from "nothing
        // was noted", so the stay is not asserted as occupancy.
        Assert.IsEmpty(Track.GetValidationErrors(schedules),
            "Without the extension the two trains only occupy their own windows, which do not overlap.");

        // ...but the schedules are still read, so the two calls are known to be the same vehicle rather
        // than two of them contending for the track.
        Assert.IsEmpty(Track.GetValidationErrors(schedules, extendOccupancyByVehicleStay: true),
            "The stay belongs to one vehicle working both trains, which is not a conflict with itself.");
    }

    [TestMethod]
    public void AUnitLeavingFromAnotherTrackDoesNotExtendThisOne()
    {
        // The second train now leaves from the other track, so the unit moved between the two and
        // nothing says when. The gap is left unclaimed rather than charged to a track it may have left.
        Leaving.Calls[0].Track = OtherTrack;

        var (_, to) = ArrivalCall.TrackOccupancy([Continuing()]);

        Assert.AreEqual(Time.FromHourAndMinute(12, 00), to);
    }

    [TestMethod]
    public void AConflictFromTheExtendedStayReportsTheOccupiedSpanNotEitherCallsOwnTimes()
    {
        // Calls in the gap between the two trains (12:00-12:50) - it never overlaps either train's own
        // window, only the unit's extended stay between them.
        var third = new Train(3, Category, 3) { Category = Category };
        _ = third.Add(new StationCall(1, OtherTrack, Time.FromHourAndMinute(12, 05), Time.FromHourAndMinute(12, 05)));
        _ = third.Add(new StationCall(2, Track, Time.FromHourAndMinute(12, 10), Time.FromHourAndMinute(12, 20)));

        var errors = Track.GetValidationErrors([Continuing()], extendOccupancyByVehicleStay: true).ToList();

        Assert.HasCount(1, errors);
        var text = errors[0].Message.Text;
        // The message must show the span that actually overlaps (the extended stay to 12:50), not the
        // arriving call's own arr/dep, which the third train's 12:10-12:20 call does not overlap at all.
        StringAssert.Contains(text, "12:50");
        Assert.IsFalse(text.Contains("dep 12:00"), $"Message should not show the arriving call's own departure: {text}");
    }
}
