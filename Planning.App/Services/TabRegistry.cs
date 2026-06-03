namespace Tellurian.Trains.Schedules.Planning.App.Services;

public static class TabRegistry
{
    public static IReadOnlyList<TabDefinition> Tabs { get; } =
    [
        new("Home", "", typeof(Pages.Home)),
        new("Settings", "settings", typeof(Pages.Settings)),
        new("Layout", "layout", typeof(Pages.LayoutEditor)),
        new("Stretches", "stretches", typeof(Pages.Stretches)),
        new("Train Categories", "categories", typeof(Pages.TrainCategories)),
        new("Trains", "trains", typeof(Pages.Trains)),
        new("Graphical Timetable", "graphical-timetable", typeof(Pages.GraphicalTimetable)),
        new("Vehicle Schedules", "vehicle-schedules", typeof(Pages.VehicleSchedules)),
        new("Vehicle Owners", "vehicle-owners", typeof(Pages.VehicleOwners)),
        new("Import", "import", typeof(Pages.Import)),
    ];

    public static Type? GetComponentType(string href) =>
        Tabs.FirstOrDefault(t => t.Href == href)?.ComponentType;
}

public sealed record TabDefinition(string Label, string Href, Type ComponentType);
