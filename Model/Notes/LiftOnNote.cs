namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying to lift the <see cref="ScheduledObject"/> on to the departure track before the train departs.</summary>
public sealed record LiftOnNote(ScheduledObject ScheduledObject) : GeneratedNote;
