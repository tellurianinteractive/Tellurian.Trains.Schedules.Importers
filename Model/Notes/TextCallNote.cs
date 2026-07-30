using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Represents a text-based note associated with a station call — the manual note a planner writes, as
/// opposed to the <see cref="GeneratedNote"/> family the model derives.
/// </summary>
/// <remarks>
/// The text may carry the two Markdown emphases <see cref="NoteMarkdown"/> supports, so a planner can
/// stress the part of a note that matters.
/// </remarks>
public class TextCallNote : CallNote
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private TextCallNote() => DisplayOrder = 1000;

    /// <summary>
    /// Initializes a new instance of <see cref="TextCallNote"/> with the specified text.
    /// </summary>
    /// <param name="text">The note text.</param>
    /// <param name="languageCode"></param>
    /// <param name="displayOrder"></param>
    public TextCallNote(string text, string languageCode, int displayOrder = 1000)
    {
        Texts.Add(new LocalizedText(text, languageCode));
        DisplayOrder = displayOrder;
    }
    [JsonInclude]
    private List<LocalizedText> Texts { get; set; } = [];
    [JsonInclude]
    private static string CurrentLanguageCode => System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    /// <summary>
    /// The note exactly as it is stored, in the current UI language — Markdown emphasis included.
    /// </summary>
    /// <remarks>
    /// Falls back to the first available text and finally to an empty string. Must never throw: it is
    /// read during JSON serialization, so an index/Single access on an empty or multi-entry
    /// <see cref="Texts"/> list would break persistence. This is the value an editor binds to; the two
    /// rendered forms are <see cref="ToText"/> and <see cref="ToHtml"/>.
    /// </remarks>
    [JsonIgnore]
    public string Text =>
        Texts.FirstOrDefault(t => t.LanguageCode.Equals(CurrentLanguageCode, StringComparison.OrdinalIgnoreCase))?.Text
        ?? Texts.FirstOrDefault()?.Text
        ?? string.Empty;

    /// <inheritdoc/>
    /// <remarks>
    /// The stored text with its emphasis markers removed — what a plain-text reader should see, and
    /// what the printed booklet must measure, since the markers occupy no width on the page.
    /// </remarks>
    public override string ToText => NoteMarkdown.ToText(Text);

    /// <inheritdoc/>
    /// <remarks>
    /// A planner may stress part of a manual note with <c>*italic*</c> or <c>**bold**</c>; everything
    /// else in the text is encoded, since it is free text and not markup. See <see cref="NoteMarkdown"/>.
    /// </remarks>
    public override MarkupString ToHtml =>
        new($"""<span class="callnote">{NoteMarkdown.ToHtml(Text)}</span>""");

    /// <summary>
    /// Replaces what the note says in one language, leaving its other translations alone.
    /// </summary>
    /// <param name="text">The new text, Markdown emphasis included.</param>
    /// <param name="languageCode">The language written, or <c>null</c> for the language the reader is
    /// currently in — which is what <see cref="Text"/> reads back, so writing and reading cannot
    /// disagree about which translation is being edited.</param>
    /// <remarks>
    /// A stored text with no language code is not a translation but text of unknown origin — the XPLN
    /// import leaves remarks that way — so an edit replaces it rather than leaving it behind to
    /// resurface for a reader in some other language.
    /// </remarks>
    public void SetText(string text, string? languageCode = null)
    {
        var code = languageCode ?? CurrentLanguageCode;
        Texts.RemoveAll(t => !t.LanguageCode.HasValue || t.LanguageCode.Equals(code, StringComparison.OrdinalIgnoreCase));
        Texts.Add(new LocalizedText(text, code));
    }
}
