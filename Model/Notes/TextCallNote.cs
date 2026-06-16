using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Represents a text-based note associated with a station call.
/// </summary>
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

    /// <inheritdoc/>
    public override string Text => Texts.Where(t => t.LanguageCode.Equals(CurrentLanguageCode, StringComparison.OrdinalIgnoreCase)).SingleOrDefault()?.Text ?? Texts[0].Text;
}
