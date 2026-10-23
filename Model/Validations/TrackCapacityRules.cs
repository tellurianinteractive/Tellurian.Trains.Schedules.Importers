using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Rule L5: what stands on a station track together has to fit it, and has to be able to leave it in the
/// order it is planned to.
/// </summary>
/// <remarks>
/// <para>
/// A track with a length can hold more than one train, when they are short enough: the trains' lengths (the
/// vehicles assigned to them, or failing that the metre limit of the train) are added up. This replaces
/// plain double booking (L2) for the occupants that have a length; an occupant without one is still judged
/// by L2, so a plan that records no lengths is checked exactly as before.
/// </para>
/// <para>
/// Vehicles standing on a track stand in a row, and a vehicle can leave only by an end of the track with
/// nothing between it and that end. A vehicle that comes in by an end stands nearer it than the vehicles
/// already there, so two things go wrong. A vehicle that arrives later at the end another is to leave by
/// stands in its way, and has to leave first. And a vehicle cannot leave by the end it did not come in by
/// while another stands nearer that end, which includes one that came in the other way. The two ends of a
/// track are told apart by the location the train comes from or goes on to; a stub track has the one end
/// only (<see cref="StationTrack.IsStub"/>). Where an end is not known — a train that begins or ends its
/// run here — the order is not judged.
/// </para>
/// </remarks>
internal static class TrackCapacityRules
{
    // Lengths are kept to one decimal, so anything within half of that is the same length.
    private const double Tolerance = 0.05;

    /// <summary>
    /// The capacity errors of a track: occupants that do not fit together, and occupants that cannot leave
    /// in the order planned.
    /// </summary>
    public static IEnumerable<ValidationError> GetCapacityErrors(StationTrack track, IReadOnlyList<TrackStanding> standings, IEnumerable<Schedule> schedules)
    {
        var measured = standings.Where(s => s.IsMeasured).ToList();
        if (measured.Count == 0) return [];
        return [.. TooShort(track, measured), .. Blocked(track, measured, schedules)];
    }

    private static IEnumerable<ValidationError> TooShort(StationTrack track, List<TrackStanding> measured)
    {
        foreach (var arriving in measured)
        {
            var together = measured.Where(o => ReferenceEquals(o, arriving) || IsThereWhenArriving(arriving, o)).ToList();
            var total = together.Sum(o => o.Length);
            // Reported once, at the arrival that tips it over: what was already too long before is reported at its own.
            if (total <= track.Length + Tolerance || total - arriving.Length > track.Length + Tolerance) continue;

            var message = Message.Information(Strings.StationTrackTooShortForStandingVehicles,
                track.Station, track, Metres(track.Length), string.Join(", ", together.Select(o => o.Name)), arriving.From.HHMM(), Metres(total));
            yield return ValidationError.StationTrackTooShort(track, together, message);
        }
    }

    private static IEnumerable<ValidationError> Blocked(StationTrack track, List<TrackStanding> measured, IEnumerable<Schedule> schedules)
    {
        for (var i = 0; i < measured.Count - 1; i++)
        {
            for (var j = i + 1; j < measured.Count; j++)
            {
                var (a, b) = (measured[i], measured[j]);
                if (IsOfSameVehicles(a, b, schedules)) continue;
                if (a.Span.OverlapDayShift(a.Sessions, b.Span, b.Sessions) is not { } shift) continue;

                // The one that arrives first is standing when the other comes in; the other is nearer the end it
                // came in by. Times past midnight are put on the day they belong to before they are compared.
                var bFrom = b.From.AddDays(shift);
                var bTo = b.To.AddDays(shift);
                var (first, firstFrom, firstTo, second, secondFrom, secondTo) =
                    a.From < bFrom ? (a, a.From, a.To, b, bFrom, bTo) : (b, bFrom, bTo, a, a.From, a.To);
                // Arriving together, neither is nearer an end than the other: nothing to say which is in the way.
                if (firstFrom == secondFrom) continue;

                // The first leaves by the end the second came in by, and the second is still there: in the way.
                if (first.Exit is { } exit && second.Entry == exit && secondTo > firstTo)
                    yield return Report(track, first, firstTo, second, secondTo);
                // The second leaves by the end it did not come in by, past the first, which is still there.
                else if (second.Entry is { } entry && second.Exit is { } leaves && leaves != entry && firstTo > secondTo)
                    yield return Report(track, second, secondTo, first, firstTo);
            }
        }
    }

    private static ValidationError Report(StationTrack track, TrackStanding blocked, Time blockedLeaves, TrackStanding blocker, Time blockerLeaves)
    {
        var message = Message.Information(Strings.StationTrackBlockedByStandingVehicles,
            track.Station, track, blocked.Name, blockedLeaves.HHMM(), blocker.Name, blockerLeaves.HHMM());
        return ValidationError.StationTrackBlocked(track, blocked, blocker, message);
    }

    // Whether the other is on the track at the moment this one arrives.
    private static bool IsThereWhenArriving(TrackStanding arriving, TrackStanding other) =>
        arriving.Span.OverlapDayShift(arriving.Sessions, other.Span, other.Sessions) is { } shift &&
        other.From.AddDays(shift) <= arriving.From && other.To.AddDays(shift) > arriving.From;

    // Vehicles that are together by design do not stand in each other's way: they come or go as one.
    private static bool IsOfSameVehicles(TrackStanding a, TrackStanding b, IEnumerable<Schedule> schedules) => (a, b) switch
    {
        ({ Call: { } one }, { Call: { } other }) => schedules.HasSameVehicle(one, other),
        ({ Hold: { } hold }, { Hold: { } other }) => hold.IsOfSameVehiclesAs(other) || hold.SharesTrainWith(other),
        ({ Hold: { } hold }, { Call: { } call }) => IsOfSameVehicles(hold, call, schedules),
        ({ Call: { } call }, { Hold: { } hold }) => IsOfSameVehicles(hold, call, schedules),
        _ => false,
    };

    // A call of the schedule whose vehicles stand there is theirs, and so is the call of a locomotive that
    // comes to fetch them.
    private static bool IsOfSameVehicles(TrackHold hold, StationCall call, IEnumerable<Schedule> schedules) =>
        hold.Schedule.Parts.Any(p => p.ContainsCall(call)) || hold.IsCollectedBy(call.Train!, schedules);

    private static string Metres(double metres) => metres.ToString("0.#", CultureInfo.CurrentCulture);
}
