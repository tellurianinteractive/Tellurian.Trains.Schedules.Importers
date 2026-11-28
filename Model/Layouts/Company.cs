using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>A railway company that operates trains and vehicles on the layout. Stored in <see cref="Timetables.Timetable.Companies"/> and referred to by id.</summary>
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

    /// <summary>
    /// The short name, e.g. "SJ". Not unique: one company may appear several times with the same signature,
    /// e.g. for different periods with different logos. <see cref="Id"/> identifies the company.
    /// </summary>
    public string Signature { get; set; }

    /// <summary>The id of the company's country, or null.</summary>
    public int? CountryId { get; set; }

    /// <summary>The company's logo, as a data URL, or null.</summary>
    public string? Logo { get; set; }

    /// <summary>
    /// The first year the company operates, or null when it has no known start. Together with
    /// <see cref="OperatesToYear"/> it tells apart companies that share a <see cref="Signature"/>,
    /// e.g. one per period with a logo of its own.
    /// </summary>
    public int? OperatesFromYear { get; set; }

    /// <summary>The last year the company operates (inclusive), or null when it has no known end.</summary>
    public int? OperatesToYear { get; set; }

    /// <summary>
    /// Whether this company and <paramref name="other"/> operate in at least one common year. A missing
    /// year leaves the period open at that end, so a company without years overlaps every other.
    /// </summary>
    public bool OperatesInSamePeriodAs(Company other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return (OperatesFromYear ?? int.MinValue) <= (other.OperatesToYear ?? int.MaxValue)
            && (other.OperatesFromYear ?? int.MinValue) <= (OperatesToYear ?? int.MaxValue);
    }

    /// <summary>
    /// The operating period as text, e.g. "1988–2008", "1988–" or "–2008", or empty when no years are given.
    /// </summary>
    [JsonIgnore]
    public string OperatingPeriodText =>
        OperatesFromYear is null && OperatesToYear is null ? string.Empty
        : OperatesFromYear == OperatesToYear ? $"{OperatesFromYear}"
        : $"{OperatesFromYear}–{OperatesToYear}";

    /// <summary>The foreign key of the timetable. Not written: the timetable owns the company.</summary>
    [JsonIgnore]
    public int TimetableId { get; set; }

    /// <summary>The timetable the company belongs to. Not written: the timetable owns the company.</summary>
    [JsonIgnore]
    public Timetables.Timetable Timetable { get; set; } = default!;

    /// <inheritdoc/>
    /// <remarks>
    /// Companies are the same when they have the same <see cref="Id"/>; signatures may repeat. A company
    /// not yet given an id (0, before the database assigns one) is only the same as itself.
    /// </remarks>
    public bool Equals(Company? other) =>
        other is not null && (ReferenceEquals(this, other) || Id != 0 && Id == other.Id);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Company other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({Signature})";
}
