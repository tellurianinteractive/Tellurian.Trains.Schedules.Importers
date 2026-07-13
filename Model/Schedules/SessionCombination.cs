namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// A distinct set of sessions/days a vehicle works, together with the train parts it works on them.
/// Each combination is one turnus card: on every session/day in <see cref="Sessions"/> the vehicle
/// works exactly <see cref="Parts"/>, in departure order.
/// </summary>
/// <param name="Sessions">The sessions/days on which the vehicle works this exact set of parts.</param>
/// <param name="Parts">The parts worked on those sessions, in the order the vehicle works them (by departure).</param>
public sealed record SessionCombination(Sessions Sessions, IReadOnlyList<ScheduledTrainPart> Parts);
