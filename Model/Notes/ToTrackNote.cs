namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note saying to put the <see cref="ScheduledObject"/> on the <see cref="Track"/> after the train has
/// arrived at another track. Derived from <see cref="ScheduledTrainPart.ToTrack"/>.
/// </summary>
/// <remarks>
/// Putting the vehicle on another track takes it off the train, so where this note is given the part says
/// nothing more about uncoupling it or driving it to parking. Like every arrival note it needs no "after
/// arrival": that is where it is printed.
/// <para>
/// For a wagonset only the dispatcher reads it; the loco driver reads a
/// <see cref="ShuntWagonsToArrivalTrackNote"/> instead.
/// </para>
/// </remarks>
/// <param name="ScheduledObject">The vehicle to put away.</param>
/// <param name="Track">The track to put it on.</param>
public sealed record ToTrackNote(ScheduledObject ScheduledObject, StationTrack Track) : GeneratedNote;
