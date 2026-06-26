namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// Settings for integration with external services used to import and export data.
/// </summary>
public sealed class IntegrationSettings
{
    /// <summary>Base URL of the ModuleRegistry web API, used when importing operation locations and
    /// when sending the plan there for conversion/distribution (see Requirements Specification §5.5).</summary>
    public string? ModuleRegistryApiUrl { get; set; } = "https://moduleregistry.azurewebsites.net";

    /// <summary>API key for the ModuleRegistry web API.</summary>
    public string? ModuleRegistryApiKey { get; set; }
}
