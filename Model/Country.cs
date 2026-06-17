namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// A country offered for a layout's default and for companies. There is one entry per country; a
/// country's language and code are only used to derive the language and culture of reports (the
/// user-interface language is chosen independently).
/// </summary>
/// <param name="Id">A stable unique identifier, used as the reference key from a layout. Once
/// assigned it must never be reused for a different country (only append new ones).</param>
/// <param name="ResourceKey">The translation key for the country's display name in the current
/// language, for example <c>Sweden</c> or <c>Switzerland</c>.</param>
/// <param name="Languages">The country's languages as a comma-separated, ordered list of two-letter
/// ISO codes, for example <c>de,fr,it</c> for Switzerland. The first language is the default report
/// language for the country; a company may override it to a later one in the list.</param>
/// <param name="CountryCode">The two-letter ISO 3166-1 alpha-2 country code, for example <c>CH</c>.
/// The culture used for a report is composed as <c>{language}-{CountryCode}</c> (for example <c>de-CH</c>).</param>
public record Country(int Id, string ResourceKey, string Languages, string CountryCode);


/// <summary>
/// Provider for the <see cref="Country">countries</see> offered for a layout, filtered by the
/// layout's <see cref="Theme"/>. There is one entry per country, identified by a stable
/// <see cref="Country.Id"/>; multilingual countries (Switzerland, Belgium, Norway, Canada) list
/// their languages in <see cref="Country.Languages"/>, the first being the default report language.
/// </summary>
public static class CountryExtensions
{
    extension(Country country)
    {
        // Id values are a stable contract: never reuse one, only append. European ids 1-14 match the
        // Module Registry; countries outside the registry (American, plus Belgium/Finland) use a
        // shared 101+ range so the registry can append 15, 16, ... without colliding.
        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<Country> Countries =>
        [
            new(1, "Sweden", "sv", "SE"),
            new(2, "Norway", "nb,nn", "NO"),
            new(3, "Denmark", "da", "DK"),
            new(4, "Germany", "de", "DE"),
            new(5, "Switzerland", "de,fr,it", "CH"),
            new(6, "UnitedKingdom", "en", "GB"),
            new(7, "Netherlands", "nl", "NL"),
            new(8, "Italy", "it", "IT"),
            new(9, "Poland", "pl", "PL"),
            new(10, "France", "fr", "FR"),
            new(11, "Hungary", "hu", "HU"),
            new(12, "CzechRepublic", "cs", "CZ"),
            new(13, "Slovakia", "sk", "SK"),
            new(14, "Austria", "de", "AT"),
            // European countries outside the Module Registry, in the shared 101+ overflow range.
            new(101, "UnitedStates", "en", "US"),
            new(102, "Canada", "en,fr", "CA"),
            new(103, "Belgium", "nl,fr", "BE"),
            new(104, "Finland", "fi", "FI"),
        ];

        /// <summary>
        /// Finds the <see cref="Country"/> with the given <see cref="Country.Id"/> across all themes,
        /// or <c>null</c> when none matches.
        /// </summary>
        public static Country? ById(int id) =>
            Country.Countries.FirstOrDefault(c => c.Id == id);

        /// <summary>
        /// Finds the <see cref="Country"/> with the given <see cref="Country.CountryCode"/> across all
        /// themes, or <c>null</c> when none matches.
        /// </summary>
        public static Country? ByCountryCode(string countryCode) =>
            Country.Countries.FirstOrDefault(c => c.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Finds countries by <see cref="Theme"/>.
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        public static IEnumerable<Country> CountriesByTheme(Theme theme) =>
            theme == Theme.American ? Country.Countries.Where(c => c.Id == 101 || c.Id == 102) :
            theme == Theme.European ? Country.Countries.Where(c => c.Id != 101 && c.Id != 102) :
            [];

    }
}

