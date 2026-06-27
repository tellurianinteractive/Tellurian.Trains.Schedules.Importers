using System.Globalization;
using System.Resources;

namespace Tellurian.Trains.Schedules.Model.Resources;

/// <summary>
/// Resolves a model class name to its localised display name in the current UI language, from the
/// <c>Classes</c> resources. Used to present model objects to the user (for example in deletion
/// messages) without the model depending on the application's own resources.
/// </summary>
/// <remarks>
/// Lookup is by the raw type name (for example <c>nameof(Company)</c>) so the same key works for the
/// deleted object and for the objects referencing it. Resolution uses
/// <see cref="CultureInfo.CurrentUICulture"/> at call time, which the application sets when the user
/// chooses a language; an unknown key falls back to the key itself.
/// </remarks>
public static class ClassNames
{
    private static readonly ResourceManager Manager =
        new("Tellurian.Trains.Schedules.Model.Resources.Classes", typeof(ClassNames).Assembly);

    /// <summary>
    /// Returns the localised name for <paramref name="classNameKey"/> in the current UI language, or
    /// the key itself when no resource exists for it.
    /// </summary>
    /// <param name="classNameKey">A model type name, for example <c>nameof(Company)</c>.</param>
    public static string Localized(string classNameKey) =>
        Manager.GetString(classNameKey, CultureInfo.CurrentUICulture) is { Length: > 0 } name ? name : classNameKey;

    /// <summary>
    /// The resource key for an object: its <see cref="ITranslatable.TranslationKey"/> when it is
    /// <see cref="ITranslatable"/>, otherwise its type name. This is the override point used by
    /// <see cref="LocalizedFor"/>.
    /// </summary>
    /// <param name="value">The object to label.</param>
    public static string KeyOf(object value) =>
        value is ITranslatable translatable ? translatable.TranslationKey : value.GetType().Name;

    /// <summary>
    /// Returns the localised display name for <paramref name="value"/> in the current UI language,
    /// honouring an <see cref="ITranslatable"/> override and falling back to the type name.
    /// </summary>
    /// <param name="value">The object to label.</param>
    public static string LocalizedFor(object value) => Localized(KeyOf(value));
}
