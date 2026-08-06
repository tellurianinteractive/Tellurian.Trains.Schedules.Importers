namespace Tellurian.Trains.Schedules.Planning.Components.Services;

/// <summary>
/// App-wide user-interface preferences persisted in browser storage, distinct from a layout's own
/// settings (which are saved with the planning document). Values are cached in memory after the
/// first read so components can apply them without an async round-trip on every render.
/// </summary>
public sealed class UiPreferenceService(BrowserStorageService storage)
{
    private const string SettingsSectionKey = "planning.ui.settingsSection";
    private const string StretchesSectionKey = "planning.ui.stretchesSection";
    private const string CargoFlowSectionKey = "planning.ui.cargoFlowSection";
    private const string LastRouteKey = "planning.ui.lastRoute";
    private const string GraphHalfKey = "planning.ui.graphHalf";

    private string? _settingsSection;
    private bool _settingsSectionLoaded;
    private string? _stretchesSection;
    private bool _stretchesSectionLoaded;
    private string? _cargoFlowSection;
    private bool _cargoFlowSectionLoaded;
    private string? _lastRoute;
    private bool _lastRouteLoaded;
    private string? _graphHalf;
    private bool _graphHalfLoaded;

    /// <summary>The last active sub-tab in the Settings page, or null when none has been stored.</summary>
    public async Task<string?> GetSettingsSectionAsync()
    {
        if (_settingsSectionLoaded) return _settingsSection;
        _settingsSection = await storage.GetStringAsync(SettingsSectionKey);
        _settingsSectionLoaded = true;
        return _settingsSection;
    }

    /// <summary>Remembers the active sub-tab in the Settings page.</summary>
    public async Task SetSettingsSectionAsync(string section)
    {
        if (_settingsSection == section) return;
        _settingsSection = section;
        _settingsSectionLoaded = true;
        await storage.SetStringAsync(SettingsSectionKey, section);
    }

    /// <summary>The last active sub-section in the Stretches page, or null when none has been stored.</summary>
    public async Task<string?> GetStretchesSectionAsync()
    {
        if (_stretchesSectionLoaded) return _stretchesSection;
        _stretchesSection = await storage.GetStringAsync(StretchesSectionKey);
        _stretchesSectionLoaded = true;
        return _stretchesSection;
    }

    /// <summary>Remembers the active sub-section in the Stretches page.</summary>
    public async Task SetStretchesSectionAsync(string section)
    {
        if (_stretchesSection == section) return;
        _stretchesSection = section;
        _stretchesSectionLoaded = true;
        await storage.SetStringAsync(StretchesSectionKey, section);
    }

    /// <summary>The last active sub-section in the Cargo Flow page, or null when none has been stored.</summary>
    public async Task<string?> GetCargoFlowSectionAsync()
    {
        if (_cargoFlowSectionLoaded) return _cargoFlowSection;
        _cargoFlowSection = await storage.GetStringAsync(CargoFlowSectionKey);
        _cargoFlowSectionLoaded = true;
        return _cargoFlowSection;
    }

    /// <summary>Remembers the active sub-section in the Cargo Flow page.</summary>
    public async Task SetCargoFlowSectionAsync(string section)
    {
        if (_cargoFlowSection == section) return;
        _cargoFlowSection = section;
        _cargoFlowSectionLoaded = true;
        await storage.SetStringAsync(CargoFlowSectionKey, section);
    }

    /// <summary>The last active top-level route (e.g. "settings", "workspace"), or null when none.</summary>
    public async Task<string?> GetLastRouteAsync()
    {
        if (_lastRouteLoaded) return _lastRoute;
        _lastRoute = await storage.GetStringAsync(LastRouteKey);
        _lastRouteLoaded = true;
        return _lastRoute;
    }

    /// <summary>Remembers the active top-level route so it can be restored on the next start.</summary>
    public async Task SetLastRouteAsync(string route)
    {
        if (_lastRoute == route) return;
        _lastRoute = route;
        _lastRouteLoaded = true;
        await storage.SetStringAsync(LastRouteKey, route);
    }

    /// <summary>Which part of the graphical timetable to show (whole/first/last half), or null when none
    /// has been stored. This is a user preference, not part of the planning document.</summary>
    public async Task<string?> GetGraphHalfAsync()
    {
        if (_graphHalfLoaded) return _graphHalf;
        _graphHalf = await storage.GetStringAsync(GraphHalfKey);
        _graphHalfLoaded = true;
        return _graphHalf;
    }

    /// <summary>Remembers which part of the graphical timetable to show.</summary>
    public async Task SetGraphHalfAsync(string half)
    {
        if (_graphHalf == half) return;
        _graphHalf = half;
        _graphHalfLoaded = true;
        await storage.SetStringAsync(GraphHalfKey, half);
    }
}
