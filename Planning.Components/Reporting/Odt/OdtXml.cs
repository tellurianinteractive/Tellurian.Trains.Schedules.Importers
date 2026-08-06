using System.Text;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Odt;

/// <summary>
/// Turns plain strings into OpenDocument XML text content.
/// </summary>
/// <remarks>
/// <para>
/// The documents here are generated from a fixed template, so the XML is written as text rather than
/// through <c>XmlWriter</c>: it keeps <c>System.Private.Xml</c> out of a WebAssembly payload that is
/// downloaded before the app starts, and it lets the styles and the page layout be read as a stylesheet
/// instead of as a stream of method calls. What that trades away is the writer's guarantee of
/// well-formedness, which is bought back by the tests — they parse every generated part.
/// </para>
/// <para>
/// Escaping is therefore the one place a value from the model meets the markup, and it has to hold for
/// values nobody anticipated: a station named <c>Ny &amp; Gammel</c>, a note carrying a quotation mark.
/// </para>
/// </remarks>
public static class OdtXml
{
    /// <summary>
    /// Escapes a value for use in an attribute or as element content.
    /// </summary>
    /// <remarks>
    /// Quotes are escaped as well as the three characters that strictly need it, so one routine is safe in
    /// both positions — an attribute value is where an unescaped quote ends the document.
    /// </remarks>
    /// <param name="text">The value to escape; <c>null</c> gives the empty string.</param>
    public static string Escape(string? text)
    {
        if (!text.HasValue) return "";

        var result = new StringBuilder(text!.Length + 8);
        foreach (var character in text)
        {
            _ = character switch
            {
                '&' => result.Append("&amp;"),
                '<' => result.Append("&lt;"),
                '>' => result.Append("&gt;"),
                '"' => result.Append("&quot;"),
                '\'' => result.Append("&apos;"),
                _ => result.Append(character),
            };
        }
        return result.ToString();
    }

    /// <summary>
    /// Escapes a value as the text content of a paragraph or span, preserving its spacing.
    /// </summary>
    /// <remarks>
    /// XML collapses whitespace, so ODF carries it in elements of its own: a run of spaces is
    /// <c>text:s</c>, a tab is <c>text:tab</c> and a line break is <c>text:line-break</c>. Without this a
    /// note written with aligned columns arrives as one run of single spaces — and a note that is nothing
    /// but spaces arrives as nothing at all.
    /// </remarks>
    /// <param name="text">The value to render; <c>null</c> gives the empty string.</param>
    public static string Content(string? text)
    {
        if (!text.HasValue) return "";

        var result = new StringBuilder(text!.Length + 8);
        for (var i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '\t':
                    result.Append("<text:tab/>");
                    break;
                case '\r':
                    // A CRLF is one break, not two: the LF that follows falls through to the case below.
                    if (i + 1 < text.Length && text[i + 1] == '\n') break;
                    result.Append("<text:line-break/>");
                    break;
                case '\n':
                    result.Append("<text:line-break/>");
                    break;
                case ' ':
                    // The first space of a run is an ordinary one; only the rest need the element, and a
                    // space at the very start of a paragraph needs it too or it is dropped.
                    var run = 0;
                    while (i + run + 1 < text.Length && text[i + run + 1] == ' ') run++;
                    if (result.Length == 0)
                        result.Append(run == 0 ? "<text:s/>" : $"""<text:s text:c="{run + 1}"/>""");
                    else
                        result.Append(run == 0 ? " " : $""" <text:s text:c="{run}"/>""");
                    i += run;
                    break;
                default:
                    result.Append(Escape(text[i].ToString()));
                    break;
            }
        }
        return result.ToString();
    }

    /// <summary>A paragraph of plain text in the given style.</summary>
    /// <param name="styleName">The name of a paragraph style defined in <c>styles.xml</c>.</param>
    /// <param name="text">The paragraph's text; an empty paragraph when <c>null</c> or blank.</param>
    public static string Paragraph(string styleName, string? text) =>
        $"""<text:p text:style-name="{Escape(styleName)}">{Content(text)}</text:p>""";

    /// <summary>A run of plain text in the given text style, for use inside a paragraph.</summary>
    /// <param name="styleName">The name of a text style defined in <c>styles.xml</c>.</param>
    /// <param name="text">The run's text.</param>
    public static string Span(string styleName, string? text) =>
        $"""<text:span text:style-name="{Escape(styleName)}">{Content(text)}</text:span>""";
}
