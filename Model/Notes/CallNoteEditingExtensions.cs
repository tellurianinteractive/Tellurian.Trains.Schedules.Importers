namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Editing a call's manual note, so a caller never does collection surgery on
/// <see cref="StationCall.Notes"/> to write one.
/// </summary>
public static class CallNoteEditingExtensions
{
    extension(StationCall call)
    {
        /// <summary>
        /// The manual note of this call — the one a planner writes and edits — or <c>null</c> when the
        /// call has none.
        /// </summary>
        /// <remarks>
        /// A call can hold more than one <see cref="TextCallNote"/>: the XPLN import adds one per
        /// remark and further ones for the locomotive and trainset it read. This is the first of them
        /// in display order, which is the one an editing field shows; any others still render in
        /// reports and are left untouched.
        /// </remarks>
        public TextCallNote? ManualNote =>
            call.ValueOrException(nameof(call)).Notes.OfType<TextCallNote>().OrderBy(n => n.DisplayOrder).FirstOrDefault();

        /// <summary>
        /// What the manual note says, exactly as stored — Markdown emphasis included — or an empty
        /// string when there is no manual note. This is the value an editor binds to.
        /// </summary>
        public string ManualNoteText => call.ManualNote?.Text ?? string.Empty;

        /// <summary>
        /// The half of the call the manual note is for — the note's own where it has one, and otherwise
        /// the half a note written here would get. Null at a call that may carry no manual note.
        /// </summary>
        /// <remarks>
        /// This is the value the note field's arrival/departure selector binds to, so it never answers
        /// with a half the call no longer offers: a note written before its half was taken away shows,
        /// and on its next edit moves to, the half that is left. What the note itself still says is
        /// <c>CallNote.Target</c>.
        /// </remarks>
        public CallNoteTarget? ManualNoteTarget =>
            call.ValueOrException(nameof(call)).ManualNote?.Target is { } target && call.ManualNoteTargets.Contains(target)
                ? target
                : call.DefaultManualNoteTarget;

        /// <summary>
        /// Writes the manual note of this call, creating it when there is none and removing it when the
        /// text is cleared — so an emptied field leaves no blank note behind to occupy a row in a
        /// printed booklet.
        /// </summary>
        /// <param name="text">The new text, Markdown emphasis included. Null or blank removes the note.</param>
        /// <param name="target">The half of the call the note is for. Null takes the call's own default,
        /// and so does a half this call does not offer. See <see cref="ManualNoteRules"/>.</param>
        /// <param name="languageCode">The language written, or <c>null</c> for the reader's current
        /// language. See <see cref="TextCallNote.SetText(string, string?)"/>.</param>
        /// <remarks>
        /// Nothing is written at a call the train runs past: a note there belongs to neither half, and
        /// the reports that print the two apart would never show it. Clearing still works, so a note
        /// left over from when the call was a stop can be taken away.
        /// </remarks>
        public void SetManualNote(string? text, CallNoteTarget? target = null, string? languageCode = null)
        {
            call = call.ValueOrException(nameof(call));
            var note = call.ManualNote;
            if (!text.HasValue)
            {
                if (note is not null) call.Notes.Remove(note);
                return;
            }
            if (call.DefaultManualNoteTarget is not { } fallback) return;
            var half = target is { } wanted && call.ManualNoteTargets.Contains(wanted) ? wanted : fallback;
            if (note is null)
            {
                // A manual note is for everyone at the call until someone says otherwise: the audience
                // flags a planner would set are not asked for by a single field, and defaulting to
                // silence would make a typed note vanish from the reports it was typed for.
                note = new TextCallNote(string.Empty, string.Empty)
                {
                    IsDriverNote = true,
                    IsStationNote = true,
                    IsShuntingNote = true,
                };
                call.Notes.Add(note);
            }
            note.SetTarget(half);
            // Written through SetText even when just created, so the language a note is stored under is
            // resolved in one place and cannot disagree with the language it is read back in.
            note.SetText(text, languageCode);
        }
    }
}
