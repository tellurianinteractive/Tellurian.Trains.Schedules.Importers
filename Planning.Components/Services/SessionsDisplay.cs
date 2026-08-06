namespace Tellurian.Trains.Schedules.Planning.Components.Services;

/// <summary>
/// Identifies and parses <see cref="Sessions"/> patterns for the session catalogue drop-downs.
/// </summary>
/// <remarks>
/// Rendering a pattern for display is <c>Sessions.ToText</c> and <c>Sessions.ToHtml</c> in the model,
/// which every call site uses directly — in markup through the shared <c>SessionsView</c> component,
/// and as text where markup cannot go, such as inside an <c>&lt;option&gt;</c>.
/// </remarks>
public static class SessionsDisplay
{
    /// <summary>
    /// A stable key for a pattern, used to match a train's <see cref="Sessions"/> against a catalogue
    /// entry in a drop-down and to de-duplicate the catalogue. Equal patterns share the same key.
    /// </summary>
    public static string Key(Sessions sessions) => sessions.ToString();

    /// <summary>
    /// Parses session numbers and ranges from free text such as <c>"1-3, 4 7"</c> into the distinct
    /// session numbers (1-14). Accepts comma- or space-separated single numbers and <c>a-b</c> ranges.
    /// Returns an empty array when nothing valid is found.
    /// </summary>
    public static int[] ParseSessionNumbers(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var numbers = new SortedSet<int>();
        foreach (var token in text.Split([',', ' ', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var range = token.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (range.Length == 2 && int.TryParse(range[0], out var from) && int.TryParse(range[1], out var to))
            {
                for (var n = Math.Min(from, to); n <= Math.Max(from, to); n++)
                    if (n is >= 1 and <= 14) numbers.Add(n);
            }
            else if (int.TryParse(token, out var single) && single is >= 1 and <= 14)
            {
                numbers.Add(single);
            }
        }
        return [.. numbers];
    }
}
