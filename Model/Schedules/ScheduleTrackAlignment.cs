namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Puts a working's arrival tracks where the vehicle's next train departs from, for the workings where
/// the vehicle cannot get to another track on its own.
/// </summary>
/// <remarks>
/// <para>
/// A locomotive is free of the train it brought in: it uncouples, runs light across the station and takes
/// over whatever stands on another track. Which track its next train leaves from is therefore no concern
/// of the track it arrived at. A double-directed unit — a trainset, or a locomotive working a reversible
/// train, the traction that <see cref="ScheduledObjectExtensions.get_ReversesWithoutRunaround(ScheduledObject)"/>
/// describes — is a different matter: it <em>is</em> the train, so it leaves from the very track it came in
/// on. Its next train must therefore depart from that track, and the automatic build says so by moving the
/// arrival rather than the departure: a departure track is where the planner put the train, and the arrival
/// is what the chaining has just decided.
/// </para>
/// <para>
/// The last arrival is aligned as well where the working closes on itself, so that the vehicle is left
/// standing where the first departure fetches it for the next session.
/// </para>
/// <para>
/// A working with a locomotive that runs round, or with no traction unit yet, keeps the tracks the planner
/// chose: its next train may well take other coaches, which is often settled only later in the planning.
/// The planner can still ask for such a working to be aligned (see
/// <see cref="ScheduleTrackAlignmentExtensions.AlignArrivalTracksWhateverTheTraction"/>).
/// </para>
/// </remarks>
public static class ScheduleTrackAlignmentExtensions
{
    extension(Schedule schedule)
    {
        /// <summary>
        /// True when the vehicles working this schedule leave a train on the track they brought it in on:
        /// every traction unit assigned to it reverses without a runaround (see
        /// <see cref="ScheduledObjectExtensions.get_ReversesWithoutRunaround(ScheduledObject)"/>), and at
        /// least one is assigned.
        /// </summary>
        /// <remarks>
        /// A schedule no traction unit works yet — a working the build has just created, above all — is
        /// not one of these: what traction it gets is still open, and until it is said the tracks the
        /// planner chose are left alone. Assigning the vehicle is what settles it, and that is where the
        /// tracks are aligned (see <see cref="ScheduleEditingExtensions.AssignVehicle"/>).
        /// </remarks>
        public bool IsWorkedWithoutRunaround
        {
            get
            {
                var traction = schedule.Vehicles.Where(vehicle => vehicle.IsTraction).ToList();
                return traction.Count > 0 && traction.All(vehicle => vehicle.ReversesWithoutRunaround);
            }
        }

        /// <summary>
        /// Moves each arrival of the working onto the track the next train departs from, and the last
        /// arrival onto the first train's departure track where the working ends where it began. Does
        /// nothing to a working whose traction can run round to another track
        /// (see <see cref="ScheduleTrackAlignmentExtensions.get_IsWorkedWithoutRunaround(Schedule)"/>).
        /// </summary>
        /// <remarks>
        /// A joint where the vehicle does not stand still at one location — the working is broken there,
        /// as it is while an out-and-back trip is only half worked in — is passed over; there is no track
        /// to agree on. The move is made whatever else stands on the track at the time: it states what the
        /// vehicle does, and a track taken twice over is reported by the track-occupancy validation for the
        /// planner to resolve.
        /// </remarks>
        /// <returns>The number of arrivals moved to another track.</returns>
        public int AlignArrivalTracks()
        {
            schedule = schedule.ValueOrException(nameof(schedule));
            return schedule.IsWorkedWithoutRunaround ? schedule.AlignArrivalTracksWhateverTheTraction() : 0;
        }

        /// <summary>
        /// Moves each arrival of the working onto the track the next train departs from, and the last
        /// arrival onto the first train's departure track where the working ends where it began, whatever
        /// traction works it — a locomotive that could run round, or no vehicle at all yet.
        /// </summary>
        /// <remarks>
        /// This is what the planner asks for on a working afterwards, once it is known that the vehicle stays
        /// with its train. Broken joints are passed over and occupied tracks are not avoided, as in
        /// <see cref="ScheduleTrackAlignmentExtensions.AlignArrivalTracks"/>.
        /// </remarks>
        /// <returns>The number of arrivals moved to another track.</returns>
        public int AlignArrivalTracksWhateverTheTraction()
        {
            schedule = schedule.ValueOrException(nameof(schedule));
            var parts = schedule.OrderedParts;
            if (parts.Count == 0) return 0;
            var moved = 0;
            for (var i = 0; i < parts.Count - 1; i++) moved += ArriveWhereNextDeparts(parts[i], parts[i + 1]);
            return moved + ArriveWhereNextDeparts(parts[^1], parts[0]);
        }
    }

    // Puts the arriving part's last call on the track the departing part leaves from, where both are at the
    // same location and it is not there already. The per-track call index is the track's own (see
    // Timetable.RebuildStationCalls), so the call has to be taken off the one track and put on the other.
    private static int ArriveWhereNextDeparts(ScheduledTrainPart arriving, ScheduledTrainPart departing)
    {
        var call = arriving.To;
        var track = departing.From.Track;
        if (!call.OperationLocation.Equals(track.Station)) return 0;
        if (ReferenceEquals(call.Track, track)) return 0;
        call.Track.Calls.Remove(call);
        call.Track = track;
        call.TrackId = track.Id;
        track.Add(call);
        return 1;
    }
}
