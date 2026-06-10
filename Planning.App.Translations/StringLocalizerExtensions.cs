using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Tellurian.Localization;
using Tellurian.Trains.Schedules.Planning.App.Translations.Resources;

namespace Tellurian.Trains.Schedules.Planning.App.Translations;

/// <summary>
/// Resolves the localized label for <paramref name="key"/> in the current UI culture, falling back
/// to the key when not translated. Injected so components can write <c>@Localizer("From")</c>.
/// </summary>
public delegate string Translator(string? key);

/// <summary>
/// Convenience helpers for <see cref="IStringLocalizer"/> backed by <see cref="ResxStringLocalizer{T}"/>.
/// </summary>
public static class StringLocalizerExtensions
{
    /// <summary>
    /// Returns the localized text for <paramref name="key"/> in the current UI culture
    /// (the value of the localizer lookup), falling back to the key when not translated.
    /// </summary>
    public static string Localized(this IStringLocalizer localizer, string key) => localizer[key].Value;

    /// <summary>
    /// Registers the resx-backed <see cref="IStringLocalizer{T}"/> implementation and the
    /// <see cref="Translator"/> convenience delegate (bound to the <c>Labels</c> resources).
    /// </summary>
    public static IServiceCollection AddResxStringLocalizers(this IServiceCollection services)
    {
        services.AddTransient(typeof(IStringLocalizer<>), typeof(ResxStringLocalizer<>));
        services.AddTransient<Translator>(sp =>
        {
            var localizer = sp.GetRequiredService<IStringLocalizer<Labels>>();

            return key => key.HasValue ? localizer[key!].Value : string.Empty;
        });
        return services;
    }
}
