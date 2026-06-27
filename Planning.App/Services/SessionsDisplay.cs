using Tellurian.Trains.Schedules.Planning.App.Translations;

namespace Tellurian.Trains.Schedules.Planning.App.Services;

/// <summary>
/// Renders a <see cref="Sessions"/> pattern as human-readable text and parses the text a user types
/// when adding a custom pattern to a timetable's session catalogue. The same value can be presented
/// either as session numbers (e.g. <c>1-5</c>, <c>1 3 5</c>) or as weekdays, depending on the
/// layout's <c>UseDays</c> setting.
/// </summary>
public static class SessionsDisplay
{
    /// <summary>
    /// Builds the display text for a session/day pattern. When <paramref name="useDays"/> is set the
    /// active weekdays are shown (using the short day-name resources); otherwise the active session
    /// numbers are shown.
    /// </summary>
    public static string Text(Sessions sessions, bool useDays, Translator translator)
    {
        // Day names are translated and ordered (Sunday- or Monday-first) by the model using the
        // current culture, so the printed order follows the user's locale.
        if (useDays) return sessions.DaysShort;
        // SessionsNumbers yields "All"/"None" as literals (translate those) and numeric strings otherwise.
        var text = sessions.SessionsNumbers;
        return text switch
        {
            "All" => translator("All"),
            "None" => translator("None"),
            _ => text,
        };
    }

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
