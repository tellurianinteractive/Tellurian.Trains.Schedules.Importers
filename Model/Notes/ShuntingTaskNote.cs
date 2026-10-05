namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note saying the row is a shunting task and when its work starts and ends.
/// </summary>
/// <remarks>
/// A task's one call is both its origin and its destination, so the arrival and departure columns of a
/// station's dispatch list — which leave those out — stand empty, and nothing else on the row says that
/// it is work at the station rather than a train running. The note says both. It is for the station
/// only: the duty booklet gives a task its own block with the start and end times already in it.
/// </remarks>
/// <param name="Starts">The time the work starts.</param>
/// <param name="Ends">The time the work ends.</param>
public sealed record ShuntingTaskNote(Time Starts, Time Ends) : GeneratedNote;
