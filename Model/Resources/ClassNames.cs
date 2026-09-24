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

    /// <summary>
    /// Returns the localised name for <paramref name="value"/> as it is written <em>inside</em> a
    /// sentence, for example in a call note: "Before departure, fetch <c>locomotive</c> 21 from track 31".
    /// </summary>
    /// <remarks>
    /// A display name is written the way a label is — with a capital first letter — but a note embeds it
    /// mid-sentence, where every language here but German writes a common noun in lower case. German
    /// capitalises its nouns wherever they stand, so its names are returned unchanged; see
    /// <see cref="InSentence"/>.
    /// </remarks>
    /// <param name="value">The object to name.</param>
    public static string InSentenceFor(object value) => InSentence(LocalizedFor(value));

    /// <summary>
    /// Returns <paramref name="name"/> as it is written inside a sentence in the current UI language:
    /// with a lower-case first letter, except in German, which capitalises nouns wherever they stand.
    /// </summary>
    /// <param name="name">A display name, as the resources give it.</param>
    public static string InSentence(string name) =>
        CapitalisesNouns || name.Length == 0
            ? name
            : char.ToLower(name[0], CultureInfo.CurrentUICulture) + name[1..];

    // German is the one language here that writes a common noun with a capital wherever it stands.
    private static bool CapitalisesNouns =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase);
}
