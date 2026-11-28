namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Provides extension methods for the topology positions saved with a <see cref="Timetable"/>.
/// </summary>
public static class LayoutTopologyExtensions
{
    extension(Timetable timetable)
    {
        /// <summary>
        /// Where the planner has put <paramref name="location"/>, or <see langword="null"/> when it has
        /// not been moved and is to be placed automatically.
        /// </summary>
        /// <param name="location">The location whose saved position is wanted.</param>
        public TopologyPosition? TopologyPositionOf(OperationLocation? location) =>
            location is null ? null : timetable.TopologyPositions.FirstOrDefault(p => p.LocationId == location.Id);

        /// <summary>
        /// Records where the planner has put <paramref name="location"/>, replacing any position it had.
        /// </summary>
        /// <param name="location">The location that was moved.</param>
        /// <param name="x">Its new horizontal coordinate, in diagram units.</param>
        /// <param name="y">Its new vertical coordinate, in diagram units.</param>
        public void SetTopologyPosition(OperationLocation location, double x, double y)
        {
            location = location.ValueOrException(nameof(location));
            if (timetable.TopologyPositionOf(location) is { } existing) (existing.X, existing.Y) = (x, y);
            else timetable.TopologyPositions.Add(new TopologyPosition { LocationId = location.Id, X = x, Y = y });
        }

        /// <summary>
        /// Forgets the position of <paramref name="location"/>, so nothing is left placing a location the
        /// layout no longer has. Call it when a location is removed. Returns <c>true</c> when there was a
        /// position to forget.
        /// </summary>
        /// <param name="location">The location being removed.</param>
        public bool ForgetTopologyPositionOf(OperationLocation? location) =>
            timetable.TopologyPositionOf(location) is { } position && timetable.TopologyPositions.Remove(position);

        /// <summary>
        /// Forgets every position the planner has set, so the whole diagram is drawn automatically again.
        /// Returns <c>true</c> when there was something to forget.
        /// </summary>
        public bool ClearTopologyPositions()
        {
            if (timetable.TopologyPositions.Count == 0) return false;
            timetable.TopologyPositions.Clear();
            return true;
        }

        /// <summary>
        /// Whether the planner has moved anything in the topology diagram.
        /// </summary>
        public bool HasTopologyPositions => timetable.TopologyPositions.Count > 0;
    }
}
