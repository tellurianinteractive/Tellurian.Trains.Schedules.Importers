using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Schedules;

namespace Tellurian.Trains.Schedules.Model.Dispatching;

/// <summary>
/// The clearances dispatchers give trains: the rules behind a station's dispatch list.
/// </summary>
public static class ClearanceExtensions
{
    extension(Train train)
    {
        /// <summary>
        /// The clearances the train needs at <paramref name="location"/>, in run order.
        /// </summary>
        /// <remarks>
        /// <para>
        /// How many clearances a call needs is decided by the <em>times</em>, not by whether the train stops: a
        /// train standing at the platform needs two whether or not anyone gets on. A call whose arrival equals its
        /// departure is a single moment and needs one; the train's first call has no arrival to clear and its last
        /// no departure. A shunting task needs one, at the time the work starts.
        /// </para>
        /// <para>
        /// Trains running past are included, regardless of the location's <c>HidePassings</c> setting: a
        /// dispatcher clears every train through their station.
        /// </para>
        /// </remarks>
        /// <param name="location">The location the clearances are given at.</param>
        /// <param name="settings">The operating period.</param>
        /// <param name="plan">The plan, whose driver duties decide which trains run in the first session.
        /// Omit it and only the train's own start does.</param>
        /// <param name="isRemote">Whether <paramref name="location"/> is worked from afar by the dispatcher.</param>
        public IEnumerable<Clearance> ClearancesAt(OperationLocation location, SessionsSettings settings, Plan? plan = null, bool isRemote = false)
        {
            location = location.ValueOrException(nameof(location));
            var startsBeforeFirstSession = StartsBeforeFirstSession(train, plan);
            return train.ClearancesAt(location, settings, startsBeforeFirstSession, isRemote);
        }

        private IEnumerable<Clearance> ClearancesAt(OperationLocation location, SessionsSettings settings, bool startsBeforeFirstSession, bool isRemote)
        {
            var calls = train.CallsInRunOrder;
            for (var i = 0; i < calls.Count; i++)
            {
                var call = calls[i];
                if (!call.OperationLocation.Equals(location)) continue;

                if (call == train.ShuntingCall)
                    yield return Create(call, ClearanceKind.ShuntingTask, call.Arrival, sole: true);
                else if (i == 0)
                    yield return Create(call, ClearanceKind.Departure, call.Departure, sole: true);
                else if (i == calls.Count - 1)
                    yield return Create(call, ClearanceKind.Arrival, call.Arrival, sole: true);
                else if (call.Arrival.Equals(call.Departure))
                    yield return Create(call, ClearanceKind.PassThrough, call.Departure, sole: true);
                else
                {
                    yield return Create(call, ClearanceKind.Arrival, call.Arrival, sole: false);
                    yield return Create(call, ClearanceKind.Departure, call.Departure, sole: false);
                }
            }

            Clearance Create(StationCall call, ClearanceKind kind, Time time, bool sole) =>
                new(call, kind, time, train.Sessions.ShiftedWithinPeriod(time.Value.Days, settings.UseDaysInsteadOfSessionNumbers, settings.MaxNumberOfSessions))
                {
                    IsSole = sole,
                    IsRemote = isRemote,
                    // What lies before the meeting begins was never worked on the first session, and a clearance
                    // after midnight is reached on the first session only by the period wrapping round.
                    IsOutsideFirstSession = location.Layout?.Settings.General.FirstSessionStart > TimeSpan.Zero &&
                        (time.Value.Days > 0 || startsBeforeFirstSession),
                };
        }
    }

    extension(OperationLocation station)
    {
        /// <summary>
        /// Every clearance the dispatcher at <paramref name="station"/> gives over the whole operating day, in time
        /// order: at the station itself, and at the locations it works from afar (see <c>LocationsDispatchedBy</c>).
        /// </summary>
        /// <remarks>
        /// A clearance at a remote location directly beside the train's own call at the station is left out: the
        /// dispatcher sets that route when clearing the train in or out of the station. A call elsewhere in the run —
        /// a train from the station that comes back through the junction after turning, or one that never calls at
        /// the station at all — is a clearance of its own. Simultaneous clearances are ordered arrivals (and shunting
        /// tasks) first, since a train pulling in has to be cleared in before the platform is given away, then by train.
        /// </remarks>
        /// <param name="trains">The timetable's trains.</param>
        /// <param name="settings">The operating period.</param>
        /// <param name="plan">See <c>ClearancesAt</c>.</param>
        public IReadOnlyList<Clearance> Clearances(IEnumerable<Train> trains, SessionsSettings settings, Plan? plan = null)
        {
            station = station.ValueOrException(nameof(station));
            trains = trains.ValueOrException(nameof(trains));
            var layout = station.Layout;
            var dispatched = layout is not null && station is Station dispatcher
                ? layout.LocationsDispatchedBy(dispatcher)
                : [station];
            var startingBeforeFirstSession = plan?.TrainsStartingBeforeFirstSession;
            var trainList = trains.ToList();
            return
            [
                .. dispatched
                    .SelectMany(location => trainList
                        .SelectMany(train => train.ClearancesAt(location, settings,
                            startingBeforeFirstSession?.Contains(train) ?? StartsBeforeFirstSession(train, null),
                            isRemote: !location.Equals(station)))
                        .Where(clearance => !clearance.IsRemote || !IsNextToCallAt(clearance.Call, station)))
                    .OrderBy(clearance => clearance.TimeOfDay)
                    .ThenBy(clearance => clearance.Kind is ClearanceKind.Arrival or ClearanceKind.ShuntingTask ? 0 : 1)
                    .ThenBy(clearance => clearance.Call.Train.IdentityWithCompany, StringComparer.CurrentCulture)
            ];
        }
    }

    private static bool StartsBeforeFirstSession(Train train, Plan? plan)
    {
        if (plan is not null) return plan.StartsBeforeFirstSession(train);
        var firstSessionStart = train.CallsInRunOrder.FirstOrDefault()?.OperationLocation.Layout?.Settings.General.FirstSessionStart ?? TimeSpan.Zero;
        return firstSessionStart > TimeSpan.Zero && train.StartsBefore(firstSessionStart);
    }

    private static bool IsNextToCallAt(StationCall call, OperationLocation station)
    {
        var calls = call.Train.CallsInRunOrder;
        var index = -1;
        for (var i = 0; i < calls.Count; i++) if (calls[i] == call) index = i;
        return (index > 0 && calls[index - 1].OperationLocation.Equals(station)) ||
               (index >= 0 && index < calls.Count - 1 && calls[index + 1].OperationLocation.Equals(station));
    }
}
