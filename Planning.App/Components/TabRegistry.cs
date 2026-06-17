namespace Tellurian.Trains.Schedules.Planning.App.Components;

public static class TabRegistry
{
    public static IReadOnlyList<TabDefinition> Tabs { get; } =
    [
        new("Home", "", typeof(Pages.Home), HelpKey: "Home"),
        new("Settings", "settings", typeof(Pages.Settings), HelpKey: "Settings"),
        new("OperationLocations", "operation-locations", typeof(Pages.OperationLocationsEditor), HelpKey: "OperationLocations"),
        new("Stretches", "stretches", typeof(Pages.Stretches), HelpKey: "Stretches"),
        new("Companies", "companies", typeof(Pages.Companies)),
        new("TrainCategories", "categories", typeof(Pages.TrainCategories), HelpKey: "TrainCategories"),
        new("Trains", "trains", typeof(Pages.Trains), HelpKey: "Trains"),
        new("GraphicalTimetable", "graphical-timetable", typeof(Pages.GraphicalTimetable), HelpKey: "GraphicalTimetable"),
        new("Schedules", "schedules", typeof(Pages.Schedules), HelpKey: "Schedules"),
        new("VehicleOwners", "vehicle-owners", typeof(Pages.VehicleOwners), HelpKey: "VehicleOwners"),
        new("Import", "import", typeof(Pages.Import), HelpKey: "Import"),
    ];

    public static Type? GetComponentType(string href) =>
        Tabs.FirstOrDefault(t => t.Href == href)?.ComponentType;
}

/// <summary>A workspace tab. <paramref name="ResourceKey"/> is the <c>Labels</c> resource key used to
/// localise the tab caption via the <c>Translator</c> service.</summary>
/// <param name="HelpKey">Key of the per-tab help markdown (<c>Content/Help/{HelpKey}.md</c>), surfaced
/// via the <c>?</c> popover in the tab title. <see langword="null"/> when the tab has no help yet.</param>
public sealed record TabDefinition(string ResourceKey, string Href, Type ComponentType, string? HelpKey = null);
