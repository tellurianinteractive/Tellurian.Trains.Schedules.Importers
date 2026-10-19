namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note saying the train departs with a locomotive at each end — the <paramref name="Front"/> ones at the
/// head, the <paramref name="Back"/> ones at the other end. Nothing about a train hauled by locomotives
/// suggests the second end, so it is said outright.
/// </summary>
public sealed record LocomotiveAtEachEndNote(IReadOnlyList<ScheduledObject> Front, IReadOnlyList<ScheduledObject> Back) : GeneratedNote;
