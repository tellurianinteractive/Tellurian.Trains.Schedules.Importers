using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// What a note says, described once: a localised format string from <see cref="Resources.Notes"/> and
/// the values substituted into it. <see cref="ToText"/> and <see cref="ToHtml"/> are two renderings of
/// that one description.
/// </summary>
/// <remarks>
/// <para>
/// Describing the note once is what keeps its two forms in step. The alternative — a text switch beside
/// a markup switch, one arm per note in each — is two edits for every new note with nothing requiring
/// the second, and the forms drift apart the first time one is forgotten.
/// </para>
/// <para>
/// An argument that is not already a <see cref="NoteArg"/> is emphasised in the markup form, so a note
/// gets bold placeholders by writing nothing: the substituted values are the placeholders. Pass
/// <see cref="NoteArg.Plain(object?)"/> or <see cref="NoteArg.Markup(string, string)"/> to opt out.
/// </para>
/// <para>
/// Arguments are rendered to strings before the format is applied, so a note resource must use bare
/// placeholders (<c>{0}</c>) and not composite format specifiers (<c>{0:N0}</c>) — values that need
/// formatting are formatted by the caller, as the meet times are.
/// </para>
/// </remarks>
/// <param name="Format">The localised format string.</param>
/// <param name="Args">The values substituted into it.</param>
internal readonly record struct NoteTemplate(string Format, params object?[] Args)
{
    /// <summary>
    /// The note as plain text.
    /// </summary>
    public string ToText => Render(arg => arg.Text);

    /// <summary>
    /// The note as markup, with the substituted values emphasised.
    /// </summary>
    public string ToHtml => Render(arg => arg.Html);

    private string Render(Func<NoteArg, string> form) =>
        string.Format(CultureInfo.CurrentCulture, Format, [.. Args.Select(arg => form(Of(arg)))]);

    private static NoteArg Of(object? value) => value is NoteArg arg ? arg : NoteArg.Emphasised(value);
}
