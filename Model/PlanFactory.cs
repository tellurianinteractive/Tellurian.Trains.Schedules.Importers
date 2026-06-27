namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Builds a new, empty <see cref="Plan"/> (layout + timetable) from scratch with reasonable
/// defaults, so a planner can start without importing. This is the single, centralised place that
/// defines what a brand-new plan contains: default settings, the default country and its regions,
/// the standard train categories and the standard session catalogue — all generated in the layout's
/// default language. Reference data that should be localised (regions, train categories) is generated
/// in that language.
/// </summary>
public static class PlanFactory
{
    /// <summary>
    /// Creates a new plan with default settings for the given interface <paramref name="language"/>,
    /// deriving the default country from that language. This is the entry point UI should use.
    /// </summary>
    /// <param name="name">The (already localised) name for the layout, timetable and plan, e.g. "New layout".</param>
    /// <param name="language">The two-letter language code used to pick the default country and to
    /// localise the seeded regions and train categories.</param>
    public static Plan CreatePlan(string name, string language) =>
        CreatePlan(name, Country.ByLanguage(language).Id, language);

    /// <summary>
    /// Creates a new plan with default settings, a default country added to the layout's country
    /// catalogue, the standard regions for that country, the standard Passenger/Freight train
    /// categories and the standard session catalogue. No operation locations, stretches, companies
    /// or trains are created — the user adds or imports those.
    /// </summary>
    /// <param name="name">The (already localised) name for the layout, timetable and plan, e.g. "New layout".</param>
    /// <param name="defaultCountryId">The <see cref="Country.Id"/> of the layout's default country.</param>
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
        SeedSessions(timetable);

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

    // Start the timetable's session catalogue with the standard patterns (All, Odd, Even, every third);
    // the user adds custom patterns (e.g. 1-3, 4-7) on the Settings tab.
    private static void SeedSessions(Timetable timetable)
    {
        foreach (var sessions in CommonSessionPatterns.DefaultCatalogue) timetable.Sessions.Add(sessions);
    }
}
