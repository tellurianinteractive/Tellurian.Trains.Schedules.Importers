namespace Tellurian.Trains.Schedules.Planning.Components.Trains;

/// <summary>
/// What the call note field hands back when a note is written: the text, and the half of the call it is
/// for.
/// </summary>
/// <remarks>
/// The two travel together because neither is worth storing without the other. A half chosen before
/// anything is typed has no note to be written on yet, so it waits in the field until the text arrives
/// and both are stored in one go (see <c>StationCall.SetManualNote</c>).
/// </remarks>
/// <param name="Text">The note as typed, Markdown emphasis included. Blank removes the note.</param>
/// <param name="Target">The half of the call the note is for, or null to leave the choice to the call.</param>
public sealed record ManualNoteEdit(string Text, CallNoteTarget? Target);
