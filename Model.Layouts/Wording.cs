using System.Text;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Joins the parts of a name, such as standard, scale and designation, without saying a word twice: standard
/// "FREMO/H0-RE", scale "H0" and designation "FREMO H0 B96L" give "FREMO/H0-RE B96L". Every system names things alike
/// with it: the display name of a <c>Module</c>, and Module Draw's title of an end profile.
/// </summary>
public static class Wording
{
    /// <summary>
    /// The parts joined by spaces, leaving out empty parts and every word of a part that an earlier part already has.
    /// The first part that has text is kept as it is. A word is a run of letters and digits; words are compared
    /// ignoring case. A kept word keeps the separator before it, e.g. "-" in "B96-L"; the first kept word of a part
    /// has none. A part with nothing left is left out.
    /// </summary>
    public static string JoinWithoutRepeatedWords(params IEnumerable<string?> parts)
    {
        ArgumentNullException.ThrowIfNull(parts);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var kept = new List<string>();
        foreach (var part in parts.Select(p => p?.Trim()).Where(p => !string.IsNullOrEmpty(p)))
        {
            var text = kept.Count == 0 ? Seed(part!, seen) : WithoutSeen(part!, seen);
            if (text.Length > 0) kept.Add(text);
        }
        return string.Join(' ', kept);
    }

    /// <summary><paramref name="text"/> as it is, with its words added to <paramref name="seen"/>.</summary>
    private static string Seed(string text, HashSet<string> seen)
    {
        foreach (var (_, word) in Words(text)) seen.Add(word);
        return text;
    }

    /// <summary><paramref name="text"/> without the words in <paramref name="seen"/>, which then gets its other words.</summary>
    private static string WithoutSeen(string text, HashSet<string> seen)
    {
        var kept = new StringBuilder();
        foreach (var (separator, word) in Words(text))
        {
            if (!seen.Add(word)) continue;
            if (kept.Length > 0) kept.Append(separator.Length > 0 ? separator : " ");
            kept.Append(word);
        }
        return kept.ToString();
    }

    /// <summary>The runs of letters and digits in <paramref name="text"/>, each with the separator before it, trimmed.</summary>
    private static IEnumerable<(string Separator, string Word)> Words(string text)
    {
        var i = 0;
        while (i < text.Length)
        {
            var start = i;
            while (i < text.Length && !char.IsLetterOrDigit(text[i])) i++;
            var separator = text[start..i].Trim();
            if (separator.Length == 0 && i > start) separator = " ";
            var wordStart = i;
            while (i < text.Length && char.IsLetterOrDigit(text[i])) i++;
            if (i > wordStart) yield return (separator, text[wordStart..i]);
        }
    }
}
