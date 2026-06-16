namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying which <see cref="ScheduledObject"/> to use.</summary>
public sealed record UseNote(ScheduledObject ScheduledObject) : GeneratedNote;
