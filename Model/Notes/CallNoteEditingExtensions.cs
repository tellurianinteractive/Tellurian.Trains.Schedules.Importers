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
        /// Writes the manual note of this call, creating it when there is none and removing it when the
        /// text is cleared — so an emptied field leaves no blank note behind to occupy a row in a
        /// printed booklet.
        /// </summary>
        /// <param name="text">The new text, Markdown emphasis included. Null or blank removes the note.</param>
        /// <param name="languageCode">The language written, or <c>null</c> for the reader's current
        /// language. See <see cref="TextCallNote.SetText(string, string?)"/>.</param>
        public void SetManualNote(string? text, string? languageCode = null)
        {
            call = call.ValueOrException(nameof(call));
            var note = call.ManualNote;
            if (!text.HasValue)
            {
                if (note is not null) call.Notes.Remove(note);
                return;
            }
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
            // Written through SetText even when just created, so the language a note is stored under is
            // resolved in one place and cannot disagree with the language it is read back in.
            note.SetText(text, languageCode);
        }
    }
}
