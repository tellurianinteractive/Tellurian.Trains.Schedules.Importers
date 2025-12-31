using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

public class TextCallNote : CallNote
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private TextCallNote() => Text = string.Empty;

    public TextCallNote(string text)
    {
        Text = text;
    }

    public string Text { get; set; }
    public string LanguageCode { get; set; } = string.Empty;

    public override string ToString() => Text;
}
