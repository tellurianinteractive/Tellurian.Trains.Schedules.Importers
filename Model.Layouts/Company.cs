using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>A railway company that operates trains and vehicles on the layout. Stored in <see cref="Layout.Companies"/> and referred to by id.</summary>
public sealed class Company : IEquatable<Company>
{
    /// <summary>For JSON and EF Core.</summary>
    [JsonConstructor]
    private Company()
    {
        Name = string.Empty;
        Signature = string.Empty;
    }

    /// <summary>Creates a company.</summary>
    public Company(int id, string name, string signature, int? countryId = null)
    {
        Id = id;
        Name = name;
        Signature = signature;
        CountryId = countryId;
    }

    /// <summary>The identity of the company within the layout.</summary>
    public int Id { get; set; }

    /// <summary>The full name, e.g. "Statens Järnvägar".</summary>
    public string Name { get; set; }

    /// <summary>The short name, e.g. "SJ". Unique within the layout, without regard to case.</summary>
    public string Signature { get; set; }

    /// <summary>The id of the company's country, or null.</summary>
    public int? CountryId { get; set; }

    /// <summary>The company's logo, as a data URL, or null.</summary>
    public string? Logo { get; set; }

    /// <summary>The foreign key of the layout. Not written: the layout owns the company.</summary>
    [JsonIgnore]
    public int LayoutId { get; set; }

    /// <summary>The layout the company belongs to. Not written: the layout owns the company.</summary>
    [JsonIgnore]
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
