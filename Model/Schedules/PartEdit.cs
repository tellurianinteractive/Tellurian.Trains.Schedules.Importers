namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// The effect of changing the span of one <see cref="ScheduledTrainPart">part</see> in a
/// <see cref="Schedule"/>: the part's new from- and to-calls, and the calls the neighbouring parts are
/// adapted to so the vehicle's working still hangs together.
/// </summary>
/// <remarks>
/// A neighbour is adapted only where it meets the edited part today and its own train calls at the new
/// joint location; otherwise it is left as it is and the working is left inconsistent on purpose — the
/// planner sees it as a conflict and resolves it in a later edit. The <c>Leaves…</c> and <c>Overlaps…</c>
/// properties tell what the edit leaves behind, so an editor can show it before it is applied.
/// </remarks>
/// <param name="Part">The part being edited.</param>
/// <param name="From">The call the part departs from after the edit.</param>
/// <param name="To">The call the part arrives at after the edit.</param>
/// <param name="Previous">The part worked before <paramref name="Part"/>, or <c>null</c> when it is first.</param>
/// <param name="PreviousTo">The call <paramref name="Previous"/> is adapted to arrive at, or <c>null</c>
/// when it is left unchanged.</param>
/// <param name="Next">The part worked after <paramref name="Part"/>, or <c>null</c> when it is last.</param>
/// <param name="NextFrom">The call <paramref name="Next"/> is adapted to depart from, or <c>null</c> when
/// it is left unchanged.</param>
public sealed record PartEdit(
    ScheduledTrainPart Part,
    StationCall From,
    StationCall To,
    ScheduledTrainPart? Previous,
    StationCall? PreviousTo,
    ScheduledTrainPart? Next,
    StationCall? NextFrom)
{
    /// <summary>True when the previous part is adapted to end where the edited part now starts.</summary>
    public bool AdaptsPrevious => PreviousTo is not null;

    /// <summary>True when the next part is adapted to start where the edited part now ends.</summary>
    public bool AdaptsNext => NextFrom is not null;

    /// <summary>Where the previous part ends after the edit, or <c>null</c> when there is no previous part.</summary>
    public StationCall? PreviousEnd => PreviousTo ?? Previous?.To;

    /// <summary>Where the next part starts after the edit, or <c>null</c> when there is no next part.</summary>
    public StationCall? NextStart => NextFrom ?? Next?.From;

    /// <summary>
    /// True when the vehicle does not end the previous part where the edited part starts, so the working
    /// has a gap before it (validation rule S2).
    /// </summary>
    public bool LeavesGapBefore =>
        PreviousEnd is { } end && !end.OperationLocation.Equals(From.OperationLocation);

    /// <summary>
    /// True when the vehicle does not start the next part where the edited part ends, so the working has
    /// a gap after it (validation rule S2).
    /// </summary>
    public bool LeavesGapAfter =>
        NextStart is { } start && !start.OperationLocation.Equals(To.OperationLocation);

    /// <summary>
    /// True when the previous part is still being worked when the edited part starts, so the vehicle
    /// would be in two places at once (validation rule S1). The preparation and finishing-up time at the
    /// trains' ends count as worked, the same as everywhere else overlap is judged.
    /// </summary>
    public bool OverlapsPrevious => PreviousEnd is { } end && end.WorkEnd > From.WorkStart;

    /// <summary>
    /// True when the next part starts before the edited part is finished with, so the vehicle would be in
    /// two places at once (validation rule S1).
    /// </summary>
    public bool OverlapsNext => NextStart is { } start && To.WorkEnd > start.WorkStart;

    /// <summary>True when the edit leaves the schedule a single contiguous, non-overlapping working.</summary>
    public bool IsConsistent => !(LeavesGapBefore || LeavesGapAfter || OverlapsPrevious || OverlapsNext);
}
