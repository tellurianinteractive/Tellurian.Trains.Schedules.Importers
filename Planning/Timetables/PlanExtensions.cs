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
        /// Creates a new train scheduled to travel from the specified origin to the specified destination, starting at
        /// the given time and accounting for the required preparation period.
        /// </summary>
        /// <param name="from">The origin location from which the train will depart.</param>
        /// <param name="to">The destination location to which the train will travel.</param>
        /// <param name="startTime">The scheduled departure time for the train from the origin location.</param>
        /// <param name="preparationMinutes">The number of minutes required to prepare the train before first departure. Must be a non-negative value.</param>
        /// <param name="finishingMinutes">The number of minutes required to finish the train aftler last arrival. Must be a non-negative value.</param>
        /// <returns>A Train object representing the scheduled journey from the origin to the destination, including timing and
        /// preparation time.</returns>
        /// <exception cref="NotImplementedException">The method is not yet implemented.</exception>
        public Train Create(OperationLocation from, OperationLocation to, Time startTime, int preparationMinutes = 10, int finishingMinutes = 10)
        {
            // TODO: Find shortest path for train in the Layout and calculate run times and stop times
            // using the TimetableSettings.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Moves the specified train forward or backward by the given number of minutes and returns the updated train instance.
        /// </summary>
        /// <param name="train">The train to move. Cannot be null.</param>
        /// <param name="minutes">The number of minutes to move the train forward och backwards in time.</param>
        /// <returns>An updated train instance representing the state of the train after moving forward or backwards by the specified
        /// number of minutes.</returns>
        /// <exception cref="NotImplementedException">The method is not implemented.</exception>
        public Train Move(Train train, int minutes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clones the train and creates a copy with timings moved the specified number of minutes
        /// </summary>
        /// <param name="train">The train to clone</param>
        /// <param name="minutes">The number of minutes to move the train forward och backwards in time.</param>
        /// <returns>A new train that is the clone of the original train.</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>A train must start between 00:00 and 23:59. The operation should fail if the train's start time falls out of bounds</remarks>
        public Train Clone(Train train, int minutes)
        {
            throw new NotImplementedException();
        }

    }
}
