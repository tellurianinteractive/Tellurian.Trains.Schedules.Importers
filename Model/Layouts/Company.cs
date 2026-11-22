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
        /// Creates a company from just a signature, using the signature as the name.
        /// </summary>
        /// <param name="signature">The signature to use.</param>
        /// <returns>A new company with the signature as both name and signature.</returns>
        public static Company FromSignature(string signature) =>
            new(0, signature, signature);
    }
}
