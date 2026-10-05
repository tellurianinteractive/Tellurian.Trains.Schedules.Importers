using System.Net;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Instructions;

/// <summary>A report part the author may place inside their own instructions text.</summary>
public enum InstructionsPlaceholder
{
    /// <summary>The layout's shunting yards table, written <c>&lt;ShuntingYards/&gt;</c>.</summary>
    ShuntingYards,

    /// <summary>
    /// The stations where passengers change trains, written <c>&lt;Interchanges/&gt;</c>. Unlike the
    /// shunting yards, this list is printed nowhere else: a booklet that does not place it does not
    /// carry it.
    /// </summary>
    Interchanges,
}

/// <summary>One stretch of a page's content: either authored markdown or a placeholder standing for a
/// report part rendered by the app.</summary>
/// <param name="Markdown">The authored markdown, empty when this segment is a placeholder.</param>
/// <param name="Placeholder">The part to render here, null when this segment is markdown.</param>
public sealed record InstructionsSegment(string Markdown = "", InstructionsPlaceholder? Placeholder = null);

/// <summary>
/// Recognises the placeholder tags an author may write in the general instructions, and splits the
/// text around them.
/// </summary>
/// <remarks>
/// <para>
/// A placeholder lets the author decide where a generated part of the booklet goes. Without one, the
/// shunting yards table can only sit on the layout page at the back, which it shares with the topology
/// diagram and the cargo key — a page that overflows as soon as a layout has more than a handful of
/// shunting yards, and prints short without a word, because the page body is <c>overflow: hidden</c>.
/// </para>
/// <para>
/// The tag is written as an empty HTML element on a line of its own:
/// </para>
/// <code>
/// ## Where the wagons go
///
/// &lt;ShuntingYards/&gt;
/// </code>
/// <para>
/// It is deliberately split out of the markdown <em>before</em> Markdig sees it, rather than being
/// left to survive the conversion and picked out of the HTML afterwards. Markdig would pass it through
/// as raw HTML — an unknown element the browser then drops silently — and cutting a rendered HTML
/// string apart risks splitting it inside a paragraph the author started on the line above. Splitting
/// the source means every stretch either side is a whole markdown document in its own right, and the
/// placeholder can never end up nested in someone else's element.
/// </para>
/// <para>
/// Matching is on a line that holds nothing but the tag: leading and trailing whitespace, the tag name
/// in any casing, and an optional space before the slash. A tag written inside a sentence is left
/// alone — it is not a marker the author can place there meaningfully, since the part it stands for is
/// a block.
/// </para>
/// </remarks>
public static class InstructionsPlaceholders
{
    /// <summary>The tag an author writes for each placeholder, without its angle brackets or slash.</summary>
    private static readonly (InstructionsPlaceholder Placeholder, string Tag)[] Tags =
    [
        (InstructionsPlaceholder.ShuntingYards, "ShuntingYards"),
        (InstructionsPlaceholder.Interchanges, "Interchanges"),
    ];

    /// <summary>The tag for a placeholder, exactly as an author writes it.</summary>
    public static string TagOf(InstructionsPlaceholder placeholder) =>
        $"<{Tags.First(t => t.Placeholder == placeholder).Tag}/>";

    /// <summary>
    /// The placeholder a line stands for, or null if the line is ordinary text. A line counts only when
    /// it holds the tag and nothing else.
    /// </summary>
    public static InstructionsPlaceholder? PlaceholderOf(string line)
    {
        var text = line.Trim();
        if (text.Length < 3 || text[0] != '<' || !text.EndsWith("/>", StringComparison.Ordinal)) return null;

        var name = text[1..^2].TrimEnd();
        foreach (var (placeholder, tag) in Tags)
            if (name.Equals(tag, StringComparison.OrdinalIgnoreCase)) return placeholder;
        return null;
    }

    /// <summary>Whether <paramref name="markdown"/> places the given part itself.</summary>
    /// <remarks>
    /// What the layout page asks before printing the same part at the back: a part the author has
    /// placed belongs where they put it, and nowhere else in the same booklet.
    /// </remarks>
    public static bool Places(string? markdown, InstructionsPlaceholder placeholder) =>
        markdown is not null &&
        markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n')
            .Any(line => PlaceholderOf(line) == placeholder);

    /// <summary>
    /// The markdown with every recognised placeholder replaced by a named box, for the preview beside
    /// the text area the author is typing in.
    /// </summary>
    /// <remarks>
    /// The preview renders the markdown and nothing else, so a placeholder left as it stands shows up
    /// as an unknown element, which a browser drops without trace — the author would see their tag
    /// vanish and have no way to tell a recognised tag from a misspelt one but to print the booklet.
    /// The box says both that the tag was understood and which part it stands for.
    /// </remarks>
    /// <param name="markdown">The text as written.</param>
    /// <param name="name">What to call each part, normally the same translated label the print uses.</param>
    public static string ToPreviewMarkdown(string? markdown, Func<InstructionsPlaceholder, string> name)
    {
        if (string.IsNullOrEmpty(markdown)) return "";

        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (var i = 0; i < lines.Length; i++)
            if (PlaceholderOf(lines[i]) is { } placeholder)
                lines[i] = $"<p class=\"report-placeholder\">{WebUtility.HtmlEncode(name(placeholder))}</p>";
        return string.Join("\n", lines);
    }

    /// <summary>
    /// Splits authored markdown into the stretches of text and the placeholders between them, in the
    /// order they were written. Markdown with no placeholder in it yields itself, unchanged.
    /// </summary>
    public static IReadOnlyList<InstructionsSegment> Split(string? markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return [];

        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var segments = new List<InstructionsSegment>();
        var text = new List<string>();

        foreach (var line in lines)
        {
            if (PlaceholderOf(line) is not { } placeholder) { text.Add(line); continue; }
            Flush();
            segments.Add(new InstructionsSegment(Placeholder: placeholder));
        }
        Flush();
        return segments;

        void Flush()
        {
            var joined = string.Join("\n", text).Trim();
            text.Clear();
            if (joined.Length > 0) segments.Add(new InstructionsSegment(joined));
        }
    }
}
