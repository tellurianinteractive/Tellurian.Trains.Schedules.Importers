namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying that the wagons of a <see cref="ScheduledObject"/> are loaded where the train arrives.</summary>
public sealed record LoadWagonsNote(ScheduledObject ScheduledObject) : GeneratedNote;

/// <summary>
/// Note saying that the wagons of a <see cref="ScheduledObject"/> are unloaded where the train arrives
/// and, where known, which location they are sent back to: the one they were last loaded at in their
/// schedule. A different train may bring them back.
/// </summary>
public sealed record UnloadWagonsNote(ScheduledObject ScheduledObject, OperationLocation? ReturnTo) : GeneratedNote;
