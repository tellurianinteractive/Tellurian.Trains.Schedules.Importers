using Tellurian.Trains.Schedules.Model.Schedules;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers how long a station track is occupied, which is longer than the calling trains say on their
/// own: a traction unit that arrives with one train and leaves with the next stands on the track for the
/// whole time between — unless it is booked to or from parking, or put on or fetched from another track,
/// when each train occupies only its own window again.
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
    private Schedule Continuing(TractionLayover toLayover = TractionLayover.None, TractionLayover fromLayover = TractionLayover.None, StationTrack? toTrack = null, StationTrack? fromTrack = null)
    {
        var schedule = new Schedule(1);
        var first = Arriving.AsTrainPart(0, 1);
        var second = Leaving.AsTrainPart(0, 1);
        first.TractionOptions = new TractionOptions { ToLayover = toLayover };
        second.TractionOptions = new TractionOptions { FromLayover = fromLayover };
        first.ToTrack = toTrack;
        second.FromTrack = fromTrack;
        schedule.Add(first);
        schedule.Add(second);
        return schedule;
    }

    private static Time At(int hour, int minute) => Time.FromHourAndMinute(hour, minute);

    // The holds on a track, in time order.
    private static List<TrackHold> HoldsOn(Schedule schedule, StationTrack track) =>
        [.. schedule.TrackHolds.Where(h => h.Track.Equals(track)).OrderBy(h => h.From.Value)];

    // The stay between the two trains: the hold that begins at the given time.
    private static TrackHold? StayOn(Schedule schedule, StationTrack track, Time from) =>
        HoldsOn(schedule, track).FirstOrDefault(h => h.From == from);

    [TestMethod]
    public void ACallOccupiesItsOwnWindowAtTheTrackItself()
    {
        var (from, to) = ArrivalCall.TrackOccupancy;

        // The whole call window, ends included: at a terminus the departure time is the driver standing
        // down, and the train is standing on the track for it.
        Assert.AreEqual(At(11, 50), from);
        Assert.AreEqual(At(12, 00), to);
    }

    [TestMethod]
    public void AUnitStayingOnForTheNextTrainHoldsTheTrackBetween()
    {
        var hold = StayOn(Continuing(), Track, At(12, 00));

        // From where the arriving train window ends to where the next train own window starts, so the
        // spans are contiguous and together cover the whole stay.
        Assert.IsNotNull(hold);
        Assert.AreEqual(At(12, 50), hold.To);
    }

    [TestMethod]
    public void AUnitBookedToStablingHoldsNoTrack()
    {
        Assert.IsNull(StayOn(Continuing(toLayover: TractionLayover.Stabling), Track, At(12, 00)),
            "Booked away to stabling, the unit is not standing on this track between the two trains.");
    }

    [TestMethod]
    public void AUnitPickedUpFromStablingHoldsNoTrack()
    {
        Assert.IsNull(StayOn(Continuing(fromLayover: TractionLayover.Stabling), Track, At(12, 00)),
            "Coming from stabling, the unit was not on this track waiting for its next train.");
    }

    [TestMethod]
    public void AUnitLiftedOffHoldsNoTrackBetweenItsTrains()
    {
        var schedule = Continuing(toLayover: TractionLayover.LiftedOff, fromLayover: TractionLayover.LiftedOff);

        Assert.IsNull(StayOn(schedule, Track, At(12, 00)));
        Assert.HasCount(1, schedule.TrackHolds, "Only the night between the last train and the first train of the next day remains.");
    }

    [TestMethod]
    public void AUnitLeavingFromAnotherTrackStillHoldsTheArrivalTrackUntilItDeparts()
    {
        // Nothing says the unit was moved, so it stands where it arrived until it departs.
        Leaving.Calls[0].Track = OtherTrack;

        var hold = StayOn(Continuing(), Track, At(12, 00));

        Assert.IsNotNull(hold);
        Assert.AreEqual(At(13, 00), hold.To);
    }

    [TestMethod]
    public void AUnitPutOnAnotherTrackHoldsThatTrackFromArrivalToDeparture()
    {
        var schedule = Continuing(toTrack: OtherTrack);

        var hold = StayOn(schedule, OtherTrack, At(11, 50));

        Assert.IsNotNull(hold, "The track it is put on is taken from the arrival.");
        Assert.AreEqual(At(13, 00), hold.To, "Until the next train departs.");
        Assert.IsNull(StayOn(schedule, Track, At(12, 00)), "The arrival track is held by the train own window only.");
    }

    [TestMethod]
    public void AUnitFetchedFromAnotherTrackHoldsThatTrackUntilDeparture()
    {
        var schedule = Continuing(toTrack: OtherTrack, fromTrack: OtherTrack);

        var hold = StayOn(schedule, OtherTrack, At(11, 50));

        Assert.IsNotNull(hold);
        Assert.AreEqual(At(13, 00), hold.To);
    }

    [TestMethod]
    public void ALastArrivalHoldsItsTrackThroughTheNightUntilTheFirstDepartureOfTheNextDay()
    {
        var holds = HoldsOn(Continuing(), OtherTrack);

        // One stay, not the end of one day plus the start of the next.
        Assert.HasCount(1, holds);
        Assert.AreEqual(At(14, 00), holds[0].From);
        Assert.AreEqual(Time.FromDayHourMinute(1, 11, 0), holds[0].To);
    }

    [TestMethod]
    public void ANightStayAfterAnArrivalPastMidnightIsOnlyTheTimeUntilTheFirstDeparture()
    {
        // Arrives 01:07 (stored on the day after) and leaves 03:50 the same clock day: under three hours,
        // not the rest of the day plus the start of the day.
        var arriving = new Train(3, Category, 3) { Category = Category };
        _ = arriving.Add(new StationCall(1, OtherTrack, At(23, 00), At(23, 10)));
        _ = arriving.Add(new StationCall(2, Track, Time.FromDayHourMinute(1, 0, 47), Time.FromDayHourMinute(1, 1, 7)));
        var leaving = new Train(4, Category, 4) { Category = Category };
        _ = leaving.Add(new StationCall(1, Track, At(3, 40), At(3, 50)));
        _ = leaving.Add(new StationCall(2, OtherTrack, At(5, 40), At(6, 0)));
        var schedule = new Schedule(2);
        schedule.Add(arriving.AsTrainPart(0, 1));
        schedule.Add(leaving.AsTrainPart(0, 1));

        var hold = HoldsOn(schedule, Track).Last();

        Assert.AreEqual(Time.FromDayHourMinute(1, 1, 7), hold.From);
        Assert.AreEqual(Time.FromDayHourMinute(1, 3, 40), hold.To);
        Assert.IsNull(HoldsOn(schedule, Track).FirstOrDefault(h => h.From == Time.Zero), "No separate start-of-day hold.");
    }

    [TestMethod]
    public void AUnitStabledAtTheEndOrFetchedFromStablingAtTheStartHoldsNoDayEnd()
    {
        var schedule = Continuing();
        schedule.OrderedParts[^1].TractionOptions = new TractionOptions { ToLayover = TractionLayover.Stabling };
        schedule.OrderedParts[0].TractionOptions = new TractionOptions { FromLayover = TractionLayover.Stabling };

        Assert.IsEmpty(HoldsOn(schedule, OtherTrack));
    }

    [TestMethod]
    public void TheExtensionIsOffUntilAskedForButSameVehicleCallsStillDoNotConflict()
    {
        var schedules = new[] { Continuing() };

        // Off by default: a plan that never records stabling cannot tell "the unit stayed" from "nothing
        // was noted", so the stay is not asserted as occupancy.
        Assert.IsEmpty(Track.GetValidationErrors(schedules),
            "Without the extension the two trains only occupy their own windows, which do not overlap.");

        // ...but the schedules are still read, so the two calls are known to be the same vehicle rather
        // than two of them contending for the track.
        Assert.IsEmpty(Track.GetValidationErrors(schedules, extendOccupancyByVehicleStay: true),
            "The stay belongs to one vehicle working both trains, which is not a conflict with itself.");
    }

    [TestMethod]
    public void AConflictWithAStandingUnitReportsTheSpanItStandsFor()
    {
        // A call in the gap between the two trains (12:00-12:50) - it never overlaps either train own
        // window, only the unit stay between them.
        var third = new Train(3, Category, 3) { Category = Category };
        _ = third.Add(new StationCall(1, OtherTrack, At(12, 05), At(12, 05)));
        _ = third.Add(new StationCall(2, Track, At(12, 10), At(12, 20)));

        var errors = Track.GetValidationErrors([Continuing()], extendOccupancyByVehicleStay: true).ToList();

        Assert.HasCount(1, errors);
        StringAssert.Contains(errors[0].Message.Text, "12:00-12:50", "The message shows the span the unit stands for.");
    }

    [TestMethod]
    public void ATrainTakingTheTrackWhereAUnitStandsForTheRestOfTheDayConflicts()
    {
        var late = new Train(3, Category, 3) { Category = Category };
        _ = late.Add(new StationCall(1, Track, At(20, 00), At(20, 10)));
        _ = late.Add(new StationCall(2, OtherTrack, At(20, 40), At(20, 50)));

        var errors = OtherTrack.GetValidationErrors([Continuing()], extendOccupancyByVehicleStay: true).ToList();

        Assert.HasCount(1, errors, "The unit that arrived at 14:00 is still on the track at 20:40.");
        Assert.IsEmpty(OtherTrack.GetValidationErrors([Continuing()]), "Not without the extension.");
    }

    // A train taking the track shortly after the arriving one has left it: no overlap at all, so it is a
    // conflict only where the track is required to stand free in between.
    private Train FollowingAfter(int minutes)
    {
        var following = new Train(4, Category, 4) { Category = Category };
        var from = Time.FromHourAndMinute(12, 00).AddMinutes(minutes);
        _ = following.Add(new StationCall(1, Track, from, from.AddMinutes(10)));
        _ = following.Add(new StationCall(2, OtherTrack, Time.FromHourAndMinute(14, 50), Time.FromHourAndMinute(15, 00)));
        return following;
    }

    [TestMethod]
    public void WithoutARequiredGapTrainsFollowingCloselyDoNotConflict()
    {
        _ = FollowingAfter(1);

        Assert.IsEmpty(Track.GetValidationErrors([]),
            "At zero the rule is plain double booking, and a train taking the track a minute after the last one left is not that.");
    }

    [TestMethod]
    public void ATrainTakingTheTrackSoonerThanTheRequiredGapConflicts()
    {
        _ = FollowingAfter(3);

        // 17 minutes required: a number that appears in none of the times in play, so finding it in the
        // message says the required gap was named — whichever language the message came out in.
        var errors = Track.GetValidationErrors([], minMinutesBetweenTrackUsage: 17).ToList();

        Assert.HasCount(1, errors);
        StringAssert.Contains(errors[0].Message.Text, "17",
            "A conflict over a missing gap has to say how much free time the track was required to have.");
    }

    [TestMethod]
    public void ExactlyTheRequiredGapIsEnough()
    {
        _ = FollowingAfter(5);

        Assert.IsEmpty(Track.GetValidationErrors([], minMinutesBetweenTrackUsage: 5),
            "Five minutes free is what five minutes required asks for; only less than that is a conflict.");
    }

    [TestMethod]
    public void ARequiredGapDoesNotSeparateTheSameVehicleFromItself()
    {
        // The same unit arriving on one train and leaving on the next turns the track round in an hour,
        // but it never leaves it: one occupant, so no gap is owed.
        Assert.IsEmpty(Track.GetValidationErrors([Continuing()], minMinutesBetweenTrackUsage: 90),
            "A gap is time the track stands free between two occupants, not between one vehicle's own trains.");
    }

    [TestMethod]
    public void AnOverlapIsStillReportedAsAnOverlapWhenAGapIsRequired()
    {
        // Straight into the arriving train's own window, so the two are on the track together.
        _ = FollowingAfter(-5);

        var errors = Track.GetValidationErrors([], minMinutesBetweenTrackUsage: 17).ToList();

        Assert.HasCount(1, errors);
        // Only the missing-gap message names the required minutes, so its absence says this was reported
        // as the overlap it is rather than as two trains merely following each other too closely.
        Assert.IsFalse(errors[0].Message.Text.Contains("17", StringComparison.Ordinal),
            $"Overlapping trains overlap; they are not merely too close together: {errors[0].Message.Text}");
    }

    private static (Time From, Time To) Span(int day, int fromHour, int toHour) =>
        (Time.FromDayHourMinute(day, fromHour, 0), Time.FromDayHourMinute(day, toHour, 0));

    private static readonly Sessions Odd = Sessions.FromSessionNumbers(1, 3, 5);
    private static readonly Sessions Even = Sessions.FromSessionNumbers(2, 4, 6);

    [TestMethod]
    public void SpansOnTheSameClockTimeConflictWhenTheSessionsLineUpAcrossTheDay()
    {
        // 01:00-02:00 and the next day's 01:00-02:00 (25:00-26:00) are the same time on the clock. The
        // stored-late span is on the day after its session, so it meets the early span one session on.
        var early = Span(0, 1, 2);
        var late = Span(1, 1, 2);

        Assert.IsTrue(early.ConflictsInTime(Odd, late, Even, 0), "The 25:00 span on session 2 is 01:00 on session 3.");
        Assert.IsTrue(late.ConflictsInTime(Even, early, Odd, 0));
        Assert.IsTrue(early.ConflictsInTime(Odd, late, Sessions.All, 0));
    }

    [TestMethod]
    public void SpansADayApartDoNotConflictWhenBothTrainsRunTheSameSessions()
    {
        // 25:00 on sessions 1,3,5 is 01:00 on sessions 2,4,6, where the early train does not run.
        Assert.IsFalse(Span(0, 1, 2).ConflictsInTime(Odd, Span(1, 1, 2), Odd, 0));
        Assert.IsFalse(Span(1, 1, 2).ConflictsInTime(Odd, Span(0, 1, 2), Odd, 0));
    }

    [TestMethod]
    public void SpansOnDifferentClockTimesDoNotConflictAcrossDays()
    {
        Assert.IsFalse(Span(0, 1, 2).ConflictsInTime(Sessions.All, Span(1, 3, 4), Sessions.All, 0));
        Assert.IsFalse(Span(0, 1, 2).ConflictsInTime(Sessions.All, Span(1, 2, 3), Sessions.All, 0), "A handover is no conflict.");
    }

    [TestMethod]
    public void RequiredGapIsKeptAcrossDays()
    {
        Assert.IsTrue(Span(0, 1, 2).ConflictsInTime(Sessions.All, Span(1, 2, 3), Sessions.All, 10));
        Assert.AreEqual(60, Span(0, 1, 2).FreeMinutesBetween(Sessions.All, Span(1, 3, 4), Sessions.All));
        Assert.IsLessThan(0, Span(0, 1, 3).FreeMinutesBetween(Sessions.All, Span(1, 2, 4), Sessions.All));
    }
}
