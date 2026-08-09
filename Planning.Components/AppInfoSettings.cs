namespace Tellurian.Trains.Schedules.Planning.Components;

/// <summary>
/// Facts about the hosting application that the components display but cannot know themselves.
/// </summary>
/// <remarks>
/// Supplied by the host through <see cref="ServiceCollectionExtensions.AddPlanningComponents"/>.
/// The components deliberately do not read the entry assembly: which assembly that is depends on
/// the host, and a future interactive Server host would answer differently from the WebAssembly app.
/// </remarks>
public sealed record AppInfoSettings
{
    /// <summary>
    /// The application's release version, shown in the top bar. Has its own release cycle, separate
    /// from the version of the NuGet libraries this app is built on.
    /// </summary>
    public required string Version { get; init; }
}
