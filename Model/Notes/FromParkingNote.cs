namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying to fetch the <see cref="ScheduledObject"/> from where it is parked.</summary>
public sealed record FromParkingNote(ScheduledObject ScheduledObject) : GeneratedNote;
