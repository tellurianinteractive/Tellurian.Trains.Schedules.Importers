using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning;

/// <summary>
/// Builds a new, empty <see cref="Plan"/> (layout + timetable) from scratch with reasonable
/// defaults, so a planner can start without importing. Reference data that should be localised
/// (regions, train categories) is generated in the layout's default language.
/// </summary>
public static class NewPlanFactory
{
    /// <summary>
    /// Creates a new plan with default settings, a default country added to the layout's country
    /// catalogue, the standard regions for that country, and the standard Passenger/Freight train
    /// categories. No operation locations, stretches, companies or trains are created — the user
    /// adds or imports those.
    /// </summary>
    /// <param name="name">The (already localised) name for the layout, timetable and plan, e.g. "New layout".</param>
    /// <param name="defaultCountryId">The <see cref="Country.Id"/> of the layout's default country
    /// (typically derived from the GUI language via <see cref="CountryExtensions"/>.<c>ByLanguage</c>).</param>
    /// <param name="language">The two-letter language code used to localise the seeded regions and
    /// train categories.</param>
    public static Plan CreatePlan(string name, int defaultCountryId, string language)
    {
        var layout = new Layout { Name = name };
        ApplyDefaultSettings(layout, defaultCountryId);
        SeedCountries(layout, defaultCountryId);
        SeedRegions(layout, defaultCountryId);

        var timetable = new Timetable(name, layout);
        SeedCategories(timetable, language);

        return Plan.Create(name, timetable);
    }

    // New-layout setting defaults. Values not set here keep their class-level defaults (Theme=European,
    // Scale=H0, StartTime=06:00, clock speed 5, speed points, validations on, etc.).
    private static void ApplyDefaultSettings(Layout layout, int defaultCountryId)
    {
        var settings = layout.Settings;
        settings.Identity.DefaultCountryId = defaultCountryId;

        settings.General.EndTime = TimeSpan.FromHours(18);

        var graph = settings.GraphicTimetable;
        graph.MinuteSpacing = 3;
        graph.KilometerSpacing = 2;
        graph.StationSpacing = 100;
        graph.TrackSpacing = 10;

        settings.TimeAndSpeed.StationTimings.MinimumStopMinutes = 2;
    }

    // Start the layout's country catalogue with just the default country; the user adds more on the
    // Countries tab.
    private static void SeedCountries(Layout layout, int defaultCountryId)
    {
        if (Country.ById(defaultCountryId) is { } country) layout.Countries.Add(country);
    }

    private static void SeedRegions(Layout layout, int defaultCountryId)
    {
        foreach (var region in Region.DefaultsFor(defaultCountryId)) layout.Add(region);
    }

    private static void SeedCategories(Timetable timetable, string language)
    {
        foreach (var category in TrainCategory.DefaultsFor(language)) timetable.TrainCategories.Add(category);
    }
}
