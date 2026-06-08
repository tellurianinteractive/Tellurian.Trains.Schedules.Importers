using Microsoft.Extensions.DependencyInjection;
using Tellurian.Localization;
using Tellurian.Localization.Implementatioms;
using Tellurian.Trains.Schedules.Planning.App.Translations.Resources;

namespace Tellurian.Trains.Schedules.Planning.App.Translations;

/// <summary>
/// Provides localized UI labels from the <see cref="Labels"/> resx resources for the current
/// UI culture, via Tellurian.Localization's resx resource provider group.
/// </summary>
public sealed class TranslationService(
    [FromKeyedServices(ResxResourceProviders.Key)] IResourceProviderGroup resx)
{
    /// <summary>
    /// Returns the translated label for <paramref name="key"/> in the current UI culture,
    /// falling back to the key itself when no translation exists.
    /// </summary>
    public async Task<string> GetLabel(string key) =>
        (await resx.Translated<Labels>(key)).Text;
}
