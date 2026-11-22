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

    /// <summary>
    /// Creates a new plan from the layout of a hall plan (<see cref="Layouts.HallPlanFile.ReadLayout"/>): a plan with
    /// the defaults of <see cref="CreatePlan(string, int, string)"/>, named after the hall plan, with its locations,
    /// tracks, stretches and modules. Each stretch gets a running time from its distance and speed. The default country
    /// is the one most of the locations are in, else the one of <paramref name="language"/>. The hall plan's layout is
    /// taken over, not copied; don't use it afterwards.
    /// </summary>
    /// <param name="hallPlan">The layout read from the hall plan.</param>
    /// <param name="language">The two-letter language code used to localise the seeded regions and train categories,
    /// and to pick the default country when no location has one.</param>
    /// <param name="name">The name of the plan, when the hall plan has none.</param>
    public static Plan CreatePlan(Layout hallPlan, string language, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(hallPlan);
        var planName = hallPlan.Name is { Length: > 0 } hallPlanName ? hallPlanName : name ?? string.Empty;
        var countryId = Layouts.HallPlanImport.MostCommonCountryId(hallPlan) ?? Country.ByLanguage(language).Id;
        var plan = CreatePlan(planName, countryId, language);
        Layouts.HallPlanImport.TakeInto(plan.Layout, hallPlan);
        return plan;
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
