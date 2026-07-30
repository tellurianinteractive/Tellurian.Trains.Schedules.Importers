using System.Net;
using System.Text;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// The two Markdown emphases a planner may use in a manual note — <c>*italic*</c> and
/// <c>**bold**</c> — and nothing else.
/// </summary>
/// <remarks>
/// <para>
/// A manual note is free text a planner typed, so it cannot be trusted as markup: it is HTML-encoded
/// first and the emphasis elements are added afterwards. That rules out a general Markdown library even
/// if one were wanted — the point here is not to render documents but to let a planner stress the one
/// word in a note that matters, in a form they can also read in the box they typed it in.
/// </para>
/// <para>
/// The rules are deliberately few, and every one of them fails softly — an input this does not
/// understand prints as the planner typed it, never mangled:
/// <list type="bullet">
/// <item><c>*text*</c> renders italic and <c>**text**</c> renders bold.</item>
/// <item>They nest — <c>**bold with *italic* inside**</c> — as long as the inner markers do not sit
/// flush against the outer ones. Where they do (<c>**bold *italic***</c>), the run of three is
/// ambiguous: telling the italic closer from the bold one takes the delimiter stack of a real Markdown
/// parser, so instead a run of three or more markers is left as literal text. Put a space or a word
/// between the two closers and both render.</item>
/// <item>An asterisk with no partner is just an asterisk — a note reading "2*3 wagons" is safe.</item>
/// <item><c>\*</c> is a literal asterisk, for the note that needs one next to another one.</item>
/// <item>Underscores are not emphasis. They appear inside identifiers often enough that treating them
/// as markup would corrupt more notes than it decorates.</item>
/// </list>
/// </para>
/// <para>
/// Both forms come from one pass over the same rules, so the plain text a printed booklet measures and
/// the markup it renders always agree on where a note begins and ends: <see cref="ToText"/> drops the
/// emphasis, <see cref="ToHtml"/> renders it.
/// </para>
/// <para>
/// Public because an editor needs the same rendering the note itself uses: a field that shows formatted
/// text until it is entered has to render text that is not yet a note. Neither method adds the
/// <c>callnote</c> span — that belongs to the note, and a caller rendering loose text supplies its own
/// container.
/// </para>
/// </remarks>
public static class NoteMarkdown
{
    private const char Marker = '*';
    private const char Escape = '\\';

    /// <summary>
    /// The note without its emphasis markers — what a plain-text reader, and the printed booklet's
    /// line-count estimate, should see.
    /// </summary>
    public static string ToText(string? markdown) => Render(markdown, html: false);

    /// <summary>
    /// The note as encoded markup, with its emphasis rendered as <c>b</c> and <c>i</c> elements.
    /// </summary>
    public static string ToHtml(string? markdown) => Render(markdown, html: true);

    private static string Render(string? markdown, bool html)
    {
        if (markdown is null || markdown.Length == 0) return string.Empty;
        var builder = new StringBuilder(markdown.Length);
        Append(markdown, builder, html);
        return builder.ToString();
    }

    // Walks the span once, accumulating ordinary characters into a literal run and flushing it whenever
    // something needs emitting between them. Emphasis recurses on its own content, which is what makes
    // bold-inside-italic (and the reverse) work without a delimiter stack.
    private static void Append(ReadOnlySpan<char> markdown, StringBuilder builder, bool html)
    {
        var literalStart = 0;
        var index = 0;
        while (index < markdown.Length)
        {
            if (markdown[index] == Escape && index + 1 < markdown.Length && IsEscapable(markdown[index + 1]))
            {
                AppendLiteral(markdown[literalStart..index], builder, html);
                AppendLiteral(markdown.Slice(index + 1, 1), builder, html);
                index += 2;
                literalStart = index;
                continue;
            }

            if (markdown[index] != Marker)
            {
                index++;
                continue;
            }

            // Two markers open bold, one opens italic; a longer run matches nothing and stays literal.
            var markers = RunLength(markdown, index);
            var close = markers > 2 ? -1 : IndexOfClosing(markdown, index + markers, markers);
            if (close < 0)
            {
                // Unmatched: leave the whole run in the literal run so it prints as the planner typed it.
                index += markers;
                continue;
            }

            AppendLiteral(markdown[literalStart..index], builder, html);
            var element = markers == 2 ? "b" : "i";
            if (html) builder.Append('<').Append(element).Append('>');
            Append(markdown[(index + markers)..close], builder, html);
            if (html) builder.Append("</").Append(element).Append('>');
            index = close + markers;
            literalStart = index;
        }
        AppendLiteral(markdown[literalStart..], builder, html);
    }

    private static void AppendLiteral(ReadOnlySpan<char> literal, StringBuilder builder, bool html)
    {
        if (literal.Length == 0) return;
        builder.Append(html ? WebUtility.HtmlEncode(literal.ToString()) : literal);
    }

    // Where the run of markers closing an emphasis of this length starts, or -1 when there is none.
    // The content must be non-empty: "**" on its own opens nothing.
    private static int IndexOfClosing(ReadOnlySpan<char> markdown, int from, int markers)
    {
        for (var index = from; index < markdown.Length; index++)
        {
            if (markdown[index] == Escape) { index++; continue; }
            if (markdown[index] != Marker) continue;
            var run = RunLength(markdown, index);
            if (run == markers && index > from) return index;
            index += run - 1;
        }
        return -1;
    }

    private static int RunLength(ReadOnlySpan<char> markdown, int from)
    {
        var length = 1;
        while (from + length < markdown.Length && markdown[from + length] == Marker) length++;
        return length;
    }

    private static bool IsEscapable(char character) => character is Marker or Escape;
}
