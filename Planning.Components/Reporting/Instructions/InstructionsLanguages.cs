using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Instructions;

/// <summary>
/// Recognises the language tags an author may write in the programme and the general instructions, and
/// picks out the text for one language.
/// </summary>
/// <remarks>
/// <para>
/// A passage written in several languages is written once per language, each version enclosed in a tag
/// named by its two-letter ISO language code:
/// </para>
/// <code>
/// [sv]Svensk text[/sv][nb]Norsk tekst[/nb]
/// </code>
/// <para>
/// Versions that follow one another with nothing but white space between them are alternatives of the
/// same passage. A version may span lines and hold any markdown, placeholders included. Text outside any
/// tag is common to every language.
/// </para>
/// <para>
/// The booklet is printed once per language: the layout's default language first, then every other
/// language tagged in the text that the application can print in. Each copy carries the common text and,
/// for each passage, the version in its own language — or, when the passage has none, the version in
/// the default language, or nothing when it has neither. A text without tags prints one booklet, in the
/// default language, exactly as before.
/// </para>
/// <para>
/// Like the placeholders, the tags are taken out of the source before Markdig sees it. They are written
/// in square brackets rather than as HTML elements on purpose: an angle-bracket tag that reaches the
/// browser — in any view that does not resolve it, or when it is misspelt — is an unknown element that
/// swallows its text or upsets the rest of the page, whereas a square-bracket tag is plain text that
/// shows as written. Nor can it be confused with the HTML element names of two letters
/// (<c>&lt;em&gt;</c>, <c>&lt;tr&gt;</c> and the like).
/// </para>
/// </remarks>
public static partial class InstructionsLanguages
{
    [GeneratedRegex(
        @"\[(?<language>[a-z]{2})\](?<text>.*?)\[/\k<language>\]",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex TagPattern();

    /// <summary>One version of a passage: its language and its text, without the tags.</summary>
    private sealed record Version(string Language, string Text, int Start, int End);

    /// <summary>
    /// The languages tagged in the texts, in lower case and in the order they first appear.
    /// </summary>
    public static IReadOnlyList<string> LanguagesIn(params string?[] texts) =>
        [.. texts.SelectMany(text => VersionsIn(text)).Select(v => v.Language).Distinct()];

    /// <summary>
    /// The languages to print the booklet in: <paramref name="defaultLanguage"/> first, then every other
    /// language tagged in the texts that <paramref name="isAvailable"/> accepts.
    /// </summary>
    /// <param name="defaultLanguage">The layout's default language.</param>
    /// <param name="isAvailable">Whether reports can be printed in a two-letter language.</param>
    /// <param name="texts">The authored texts the booklet prints.</param>
    public static IReadOnlyList<string> BookletLanguages(string defaultLanguage, Func<string, bool> isAvailable, params string?[] texts)
    {
        var defaultCode = defaultLanguage.ToLowerInvariant();
        return [defaultCode, .. LanguagesIn(texts).Where(l => l != defaultCode && isAvailable(l))];
    }

    /// <summary>
    /// The text as printed in <paramref name="language"/>: the common text, and for each passage its
    /// version in that language, else in <paramref name="defaultLanguage"/>, else nothing.
    /// </summary>
    public static string Resolve(string? markdown, string language, string defaultLanguage)
    {
        if (string.IsNullOrEmpty(markdown)) return "";

        var result = new StringBuilder();
        var position = 0;
        foreach (var passage in PassagesIn(markdown))
        {
            result.Append(markdown, position, passage[0].Start - position);
            var chosen =
                passage.FirstOrDefault(v => v.Language.Equals(language, StringComparison.OrdinalIgnoreCase)) ??
                passage.FirstOrDefault(v => v.Language.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase));
            if (chosen is not null) result.Append(chosen.Text);
            position = passage[^1].End;
        }
        result.Append(markdown, position, markdown.Length - position);
        return result.ToString();
    }

    /// <summary>
    /// The markdown with every version labelled by its language, for the preview beside the text area:
    /// the author sees every version, and sees that each tag was recognised.
    /// </summary>
    /// <remarks>
    /// A version on one line gets an inline label, so a passage written within a sentence stays within
    /// it; a version spanning lines gets a label of its own above it, since it may start with a heading
    /// or a list that an inline label would break.
    /// </remarks>
    public static string ToPreviewMarkdown(string? markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return "";

        return TagPattern().Replace(markdown, match =>
        {
            var language = match.Groups["language"].Value.ToUpperInvariant();
            var text = match.Groups["text"].Value.Trim();
            return text.Contains('\n')
                ? $"\n\n<p class=\"report-language\">{WebUtility.HtmlEncode(language)}</p>\n\n{text}\n\n"
                : $"<span class=\"report-language\">{WebUtility.HtmlEncode(language)}</span> {text}";
        });
    }

    private static IEnumerable<Version> VersionsIn(string? markdown) =>
        string.IsNullOrEmpty(markdown)
            ? []
            : TagPattern().Matches(markdown).Select(m => new Version(
                m.Groups["language"].Value.ToLowerInvariant(), m.Groups["text"].Value.Trim(), m.Index, m.Index + m.Length));

    // Versions separated by nothing but white space are alternatives of one passage.
    private static IEnumerable<IReadOnlyList<Version>> PassagesIn(string markdown)
    {
        var passage = new List<Version>();
        foreach (var version in VersionsIn(markdown))
        {
            if (passage.Count > 0 && !string.IsNullOrWhiteSpace(markdown[passage[^1].End..version.Start]))
            {
                yield return passage;
                passage = [];
            }
            passage.Add(version);
        }
        if (passage.Count > 0) yield return passage;
    }
}
