using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

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

    public Company(int id, string name, string signature, string countryCode)
    {
        Id = id;
        Name = name;
        Signature = signature;
        CountryCode = countryCode;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Signature { get; set; }
    public string CountryCode { get; set; }

    // FK property for EF Core
    public int LayoutId { get; set; }
    public Layout Layout { get; set; } = default!;

    public bool Equals(Company? other) => other is not null && Signature.Equals(other.Signature, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is Company other && Equals(other);
    public override int GetHashCode() => Signature.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => $"{Name} ({Signature})";
}

public static class CompanyExtensions
{
    extension(Company company)
    {
        public string DisplayName =>
             $"{company.Name} ({company.Signature}-{company.CountryCode})";

        public static Company FromSignature(string signature) =>
            new(0, signature, signature, "");
    }
}
