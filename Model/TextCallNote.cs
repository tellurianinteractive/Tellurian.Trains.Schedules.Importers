using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a text-based note associated with a station call.
/// </summary>
public class TextCallNote : CallNote
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private TextCallNote() => Text = string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="TextCallNote"/> with the specified text.
    /// </summary>
    /// <param name="text">The note text.</param>
    public TextCallNote(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Gets or sets the text content of this note.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the language code for this note's text.
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override string ToString() => Text;
}
