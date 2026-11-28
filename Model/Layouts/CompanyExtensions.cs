using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Provides extension methods for <see cref="Company"/>.
/// </summary>
public static class CompanyExtensions
{
    extension(Company company)
    {
        /// <summary>
        /// Gets a display name including the company name, signature, and country code (resolved
        /// from <see cref="Company.CountryId"/> via the static catalogue; the code is omitted when
        /// no country is set).
        /// </summary>
        public string DisplayName =>
            Country.ById(company.CountryId ?? 0) is { } country
                ? $"{company.Name} ({company.Signature}-{country.CountryCode})"
                : $"{company.Name} ({company.Signature})";

        /// <summary>
        /// Builds the text shown for the company in a selection drop-down: the company name followed
        /// by its country's name in the current language, for example <c>DB Cargo Germany</c>. The
        /// country name is omitted when no country is set.
        /// </summary>
        /// <param name="translateCountry">Resolves a country's <see cref="Country.ResourceKey"/> to
        /// its display name in the current language.</param>
        public string DropdownText(Func<string, string> translateCountry) =>
            Country.ById(company.CountryId ?? 0) is { } country
                ? $"{company.Name} {translateCountry(country.ResourceKey)}"
                : company.Name;

        /// <summary>
        /// The text shown for the company where one is chosen from a list: the name, followed by the operating
        /// period when one is given, e.g. <c>Statens Järnvägar (1988–2008)</c>. Companies may share a signature,
        /// one per period, and the period is what tells them apart.
        /// </summary>
        public string ChoiceText =>
            company.OperatingPeriodText is { Length: > 0 } period ? $"{company.Name} ({period})" : company.Name;

        /// <summary>
        /// Creates a company from just a signature, using the signature as the name.
        /// </summary>
        /// <param name="signature">The signature to use.</param>
        /// <returns>A new company with the signature as both name and signature.</returns>
        public static Company FromSignature(string signature) =>
            new(0, signature, signature);
    }

    extension(IEnumerable<Company> companies)
    {
        /// <summary>
        /// The companies in the order they are offered for choice: by name, then the same company's periods in
        /// time order, a period without a first year first.
        /// </summary>
        public IEnumerable<Company> InChoiceOrder =>
            companies
                .OrderBy(c => c.Name, StringComparer.CurrentCulture)
                .ThenBy(c => c.OperatesFromYear ?? int.MinValue)
                .ThenBy(c => c.OperatesToYear ?? int.MaxValue)
                .ThenBy(c => c.Id);
    }
}
