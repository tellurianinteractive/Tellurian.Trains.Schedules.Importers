namespace Tellurian.Trains.Schedules.Planning.Timetables;

/// <summary>
/// A slice of a train as it appears in one graphical timetable column. When a train is
/// overtaken by another at a station (it stops long enough for a faster train to pass),
/// it is split into two or more segments so the visual order in the diagram is correct.
/// </summary>
/// <param name="Train">The train this segment belongs to.</param>
/// <param name="FromCallIndex">Index into <see cref="Train.Calls"/> where this segment starts.</param>
/// <param name="ToCallIndex">Index into <see cref="Train.Calls"/> where this segment ends.</param>
public sealed record GraphicalTrainSegment(Train Train, int FromCallIndex, int ToCallIndex);
