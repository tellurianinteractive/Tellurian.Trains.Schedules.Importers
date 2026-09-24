namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note saying to fetch the <see cref="ScheduledObject"/> from the <see cref="Track"/> it stands on,
/// before the train departs from another track. Derived from <see cref="ScheduledTrainPart.FromTrack"/>.
/// </summary>
/// <remarks>
/// Fetching the vehicle is what brings it to the train, so where this note is given the part says nothing
/// more about using it, coupling it or bringing it from parking.
/// <para>
/// For a wagonset only the dispatcher reads it; the loco driver reads a
/// <see cref="ShuntWagonsToDepartureTrackNote"/> instead.
/// </para>
/// </remarks>
/// <param name="ScheduledObject">The vehicle to fetch.</param>
/// <param name="Track">The track it stands on.</param>
public sealed record FromTrackNote(ScheduledObject ScheduledObject, StationTrack Track) : GeneratedNote;
