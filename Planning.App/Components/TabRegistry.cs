namespace Tellurian.Trains.Schedules.Planning.App.Components;

public static class TabRegistry
{
    public static IReadOnlyList<TabDefinition> Tabs { get; } =
    [
        new("Home", "", typeof(Pages.Home)),
        new("Settings", "settings", typeof(Pages.Settings)),
        new("Layout", "layout", typeof(Pages.LayoutEditor)),
        new("Stretches", "stretches", typeof(Pages.Stretches)),
        new("TrainCategories", "categories", typeof(Pages.TrainCategories)),
        new("Trains", "trains", typeof(Pages.Trains)),
        new("GraphicalTimetable", "graphical-timetable", typeof(Pages.GraphicalTimetable)),
        new("Schedules", "schedules", typeof(Pages.Schedules)),
        new("VehicleOwners", "vehicle-owners", typeof(Pages.VehicleOwners)),
        new("Import", "import", typeof(Pages.Import)),
    ];

    public static Type? GetComponentType(string href) =>
        Tabs.FirstOrDefault(t => t.Href == href)?.ComponentType;
}

/// <summary>A workspace tab. <paramref name="ResourceKey"/> is the <c>Labels</c> resource key used to
/// localise the tab caption via the <c>Translator</c> service.</summary>
public sealed record TabDefinition(string ResourceKey, string Href, Type ComponentType);
