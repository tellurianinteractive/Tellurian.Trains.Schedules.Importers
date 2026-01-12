namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a category or type of train with associated display properties.
/// </summary>
public record TrainCategory
{
    /// <summary>
    /// Gets or initializes the unique identifier for this train category.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets or initializes the prefix shown before train number (e.g., "P" for passenger, "G" for goods).
    /// Used in train identity display.
    /// </summary>
    public string Prefix { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the optional suffix shown after train number.
    /// </summary>
    public string Suffix { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes a value indicating whether this is a passenger train category.
    /// </summary>
    public bool IsPassenger { get; init; }

    /// <summary>
    /// Gets or initializes a value indicating whether this is a freight train category.
    /// </summary>
    public bool IsFreight { get; init; }

    /// <summary>
    /// Gets or initializes the type of train (e.g., "Passenger", "Freight", "HighSpeed") used for translations.
    /// </summary>
    public required string ResourceName { get; init; }

    /// <summary>
    /// Gets or initializes the color used when drawing timetable graphs and schematic train lines.
    /// Format: CSS color string (e.g., "#FF0000", "red", "rgb(255,0,0)")
    /// </summary>
    public string Color { get; init; } = "#000000";


    /// <inheritdoc/>
    public override string ToString() => ResourceName;
}

/// <summary>
/// Provides extension methods for <see cref="TrainCategory"/>.
/// </summary>
public static class TrainCategoryExtensions
{
    extension(TrainCategory category)
    {
        /// <summary>
        /// Gets the full train identity string for a given train number.
        /// </summary>
        /// <param name="trainNumber">The train number.</param>
        /// <returns>A formatted string combining prefix, number, and suffix.</returns>
        public string TrainIdentity(int trainNumber) =>
            $"{category.Prefix} {trainNumber} {category.Suffix}".Trim();
    }
}
