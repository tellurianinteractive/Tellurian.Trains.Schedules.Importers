namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying to move the <see cref="ScheduledObject"/> to parking.</summary>
public sealed record ToParkingNote(ScheduledObject ScheduledObject) : GeneratedNote;
