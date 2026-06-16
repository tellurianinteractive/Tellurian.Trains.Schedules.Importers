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
        CountryCode = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Company"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the company.</param>
    /// <param name="name">The full name of the company.</param>
    /// <param name="signature">The short signature or abbreviation for the company.</param>
    /// <param name="countryCode">The ISO country code where the company is based.</param>
    public Company(int id, string name, string signature, string countryCode)
    {
        Id = id;
        Name = name;
        Signature = signature;
        CountryCode = countryCode;
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
    /// Gets or sets the ISO country code where the company is based.
    /// </summary>
    public string CountryCode { get; set; }

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
        /// Gets a display name including the company name, signature, and country code.
        /// </summary>
        public string DisplayName =>
             $"{company.Name} ({company.Signature}-{company.CountryCode})";

        /// <summary>
        /// Creates a company from just a signature, using the signature as the name.
        /// </summary>
        /// <param name="signature">The signature to use.</param>
        /// <returns>A new company with the signature as both name and signature.</returns>
        public static Company FromSignature(string signature) =>
            new(0, signature, signature, "");
    }
}
