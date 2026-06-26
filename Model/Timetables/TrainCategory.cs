namespace Tellurian.Trains.Schedules.Model.Timetables;

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
    /// Gets or sets the prefix shown before train number (e.g., "P" for passenger, "G" for goods).
    /// Used in train identity display.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional suffix shown after train number.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is a passenger train category.
    /// </summary>
    public bool IsPassenger { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a freight train category.
    /// </summary>
    public bool IsFreight { get; set; }

    /// <summary>
    /// Gets or sets the name of this train category.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the color used when drawing timetable graphs and schematic train lines.
    /// Format: CSS color string (e.g., "#FF0000", "red", "rgb(255,0,0)")
    /// </summary>
    public string Color { get; set; } = "#000000";

    /// <summary>
    /// Gets or sets the default scale speed in km/h for trains of this category.
    /// Used when a train does not set its own <see cref="Train.MaxSpeed"/>.
    /// </summary>
    public int DefaultSpeed { get; set; } = 100;

    /// <summary>
    /// Optional company that operates trains in this category.
    /// </summary>
    public Company? Company { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Name;

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

        /// <summary>
        /// The standard pair of train categories — Passenger (prefix <c>P</c>) and Freight (prefix
        /// <c>G</c>) — named in the given two-letter <paramref name="language"/> (falling back to
        /// English), with no operating company. Used to seed a new timetable.
        /// </summary>
        /// <param name="language">The two-letter language code of the layout's default language.</param>
        public static IEnumerable<TrainCategory> DefaultsFor(string language) =>
            Defaults.Select(d => new TrainCategory
            {
                Id = d.Id,
                Name = d.NameFor(language),
                Prefix = d.Prefix,
                Color = d.Color,
                IsPassenger = d.IsPassenger,
                IsFreight = !d.IsPassenger,
                DefaultSpeed = d.DefaultSpeed,
            });
    }

    // The localised names of the standard categories; used only when seeding a new timetable, after
    // which each category keeps a single Name in the layout's default language.
    private static readonly TrainCategoryDefault[] Defaults =
    [
        new(1, "P", "#CC0000", IsPassenger: true,  DefaultSpeed: 100, EN: "Passenger", DA: "Persontog", DE: "Reisezug",  NB: "Persontog", SV: "Persontåg"),
        new(2, "G", "#000000", IsPassenger: false, DefaultSpeed: 80,  EN: "Freight",   DA: "Godstog",   DE: "Güterzug",  NB: "Godstog",   SV: "Godståg"),
    ];

    private sealed record TrainCategoryDefault(int Id, string Prefix, string Color, bool IsPassenger, int DefaultSpeed, string EN, string DA, string DE, string NB, string SV)
    {
        public string NameFor(string language) => language switch
        {
            "sv" => SV,
            "da" => DA,
            "de" => DE,
            "nb" or "nn" => NB,
            _ => EN,
        };
    }
}
