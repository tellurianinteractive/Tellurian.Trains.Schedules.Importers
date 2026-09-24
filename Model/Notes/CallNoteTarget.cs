namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Which half of a station call a note belongs to. A call is two events — the train arrives, and the
/// train departs — and the reports print them as separate rows, so a note has to say which of the two
/// it was written for.
/// </summary>
/// <remarks>
/// The generated notes the model derives are stamped with their half where they are created. This is
/// what a planner chooses for a manual note, where the call itself is the only guide: see
/// <see cref="ManualNoteRules"/> for which halves a call offers.
/// </remarks>
public enum CallNoteTarget
{
    /// <summary>The note is for the arrival: what the train meets, or is to do, on pulling in.</summary>
    Arrival,

    /// <summary>The note is for the departure: what is to be done before, or on, leaving.</summary>
    Departure,
}
