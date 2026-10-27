namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note telling the station which <see cref="ScheduledObject">wagonset</see> its train carries, with the remark
/// the owner gave it. It is for the station only: the loco driver does not handle wagonsets the train simply runs with.
/// </summary>
public sealed record WagonsetRemarkNote(ScheduledObject ScheduledObject) : GeneratedNote;
