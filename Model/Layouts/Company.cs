using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Represents a railway company that operates trains on a layout.
/// </summary>
public class Company : IEquatable<Company>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private Company()
    {
        Name = string.Empty;
        Signature = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Company"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the company.</param>
    /// <param name="name">The full name of the company.</param>
    /// <param name="signature">The short signature or abbreviation for the company.</param>
    /// <param name="countryId">The <see cref="Country.Id"/> of the country the company belongs to,
    /// or <c>null</c> when not specified.</param>
    public Company(int id, string name, string signature, int? countryId = null)
    {
        Id = id;
        Name = name;
        Signature = signature;
        CountryId = countryId;
    }

    /// <summary>
    /// Gets or sets the unique identifier for this company.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the full name of the company.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the short signature or abbreviation for the company.
    /// </summary>
    public string Signature { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Country.Id"/> of the country the company belongs to, or
    /// <c>null</c> when not specified. The country is resolved through the layout's saved catalogue
    /// (see <c>Layout.CountryById</c>). A company operating in several countries is added once per
    /// country of operation.
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the associated layout.
    /// </summary>
    public int LayoutId { get; set; }

    /// <summary>
    /// Gets or sets the layout on which this company operates.
    /// </summary>
    public Layout Layout { get; set; } = default!;

    /// <inheritdoc/>
    public bool Equals(Company? other) => other is not null && Signature.Equals(other.Signature, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Company other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Signature.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({Signature})";
}

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
