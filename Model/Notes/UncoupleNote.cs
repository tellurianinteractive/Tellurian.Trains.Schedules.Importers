namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying which <see cref="ScheduledObject"/> to uncouple from the train.</summary>
public sealed record UncoupleNote(ScheduledObject ScheduledObject) : GeneratedNote;
