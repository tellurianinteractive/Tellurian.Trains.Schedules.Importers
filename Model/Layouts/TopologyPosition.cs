namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Where the planner has put an operation location in the layout's topology diagram, in the diagram's
/// own coordinates. A location the planner has not moved has no entry here and is placed automatically.
/// </summary>
/// <remarks>
/// A layout with cycles — a triangle, a balloon loop, two lines meeting at both ends — cannot be drawn
/// well by any rule that reads the track alone, and the planner knows what the railway looks like. So
/// the diagram is only a starting point: every location may be dragged where it belongs, and what is
/// stored here is the answer. The coordinates are the diagram's, not the screen's, so a saved position
/// survives the diagram being shown at a different size or on paper.
/// </remarks>
public sealed class TopologyPosition
{
    /// <summary>Gets or sets the <see cref="OperationLocation.Id"/> the position belongs to.</summary>
    public int LocationId { get; set; }

    /// <summary>Gets or sets the horizontal coordinate, in diagram units.</summary>
    public double X { get; set; }

    /// <summary>Gets or sets the vertical coordinate, in diagram units.</summary>
    public double Y { get; set; }
}

/// <summary>
/// Provides extension methods for the topology positions saved with a <see cref="Layout"/>.
/// </summary>
public static class LayoutTopologyExtensions
{
    extension(Layout layout)
    {
        /// <summary>
        /// Where the planner has put <paramref name="location"/>, or <see langword="null"/> when it has
        /// not been moved and is to be placed automatically.
        /// </summary>
        /// <param name="location">The location whose saved position is wanted.</param>
        public TopologyPosition? TopologyPositionOf(OperationLocation? location) =>
            location is null ? null : layout.TopologyPositions.FirstOrDefault(p => p.LocationId == location.Id);

        /// <summary>
        /// Records where the planner has put <paramref name="location"/>, replacing any position it had.
        /// </summary>
        /// <param name="location">The location that was moved.</param>
        /// <param name="x">Its new horizontal coordinate, in diagram units.</param>
        /// <param name="y">Its new vertical coordinate, in diagram units.</param>
        public void SetTopologyPosition(OperationLocation location, double x, double y)
        {
            location = location.ValueOrException(nameof(location));
            if (layout.TopologyPositionOf(location) is { } existing) (existing.X, existing.Y) = (x, y);
            else layout.TopologyPositions.Add(new TopologyPosition { LocationId = location.Id, X = x, Y = y });
        }

        /// <summary>
        /// Forgets the position of <paramref name="location"/>, so nothing is left placing a location the
        /// layout no longer has. Call it when a location is removed. Returns <c>true</c> when there was a
        /// position to forget.
        /// </summary>
        /// <param name="location">The location being removed.</param>
        public bool ForgetTopologyPositionOf(OperationLocation? location) =>
            layout.TopologyPositionOf(location) is { } position && layout.TopologyPositions.Remove(position);

        /// <summary>
        /// Forgets every position the planner has set, so the whole diagram is drawn automatically again.
        /// Returns <c>true</c> when there was something to forget.
        /// </summary>
        public bool ClearTopologyPositions()
        {
            if (layout.TopologyPositions.Count == 0) return false;
            layout.TopologyPositions.Clear();
            return true;
        }

        /// <summary>
        /// Whether the planner has moved anything in the topology diagram.
        /// </summary>
        public bool HasTopologyPositions => layout.TopologyPositions.Count > 0;
    }
}
