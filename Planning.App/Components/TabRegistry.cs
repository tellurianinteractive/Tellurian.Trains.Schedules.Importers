namespace Tellurian.Trains.Schedules.Planning.App.Components;

public static class TabRegistry
{
    public static IReadOnlyList<TabDefinition> Tabs { get; } =
    [
        new("Home", "", typeof(Pages.HomeTab)),
        new("Settings", "settings", typeof(Pages.SettingsTab), HelpKey: "Settings"),
        new("Countries", "countries", typeof(Pages.CountriesTab)),
        new("Regions", "regions", typeof(Pages.RegionsTab), HelpKey: "Regions"),
        new("OperationLocations", "operation-locations", typeof(Pages.OperationLocationsTab), HelpKey: "OperationLocations"),
        new("Stretches", "stretches", typeof(Pages.StretchesTab), HelpKey: "Stretches"),
        new("Companies", "companies", typeof(Pages.CompaniesTab)),
        new("TrainCategories", "categories", typeof(Pages.TrainCategoriesTab), HelpKey: "TrainCategories"),
        new("Trains", "trains", typeof(Pages.TrainsTab), HelpKey: "Trains"),
        new("GraphicalTimetable", "graphical-timetable", typeof(Pages.GraphicalTimetableTab), HelpKey: "GraphicalTimetable"),
        new("Schedules", "schedules", typeof(Pages.SchedulesTab), HelpKey: "Schedules"),
        new("VehicleOwners", "vehicle-owners", typeof(Pages.VehicleOwnersTab), HelpKey: "VehicleOwners"),
        new("Import", "import", typeof(Pages.ImportTab), HelpKey: "Import"),
    ];

    public static Type? GetComponentType(string href) =>
        Tabs.FirstOrDefault(t => t.Href == href)?.ComponentType;
}

/// <summary>A workspace tab. <paramref name="ResourceKey"/> is the <c>Labels</c> resource key used to
/// localise the tab caption via the <c>Translator</c> service.</summary>
/// <param name="HelpKey">Key of the per-tab help markdown (<c>Content/Help/{HelpKey}.md</c>), surfaced
/// via the <c>?</c> popover in the tab title. <see langword="null"/> when the tab has no help yet.</param>
public sealed record TabDefinition(string ResourceKey, string Href, Type ComponentType, string? HelpKey = null);
