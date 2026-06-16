using System.Collections;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Localization;

namespace Tellurian.Trains.Schedules.Planning.App.Translations;

/// <summary>
/// An <see cref="IStringLocalizer{T}"/> backed by the .NET <see cref="ResourceManager"/> for the
/// resx resource type <typeparamref name="T"/> (e.g. <c>Labels</c>). Lookups use the current UI
/// culture and the standard parent-culture / neutral-language fallback, and return the key itself
/// when no translation exists. This lets components localise through the standard localizer
/// interface while the resources stay centralised in this project.
/// </summary>
/// <remarks>
/// Resx lookups resolve synchronously, so this is safe to use directly in component markup.
/// Intended to be promoted into Tellurian.Localization once stable.
/// </remarks>
public sealed class ResxStringLocalizer<T> : IStringLocalizer<T>
{
    private readonly ResourceManager _resourceManager = new(typeof(T));

    /// <summary>
    /// Gets the localized string for <paramref name="name"/> in the current UI culture,
    /// falling back to the key itself (with <see cref="LocalizedString.ResourceNotFound"/> set) when no translation exists.
    /// </summary>
    public LocalizedString this[string name]
    {
        get
        {
            var value = _resourceManager.GetString(name, CultureInfo.CurrentUICulture);
            return new LocalizedString(name, value ?? name, resourceNotFound: value is null);
        }
    }

    /// <summary>
    /// Gets the localized string for <paramref name="name"/> in the current UI culture and formats it
    /// with <paramref name="arguments"/>, falling back to the key itself when no translation exists.
    /// </summary>
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var translated = this[name];
            return new LocalizedString(
                name,
                string.Format(CultureInfo.CurrentUICulture, translated.Value, arguments),
                translated.ResourceNotFound);
        }
    }

    /// <summary>
    /// Gets all localized strings for the current UI culture, optionally including values inherited
    /// from parent cultures.
    /// </summary>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var set = _resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, createIfNotExists: true, tryParents: includeParentCultures);
        if (set is null) yield break;
        foreach (DictionaryEntry entry in set)
        {
            var name = (string)entry.Key;
            yield return new LocalizedString(name, entry.Value?.ToString() ?? name, resourceNotFound: false);
        }
    }
}
