namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>
/// An assumption or problem in an auto-draw, shown to the user, e.g. "exits inferred: 2 × FREMO-H0-B96".
/// The message is in English; UIs can localise by <see cref="Code"/>.
/// </summary>
public sealed record AutoDrawNote(AutoDrawNoteCode Code, string Message)
{
    /// <inheritdoc/>
    public override string ToString() => Message;
}
