namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying which <see cref="ScheduledObject"/> to couple to the train, optionally at a given position.</summary>
public sealed record CoupleNote(ScheduledObject ScheduledObject, int PositionInTrain = 0) : GeneratedNote;
