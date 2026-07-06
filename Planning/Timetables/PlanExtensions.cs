using Tellurian.Trains.Schedules.Planning.Layouts;

namespace Tellurian.Trains.Schedules.Planning.Timetables;

/// <summary>
/// Provides extension methods for planning operations on trains and timetable stretches.
/// </summary>
public static class PlanExtensions
{
    extension(TimetableStretch timetableStretch)
    {
        /// <summary>
        /// Returns the graphical train segments for all trains travelling in <paramref name="direction"/>
        /// along this stretch, sorted in the visual left-to-right column order of the graphical timetable.
        /// </summary>
        /// <remarks>
        /// Trains that are overtaken by a faster train (because they stop long enough for the other to
        /// pass) are split at the overtake station. The first part appears before the overtaking train,
        /// and the remainder appears after it, matching the visual order in a space-time diagram.
        /// The sort key for each segment is the time linearly extrapolated back to the first station of
        /// the stretch in the given direction, using the segment's own slope.
        /// </remarks>
        /// <param name="trains">All trains in the timetable to consider.</param>
        /// <param name="direction">The direction to sort.</param>
        /// <returns>Segments in graphical left-to-right column order.</returns>
        public IReadOnlyList<GraphicalTrainSegment> SortedTrainSegments(
            IEnumerable<Train> trains, TrainGraphDirection direction)
        {
            var directedTrains = trains
                .Where(t => timetableStretch.InferDirection(t) == direction)
                .ToList();

            var segments = directedTrains
                .Select(t => new GraphicalTrainSegment(t, 0, t.Calls.Count - 1))
                .ToList();

            var split = timetableStretch.SplitOvertakenSegments(segments);

            return [.. split.OrderBy(s =>
                timetableStretch.ExtrapolatedTimeAtFirstStation(s, direction).TotalMinutes)];
        }

        private TrainGraphDirection? InferDirection(Train train)
        {
            var calls = train.Calls.ToArray();
            foreach (var ts in timetableStretch.Stretches)
            {
                for (var i = 0; i < calls.Length - 1; i++)
                {
                    // Skip intermediate SCL stops, but stop immediately at any SCL that
                    // is itself a track stretch endpoint (ts.Start or ts.End), so that
                    // stretches whose endpoint is a signal box are still matched.
                    var j = i + 1;
                    while (j < calls.Length
                           && calls[j].OperationLocation is SignalControlledLocation
                           && !calls[j].OperationLocation.Equals(ts.Start)
                           && !calls[j].OperationLocation.Equals(ts.End))
                        j++;
                    if (j >= calls.Length) continue;

                    if (calls[i].OperationLocation.Equals(ts.Start) && calls[j].OperationLocation.Equals(ts.End))
                        return TrainGraphDirection.Upward;
                    if (calls[i].OperationLocation.Equals(ts.End) && calls[j].OperationLocation.Equals(ts.Start))
                        return TrainGraphDirection.Downward;
                }
            }
            return null;
        }

        private List<GraphicalTrainSegment> SplitOvertakenSegments(List<GraphicalTrainSegment> segments)
        {
            var result = new List<GraphicalTrainSegment>();
            foreach (var seg in segments)
            {
                var splitPoints = new SortedSet<int>();
                foreach (var other in segments.Where(s => !ReferenceEquals(s.Train, seg.Train)))
                {
                    var sp = timetableStretch.FindOvertakeIndex(seg, other);
                    if (sp is { } index) splitPoints.Add(index);
                }

                if (splitPoints.Count == 0)
                {
                    result.Add(seg);
                    continue;
                }

                var from = seg.FromCallIndex;
                foreach (var sp in splitPoints.Where(sp => sp > from && sp < seg.ToCallIndex))
                {
                    result.Add(new GraphicalTrainSegment(seg.Train, from, sp));
                    from = sp;
                }
                result.Add(new GraphicalTrainSegment(seg.Train, from, seg.ToCallIndex));
            }
            return result;
        }

        // Returns the call index in overridden where overrider passes through while overridden is stopped.
        private int? FindOvertakeIndex(GraphicalTrainSegment overridden, GraphicalTrainSegment overrider)
        {
            for (var i = overridden.FromCallIndex; i <= overridden.ToCallIndex; i++)
            {
                var call = overridden.Train.Calls[i];
                if (!timetableStretch.DistanceToStation(call.OperationLocation).HasValue) continue;

                for (var j = overrider.FromCallIndex; j <= overrider.ToCallIndex; j++)
                {
                    var other = overrider.Train.Calls[j];
                    if (!other.OperationLocation.Equals(call.OperationLocation)) continue;

                    // overrider passes overridden: overridden arrived first (or same time)
                    // but overrider departs before overridden departs
                    if (call.Arrival <= other.Arrival && other.Departure < call.Departure)
                        return i;
                }
            }
            return null;
        }

        private Time ExtrapolatedTimeAtFirstStation(GraphicalTrainSegment segment, TrainGraphDirection direction)
        {
            var calls = timetableStretch
                .StretchCallsForSegment(segment.Train, segment.FromCallIndex, segment.ToCallIndex)
                .OrderBy(c => c.Time.TotalMinutes)
                .ToList();

            if (calls.Count == 0) return Time.Zero;
            if (calls.Count == 1) return calls[0].Time;

            var totalDistance = timetableStretch.Stretches.Sum(s => s.Distance);
            var d0 = direction == TrainGraphDirection.Upward ? 0.0 : totalDistance;

            // Use the two calls nearest to the first station for the most accurate slope
            var byProximity = direction == TrainGraphDirection.Upward
                ? calls.OrderBy(c => c.Distance).ToList()
                : calls.OrderByDescending(c => c.Distance).ToList();

            var (d1, t1) = (byProximity[0].Distance, byProximity[0].Time);
            var (d2, t2) = (byProximity[1].Distance, byProximity[1].Time);

            if (Math.Abs(d2 - d1) < 0.001) return t1;

            var minutesPerKm = (t2.TotalMinutes - t1.TotalMinutes) / (d2 - d1);
            var extrapolatedMinutes = t1.TotalMinutes + (d0 - d1) * minutesPerKm;
            return Time.FromTimeSpan(TimeSpan.FromMinutes(extrapolatedMinutes));
        }

        // Yields (Distance, Time) for each call in [fromIdx, toIdx] that lies on this stretch.
        // Uses departure time for all calls except the last, where arrival time is used.
        private IEnumerable<(double Distance, Time Time)> StretchCallsForSegment(
            Train train, int fromIdx, int toIdx)
        {
            for (var i = fromIdx; i <= toIdx; i++)
            {
                var call = train.Calls[i];
                var dist = timetableStretch.DistanceToStation(call.OperationLocation);
                if (dist is { } d)
                {
                    var time = i < toIdx ? call.Departure : call.Arrival;
                    yield return (d, time);
                }
            }
        }
    }

    extension(Plan plan)
    {
        /// <summary>
        /// Gets whether the plan's operating window wraps around midnight, driven solely by
        /// <see cref="Model.Settings.GeneralSettings.RunsOverMidnight"/>. When set, a train may start before
        /// midnight and continue past it; otherwise every train must fit entirely within the window
        /// (see <c>FitsWithinOperatingWindow</c>). A plain 00:00–23:59 window without the flag does not wrap,
        /// so a train spilling into the next day is rejected.
        /// </summary>
        public bool IsWrappingMidnight => plan.Layout.Settings.General.RunsOverMidnight;

        /// <summary>
        /// Gets whether <paramref name="train"/> fits within the plan's operating window: its whole span, from the
        /// first call arrival (<see cref="Train.DriverStartTime"/>) to the last call departure
        /// (<see cref="Train.DriverEndTime"/>), must lie within
        /// <see cref="Model.Settings.GeneralSettings.StartTime"/>–<see cref="Model.Settings.GeneralSettings.EndTime"/>.
        /// When the window wraps midnight (see <c>IsWrappingMidnight</c>) the train's end may spill past
        /// midnight, so only the start is required to stay within the day. A train with no calls never fits.
        /// </summary>
        /// <param name="train">The train to test. Cannot be null.</param>
        /// <returns><c>true</c> when the train fits the operating window; otherwise <c>false</c>.</returns>
        public bool FitsWithinOperatingWindow(Train train)
        {
            ArgumentNullException.ThrowIfNull(train);
            return train.Calls.Count > 0 && plan.OperatingWindowContains(train.DriverStartTime, train.DriverEndTime);
        }

        /// <summary>
        /// Gets whether <paramref name="train"/> would still fit the plan's operating window after being shifted by
        /// <paramref name="minutes"/> (see <c>FitsWithinOperatingWindow</c>). Used to validate a move or clone
        /// before it happens, without mutating the train.
        /// </summary>
        /// <param name="train">The train to test. Cannot be null.</param>
        /// <param name="minutes">The number of minutes to shift; negative is earlier, positive later.</param>
        /// <returns><c>true</c> when the shifted train fits the operating window; otherwise <c>false</c>.</returns>
        public bool FitsWhenMovedBy(Train train, int minutes)
        {
            ArgumentNullException.ThrowIfNull(train);
            return train.Calls.Count > 0 &&
                plan.OperatingWindowContains(train.DriverStartTime.AddMinutes(minutes), train.DriverEndTime.AddMinutes(minutes));
        }

        // The shared operating-window rule. The start must always lie within the operating day (00:00–24:00);
        // when the window wraps midnight the end may spill past it, otherwise the whole span must fall within
        // [StartTime, EndTime].
        private bool OperatingWindowContains(Time start, Time end)
        {
            if (start.Value < TimeSpan.Zero || start.Value >= TimeSpan.FromDays(1)) return false;
            if (plan.IsWrappingMidnight) return true;
            var general = plan.Layout.Settings.General;
            return start.Value >= general.StartTime && end.Value <= general.EndTime;
        }

        /// <summary>
        /// Creates a new train of the given <paramref name="category"/> running the shortest path from
        /// <paramref name="from"/> to <paramref name="to"/>, computes its run and stop times, assigns the next
        /// free id and number, and adds it to the plan's timetable.
        /// </summary>
        /// <remarks>
        /// Run times come from <see cref="TrainExtensions.ScheduledTravelMinutes"/> using the layout's
        /// <see cref="Model.Settings.TimeAndSpeedSettings"/> and the train's effective speed: the train's
        /// <paramref name="maxSpeed"/> is capped per stretch by the stretch's own maximum speed (see
        /// <see cref="TrainExtensions.EffectiveScaleSpeed"/>). When <paramref name="maxSpeed"/> is <c>null</c>
        /// the train's <see cref="Train.MaxSpeed"/> stays unset and the speed falls back to the category's
        /// <see cref="TrainCategory.DefaultSpeed"/>. The train stops at
        /// the origin and terminus (always) and at any shadow station; at an intermediate location it stops only
        /// when the category can exchange there (a passenger category where <see cref="OperationLocation.HasPassengerExchange"/>,
        /// a freight category where <see cref="OperationLocation.HasCargoExchange"/>) — otherwise it passes through.
        /// A <see cref="SignalControlledLocation"/> is never a stop. Where the path reverses direction the stop is
        /// long enough for the loco runaround.
        /// </remarks>
        /// <param name="category">The category of the train.</param>
        /// <param name="from">The origin location from which the train will depart.</param>
        /// <param name="to">The destination location to which the train will travel.</param>
        /// <param name="startTime">The scheduled departure time for the train from the origin location.</param>
        /// <param name="preparationMinutes">The number of minutes required to prepare the train before first departure. Must be a non-negative value.</param>
        /// <param name="finishingMinutes">The number of minutes required to finish the train after last arrival. Must be a non-negative value.</param>
        /// <param name="maxSpeed">The train's maximum scale speed in km/h, used to compute run times (capped per stretch by the
        /// stretch's own maximum speed). When <c>null</c>, the category's <see cref="TrainCategory.DefaultSpeed"/> is used.</param>
        /// <returns>The created train, already added to the timetable, or <c>null</c> when no train could be built
        /// (origin equals destination, no path exists, or a location on the path has no track) or the finished
        /// train does not fit the plan's operating window (see <c>FitsWithinOperatingWindow</c>).</returns>
        public Train? Create(TrainCategory category, OperationLocation from, OperationLocation to, Time startTime, int preparationMinutes = 10, int finishingMinutes = 10, int? maxSpeed = null)
        {
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(from);
            ArgumentNullException.ThrowIfNull(to);

            var path = PathFinder.FindShortestPath(plan.Layout, from, to);
            if (path is null) return null;

            var locations = path.Locations.ToList();
            if (locations.Count < 2) return null;

            var settings = plan.Layout.Settings.TimeAndSpeed;
            var directionChanges = path.DirectionChanges.ToHashSet();

            var train = new Train(NextTrainId(), category, NextTrainNumber()) { Sessions = Sessions.All, MaxSpeed = maxSpeed };
            plan.Timetable.Add(train);

            var nextCallId = NextCallId();
            var previousDeparture = startTime;

            for (var i = 0; i < locations.Count; i++)
            {
                var location = locations[i];
                if (PickTrack(location) is not { } track) return null;

                Time arrival, departure;
                bool isArrival, isDeparture;

                if (i == 0)
                {
                    // Origin: prepared for departure, so the driver's service starts preparationMinutes before.
                    departure = startTime;
                    arrival = startTime.AddMinutes(-preparationMinutes);
                    (isArrival, isDeparture) = (false, true);
                }
                else
                {
                    var stretch = path.Segments[i - 1].TrackStretch;
                    var runMinutes = Math.Max(1, (int)Math.Round(train.ScheduledTravelMinutes(stretch, settings)));
                    arrival = previousDeparture.AddMinutes(runMinutes);

                    if (i == locations.Count - 1)
                    {
                        // Terminus: the driver's service ends finishingMinutes after last arrival.
                        departure = arrival.AddMinutes(finishingMinutes);
                        (isArrival, isDeparture) = (true, false);
                    }
                    else if (StopsAt(location))
                    {
                        departure = arrival.AddMinutes(plan.DwellMinutes(location, directionChanges.Contains(location)));
                        (isArrival, isDeparture) = (true, true);
                    }
                    else
                    {
                        departure = arrival;
                        (isArrival, isDeparture) = (false, false);
                    }
                }

                var call = new StationCall(nextCallId++, track, arrival, departure);
                train.Add(call);
                call.IsArrival = isArrival;
                call.IsDeparture = isDeparture;
                previousDeparture = departure;
            }

            // The finished train must fit the plan's operating window; if it runs outside it, undo the add.
            if (!plan.FitsWithinOperatingWindow(train))
            {
                plan.Timetable.Trains.Remove(train);
                return null;
            }

            return train;

            int NextTrainId() => plan.Timetable.Trains.Select(t => t.Id).DefaultIfEmpty(0).Max() + 1;
            int NextTrainNumber() => plan.Timetable.Trains.Select(t => t.Number).DefaultIfEmpty(0).Max() + 1;
            int NextCallId() => plan.Timetable.Trains.SelectMany(t => t.Calls).Select(c => c.Id).DefaultIfEmpty(0).Max() + 1;

            // A train stops at a location when the category can exchange there; shadow stations always stop and
            // signal-controlled locations never do. The origin and terminus are handled by the caller above.
            bool StopsAt(OperationLocation location)
            {
                if (location is SignalControlledLocation) return false;
                if (location is Station { IsShadow: true }) return true;
                var passenger = category.IsPassenger && location.HasPassengerExchange;
                var freight = category.IsFreight && location.HasCargoExchange;
                return passenger || freight;
            }

            // Prefer a scheduled main track, then any scheduled track, then any track at all.
            static StationTrack? PickTrack(OperationLocation location) =>
                location.Tracks.FirstOrDefault(t => t.IsMain && t.IsScheduled)
                ?? location.Tracks.FirstOrDefault(t => t.IsScheduled)
                ?? location.Tracks.FirstOrDefault();
        }

        /// <summary>
        /// Creates a repeating sequence of identical trains running the shortest path from <paramref name="from"/>
        /// to <paramref name="to"/>: the first departs at <paramref name="startTime"/> and each subsequent one
        /// <paramref name="intervalMinutes"/> later, until the next departure would fall after
        /// <paramref name="endTime"/>. The first train is built with <c>Create</c> and the rest are produced by
        /// <c>Clone</c>, so every train shares the same route, run and stop times, category and speed.
        /// </summary>
        /// <remarks>
        /// Departures are measured from <paramref name="startTime"/>: the sequence stops as soon as
        /// <c>startTime + n·interval</c> passes <paramref name="endTime"/>, or as soon as a clone would fall
        /// outside the plan's operating window (see <c>FitsWhenMovedBy</c>). When the first train cannot be
        /// built (see <c>Create</c>) the result is empty.
        /// </remarks>
        /// <param name="category">The category of the trains.</param>
        /// <param name="from">The origin location from which the trains depart.</param>
        /// <param name="to">The destination location to which the trains travel.</param>
        /// <param name="startTime">The scheduled departure time of the first train.</param>
        /// <param name="endTime">The latest departure time; no train departs after it.</param>
        /// <param name="intervalMinutes">The number of minutes between consecutive departures. Must be greater than zero.</param>
        /// <param name="preparationMinutes">The number of minutes required to prepare each train before first departure. Must be non-negative.</param>
        /// <param name="finishingMinutes">The number of minutes required to finish each train after last arrival. Must be non-negative.</param>
        /// <param name="maxSpeed">The trains' maximum scale speed in km/h; when <c>null</c>, the category's <see cref="TrainCategory.DefaultSpeed"/> is used.</param>
        /// <returns>The created trains in departure order, already added to the timetable; empty when none could be built.</returns>
        public IReadOnlyList<Train> CreateRepeating(TrainCategory category, OperationLocation from, OperationLocation to, Time startTime, Time endTime, int intervalMinutes, int preparationMinutes = 10, int finishingMinutes = 10, int? maxSpeed = null)
        {
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(from);
            ArgumentNullException.ThrowIfNull(to);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(intervalMinutes);

            if (plan.Create(category, from, to, startTime, preparationMinutes, finishingMinutes, maxSpeed) is not { } first)
                return [];

            return [first, .. plan.CloneMany(first, endTime, intervalMinutes)];
        }

        /// <summary>
        /// Adds a repeating sequence of clones of <paramref name="train"/> to the plan's timetable: one clone
        /// every <paramref name="intervalMinutes"/> after the train's departure, until the next clone's
        /// departure would fall after <paramref name="endTime"/>. Each clone is a shifted copy made by
        /// <c>Clone</c>, so they share the train's route, run and stop times, category and speed.
        /// </summary>
        /// <remarks>
        /// Departures are measured from the train's first departing call: a clone is added for every
        /// <c>departure + n·interval</c> that is not after <paramref name="endTime"/>, stopping as soon as one
        /// would fall outside the plan's operating window (see <c>FitsWhenMovedBy</c>). The train itself is not
        /// included in the result.
        /// </remarks>
        /// <param name="train">The train to clone repeatedly. Cannot be null.</param>
        /// <param name="endTime">The latest departure time; no clone departs after it.</param>
        /// <param name="intervalMinutes">The number of minutes between consecutive departures. Must be greater than zero.</param>
        /// <returns>The added clones in departure order; empty when none fit before <paramref name="endTime"/>.</returns>
        public IReadOnlyList<Train> CloneMany(Train train, Time endTime, int intervalMinutes)
        {
            ArgumentNullException.ThrowIfNull(train);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(intervalMinutes);

            var startTime = train.Calls.First(c => c.IsDeparture).Departure;
            var clones = new List<Train>();
            for (var offset = intervalMinutes; startTime.AddMinutes(offset).Value <= endTime.Value; offset += intervalMinutes)
            {
                if (plan.Clone(train, offset) is not { } clone) break;
                clones.Add(clone);
            }
            return clones;
        }

        /// <summary>
        /// Shifts every call of the specified train forward or backward in time by the given number of
        /// minutes, mutating the train in place.
        /// </summary>
        /// <remarks>
        /// The whole shifted train must still fit the plan's operating window (see
        /// <c>FitsWhenMovedBy</c>); when it would not, the train is left unchanged and <c>null</c>
        /// is returned.
        /// </remarks>
        /// <param name="train">The train to move. Cannot be null.</param>
        /// <param name="minutes">The number of minutes to move the train; negative moves it earlier, positive later.</param>
        /// <returns>The moved train, or <c>null</c> when the move would take it outside the operating window.</returns>
        public Train? Move(Train train, int minutes)
        {
            ArgumentNullException.ThrowIfNull(train);
            if (!plan.FitsWhenMovedBy(train, minutes)) return null;

            foreach (var call in train.Calls)
            {
                call.Arrival = call.Arrival.AddMinutes(minutes);
                call.Departure = call.Departure.AddMinutes(minutes);
            }
            return train;
        }

        /// <summary>
        /// Creates a copy of the train with all timings shifted by the given number of minutes, assigns it the
        /// next free id, number and call ids, and adds it to the plan's timetable.
        /// </summary>
        /// <remarks>
        /// The whole shifted clone must fit the plan's operating window (see <c>FitsWhenMovedBy</c>);
        /// when it would not, nothing is added and <c>null</c> is returned.
        /// </remarks>
        /// <param name="train">The train to clone. Cannot be null.</param>
        /// <param name="minutes">The number of minutes to shift the clone; negative moves it earlier, positive later.</param>
        /// <returns>The cloned train, already added to the timetable, or <c>null</c> when the shift would take
        /// it outside the operating window.</returns>
        public Train? Clone(Train train, int minutes)
        {
            ArgumentNullException.ThrowIfNull(train);
            if (!plan.FitsWhenMovedBy(train, minutes)) return null;

            var clone = new Train(NextTrainId(), NextTrainNumber())
            {
                Category = train.Category,
                CategoryId = train.CategoryId,
                Company = train.Company,
                CompanyId = train.CompanyId,
                MaxSpeed = train.MaxSpeed,
                Sessions = train.Sessions,
                Length = train.Length,
                Remark = train.Remark,
                ShowContinuesAs = train.ShowContinuesAs,
            };
            plan.Timetable.Add(clone);

            var nextCallId = NextCallId();
            foreach (var call in train.Calls)
            {
                var copy = new StationCall(nextCallId++, call.Track,
                    call.Arrival.AddMinutes(minutes), call.Departure.AddMinutes(minutes));
                clone.Add(copy);
                copy.IsArrival = call.IsArrival;
                copy.IsDeparture = call.IsDeparture;
            }
            return clone;

            int NextTrainId() => plan.Timetable.Trains.Select(t => t.Id).DefaultIfEmpty(0).Max() + 1;
            int NextTrainNumber() => plan.Timetable.Trains.Select(t => t.Number).DefaultIfEmpty(0).Max() + 1;
            int NextCallId() => plan.Timetable.Trains.SelectMany(t => t.Calls).Select(c => c.Id).DefaultIfEmpty(0).Max() + 1;
        }

        /// <summary>
        /// Recomputes every call's arrival and departure times of <paramref name="train"/> from its current
        /// stop pattern (the <see cref="StationCall.IsStop"/> flags), keeping the origin's departure fixed.
        /// Use this after changing which stations a train stops at so the run and dwell times follow the new
        /// pattern.
        /// </summary>
        /// <remarks>
        /// The origin call is left untouched: its departure is the fixed anchor and its arrival keeps the
        /// existing preparation dwell. From there each leg's run time is recomputed with
        /// <see cref="TrainExtensions.ScheduledTravelMinutes"/> over the <see cref="TrackStretch"/> between the
        /// two calls, using the train's effective speed. At a call that is now a stop the dwell is preserved
        /// when the call already had one (so an intentional or terminus finishing dwell is kept); a call that
        /// has just become a stop (its times were equal) is given the standard dwell from
        /// <see cref="DwellMinutes"/>, including the loco runaround where the travel direction reverses. A call
        /// that is now a pass-through gets equal arrival and departure times (no dwell). The change is
        /// all-or-nothing: it is not applied and <c>null</c> is returned when two consecutive calls are not
        /// joined by a track stretch, or when the recomputed train would fall outside the plan's operating
        /// window (see <c>FitsWithinOperatingWindow</c>).
        /// </remarks>
        /// <param name="train">The train whose timings to recompute. Cannot be null.</param>
        /// <returns>The updated train, or <c>null</c> when the timings could not be recomputed (a missing
        /// stretch) or the result would not fit the operating window.</returns>
        public Train? UpdateTimings(Train train)
        {
            ArgumentNullException.ThrowIfNull(train);
            var calls = train.Calls.ToArray();
            if (calls.Length < 2) return train;

            var settings = plan.Layout.Settings.TimeAndSpeed;

            // Resolve the stretch and its travel direction for each leg between consecutive calls.
            var legs = new (TrackStretch Stretch, bool Forward)[calls.Length - 1];
            for (var i = 0; i < legs.Length; i++)
            {
                var from = calls[i].OperationLocation;
                var to = calls[i + 1].OperationLocation;
                if (plan.StretchBetween(from, to) is not { } stretch) return null;
                legs[i] = (stretch, stretch.Start.Equals(from));
            }

            // A call where the travel direction reverses needs a loco runaround (see DwellMinutes). Direction
            // is taken relative to each stretch's own Start/End, the same convention the path finder uses.
            var reverses = new bool[calls.Length];
            for (var i = 1; i < legs.Length; i++)
                reverses[i] = legs[i].Forward != legs[i - 1].Forward;

            // Compute the new times into buffers first so the update is all-or-nothing.
            var arrivals = new Time[calls.Length];
            var departures = new Time[calls.Length];
            arrivals[0] = calls[0].Arrival;      // origin keeps its preparation dwell
            departures[0] = calls[0].Departure;  // origin departure is the fixed anchor

            for (var i = 1; i < calls.Length; i++)
            {
                var runMinutes = Math.Max(1, (int)Math.Round(train.ScheduledTravelMinutes(legs[i - 1].Stretch, settings)));
                arrivals[i] = departures[i - 1].AddMinutes(runMinutes);

                var call = calls[i];
                if (call.IsStop)
                {
                    // Keep a dwell the call already has (an intentional stop or the terminus finishing time);
                    // give a newly-added stop (its times were equal) the standard dwell.
                    var existingDwell = (int)Math.Round((call.Departure.Value - call.Arrival.Value).TotalMinutes);
                    var dwell = existingDwell > 0 ? existingDwell : plan.DwellMinutes(call.OperationLocation, reverses[i]);
                    departures[i] = arrivals[i].AddMinutes(dwell);
                }
                else
                {
                    departures[i] = arrivals[i]; // pass-through: no dwell
                }
            }

            // The recomputed train must still fit the plan's operating window.
            if (!plan.OperatingWindowContains(arrivals[0], departures[^1])) return null;

            for (var i = 0; i < calls.Length; i++)
            {
                calls[i].Arrival = arrivals[i];
                calls[i].Departure = departures[i];
            }
            return train;
        }

        // Fast-clock dwell: at least the minimum stop; where the path reverses, at least the loco runaround
        // (a real duration converted to fast-clock minutes).
        private int DwellMinutes(OperationLocation location, bool reverses)
        {
            var settings = plan.Layout.Settings.TimeAndSpeed;
            var minimumStop = location.Timings.MinimumStopMinutes ?? settings.StationTimings.MinimumStopMinutes ?? 3;
            if (!reverses) return minimumStop;
            var runaroundReal = location.Timings.LocoRunaroundRealMinutes ?? settings.StationTimings.LocoRunaroundRealMinutes ?? 5;
            return Math.Max(minimumStop, runaroundReal * settings.FastClockSpeed);
        }

        // The single track stretch joining two locations, in either orientation; null when none exists.
        private TrackStretch? StretchBetween(OperationLocation a, OperationLocation b) =>
            plan.Layout.TrackStretches.FirstOrDefault(s =>
                (s.Start.Equals(a) && s.End.Equals(b)) || (s.Start.Equals(b) && s.End.Equals(a)));

    }
}
