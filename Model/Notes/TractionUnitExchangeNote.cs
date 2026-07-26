namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Derived driver note: the train keeps running while its traction unit is exchanged at a station. Produced
/// for a driver duty where two consecutive same-train parts are worked by different traction units.
/// The note belongs at <paramref name="At"/>,
/// the station where the earlier part arrives and the later part departs.
/// </summary>
/// <param name="At">The station where the traction unit is exchanged.</param>
/// <param name="From">The traction unit worked up to the station.</param>
/// <param name="To">The traction unit worked onward from the station.</param>
public sealed record TractionUnitExchangeNote(OperationLocation At, ScheduledObject From, ScheduledObject To) : GeneratedNote;
