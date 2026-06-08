namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// Settings for integration with external services.
/// </summary>
public sealed class IntegrationSettings
{
    /// <summary>API key for the ModuleRegistry web API, used when importing operation locations.</summary>
    public string? ModuleRegistryApiKey { get; set; }
}
