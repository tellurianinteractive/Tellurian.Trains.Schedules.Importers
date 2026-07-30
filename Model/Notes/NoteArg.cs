using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Components;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// One value substituted into a note's format string, carrying both the plain-text and the markup
/// rendering of that value.
/// </summary>
/// <remarks>
/// <para>
/// A note's arguments are the parts of it that vary — the vehicle to fetch, the train to reinforce, the
/// station to bring wagons to — which is exactly what the reader must not miss. The markup form
/// therefore emphasises them by default, so "placeholder data prints bold" is a property of the
/// renderer rather than a decision taken again in every note.
/// </para>
/// <para>
/// Three kinds cover every argument the notes have:
/// <list type="bullet">
/// <item>an ordinary value — HTML-encoded and emphasised; vehicles, trains, stations;</item>
/// <item><see cref="Plain(object?)"/> — encoded but not emphasised; counts, positions and times, which
/// would dilute the emphasis where they stand beside a name;</item>
/// <item><see cref="Markup(string, string)"/> — passed through untouched; region chips, session circles
/// and other values that already carry a visual form of their own.</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Text">The value as plain text.</param>
/// <param name="Html">The value as markup, ready to be substituted into a note's markup form.</param>
internal readonly record struct NoteArg(string Text, string Html)
{
    /// <summary>
    /// The markup element wrapping an emphasised value. A <c>b</c> element, not a styled <c>span</c>,
    /// so the emphasis survives a consumer that has no stylesheet — the model is a published package —
    /// while the class stays as the single hook for tuning it from CSS.
    /// </summary>
    private const string EmphasisOpen = """<b class="value">""";
    private const string EmphasisClose = "</b>";

    /// <summary>
    /// A value rendered as it is: encoded and emphasised.
    /// </summary>
    public static NoteArg Emphasised(object? value)
    {
        var text = Render(value);
        return new(text, text.HasValue ? $"{EmphasisOpen}{WebUtility.HtmlEncode(text)}{EmphasisClose}" : string.Empty);
    }

    /// <summary>
    /// A value that is encoded but not emphasised — used where emphasis would say nothing, such as a
    /// count, a position in the train or a time the reader also has in the time column.
    /// </summary>
    public static NoteArg Plain(object? value)
    {
        var text = Render(value);
        return new(text, WebUtility.HtmlEncode(text));
    }

    /// <summary>
    /// A value that renders itself, given in both forms. Its markup is substituted verbatim, so it must
    /// already be encoded by whatever produced it.
    /// </summary>
    public static NoteArg Markup(string text, string html) => new(text, html);

    /// <inheritdoc cref="Markup(string, string)"/>
    public static NoteArg Markup(string text, MarkupString html) => new(text, html.Value);

    // The UI culture selects the note's text (the resource lookup); the formatting culture renders the
    // values — the same split NoteText documents.
    private static string Render(object? value) => value switch
    {
        null => string.Empty,
        IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentCulture),
        _ => value.ToString() ?? string.Empty,
    };
}
