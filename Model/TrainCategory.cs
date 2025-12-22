namespace Tellurian.Trains.Schedules.Importers.Model;

public record TrainCategory
{
    public int Id { get; init; }

    /// <summary>
    /// Prefix shown before train number (e.g., "P" for passenger, "G" for goods)
    /// Used in train identity display.
    /// </summary>
    public string Prefix { get; init; } = string.Empty;

    /// <summary>
    /// Optional suffix shown after train number.
    /// </summary>
    public string Suffix { get; init; } = string.Empty;

    /// <summary>
    /// Type of train (e.g., "Passenger", "Freight", "HighSpeed") used for translations.
    /// </summary>
    public required string ResourceName { get; init; }

    /// <summary>
    /// Color used when drawing timetable graphs and schematic train lines.
    /// Format: CSS color string (e.g., "#FF0000", "red", "rgb(255,0,0)")
    /// </summary>
    public string Color { get; init; } = "#000000";

    /// <summary>
    /// Display order for sorting categories in UI.
    /// </summary>
    public int DisplayOrder { get; init; }

    public override string ToString() => ResourceName;
}

public static class TrainCategoryExtensions
{
    extension(TrainCategory category)
    {
        public string TrainIdentity(int trainNumber) =>
            $"{category.Prefix} {trainNumber} {category.Suffix}".Trim();

        public static TrainCategory Unknown => new()
        {
            Id = 0,
            Prefix = "",
            Suffix = "",
            ResourceName = "Unknown",
            Color = "#808080",
            DisplayOrder = int.MaxValue
        };
    }
}
