namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note saying the train runs only when it is called for, on the sessions it is booked for.</summary>
/// <remarks>
/// Carried as a note rather than inside the operating sessions/days value, where the words would sit in a
/// column a few millimetres wide and wrap over several lines to say something that is the same on every
/// row of that train. The sessions themselves stay in their column; this says how they are worked.
/// </remarks>
public sealed record OnDemandNote : GeneratedNote;
