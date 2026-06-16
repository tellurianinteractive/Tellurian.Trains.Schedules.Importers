namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying the <see cref="ScheduledObject"/> reinforces traction over the given <see cref="TrainPart"/>.</summary>
public sealed record ReinforcementNote(ScheduledObject ScheduledObject, TrainPart Part) : GeneratedNote;
