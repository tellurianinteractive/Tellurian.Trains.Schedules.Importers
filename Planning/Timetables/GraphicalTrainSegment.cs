namespace Tellurian.Trains.Schedules.Planning.Timetables;

/// <summary>
/// A slice of a train as it appears in one graphical timetable column. When a train is
/// overtaken by another at a station (it stops long enough for a faster train to pass),
/// it is split into two or more segments so the visual order in the diagram is correct.
/// </summary>
/// <param name="Train">The train this segment belongs to.</param>
/// <param name="FromCallIndex">Index into the train's calls <em>in run order</em> where this segment
/// starts. <see cref="Train.Calls"/> is in insertion order, which on a hand-edited train is not the order
/// the train works its calls, so segments are cut from <c>Train.CallsInRunOrder</c>.</param>
/// <param name="ToCallIndex">Index into the train's calls in run order where this segment ends.</param>
public sealed record GraphicalTrainSegment(Train Train, int FromCallIndex, int ToCallIndex);
