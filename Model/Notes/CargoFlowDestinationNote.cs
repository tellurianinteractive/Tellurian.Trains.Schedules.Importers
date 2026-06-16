namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note listing the destinations to which the train brings wagons from here.</summary>
public sealed record CargoFlowDestinationNote(ScheduledObject ScheduledObject, TrainPart Part) : GeneratedNote;
