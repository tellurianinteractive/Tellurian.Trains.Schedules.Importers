using System.Globalization;
using System.Text.RegularExpressions;

namespace Tellurian.Trains.Schedules.Importers.Xpln;

/// <summary>
/// Per-import settings for an XPLN import. An XPLN file carries no language or country, so these are
/// supplied for the import: <see cref="Language"/> is used for generated notes and <see cref="CountryCode"/>
/// sets the imported layout's default country (and theme).
/// </summary>
/// <remarks>
/// The settings are taken from a culture encoded in the file name as the segment before the
/// extension, for example <c>Givskud2021.da-DK.ods</c>. A file name without such a segment
/// (for example <c>Givskud2021.ods</c>) falls back to <see cref="CultureInfo.CurrentCulture"/>.
/// </remarks>
/// <param name="Language">The two-letter ISO language code for generated content, for example <c>sv</c>.</param>
/// <param name="CountryCode">The two-letter ISO 3166-1 alpha-2 country code, for example <c>SE</c>
/// (empty when the culture has no region).</param>
public sealed partial record XplnImportOptions(string Language, string CountryCode)
{
    /// <summary>Options for the language and country of <see cref="CultureInfo.CurrentCulture"/>.</summary>
    public static XplnImportOptions FromCurrentCulture() => FromCulture(CultureInfo.CurrentCulture);

    /// <summary>
    /// Options for the culture encoded in <paramref name="fileName"/> (for example <c>da-DK</c> in
    /// <c>Givskud2021.da-DK.ods</c>), or for <see cref="CultureInfo.CurrentCulture"/> when the file
    /// name has no culture segment.
    /// </summary>
    public static XplnImportOptions FromFileName(string fileName) =>
        CultureFromFileName(fileName) is { } culture ? FromCulture(culture) : FromCurrentCulture();

    /// <summary>
    /// The schedule name from <paramref name="fileName"/>, without the extension and without a
    /// trailing culture segment (for example <c>Givskud2021</c> from <c>Givskud2021.da-DK.ods</c>).
    /// </summary>
    public static string ScheduleNameFrom(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var lastDot = name.LastIndexOf('.');
        return lastDot > 0 && TryGetCulture(name[(lastDot + 1)..]) is not null ? name[..lastDot] : name;
    }

    private static XplnImportOptions FromCulture(CultureInfo culture)
    {
        var countryCode = string.Empty;
        try { countryCode = new RegionInfo(culture.Name).TwoLetterISORegionName; }
        catch (ArgumentException) { /* neutral culture has no region */ }
        return new XplnImportOptions(culture.TwoLetterISOLanguageName, countryCode);
    }

    private static CultureInfo? CultureFromFileName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var lastDot = name.LastIndexOf('.');
        return lastDot > 0 ? TryGetCulture(name[(lastDot + 1)..]) : null;
    }

    private static CultureInfo? TryGetCulture(string candidate)
    {
        if (!CulturePattern().IsMatch(candidate)) return null;
        try { return CultureInfo.GetCultureInfo(candidate); }
        catch (CultureNotFoundException) { return null; }
    }

    [GeneratedRegex("^[A-Za-z]{2,3}(-[A-Za-z]{2,})?$")]
    private static partial Regex CulturePattern();
}
