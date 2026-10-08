namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying to lift the <see cref="ScheduledObject"/> off the arrival track after the train has arrived.</summary>
public sealed record LiftOffNote(ScheduledObject ScheduledObject) : GeneratedNote;
