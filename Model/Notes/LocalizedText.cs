namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Represents a text in a specific language
/// </summary>
/// <param name="Text"></param>
/// <param name="LanguageCode"></param>
public record LocalizedText(string Text, string LanguageCode);
