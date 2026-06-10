using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a railway vehicle (locomotive or trainset) that can be assigned to trains.
/// </summary>
public class ScheduledObject : IEquatable<ScheduledObject>
{
    /// <summary>
    /// Gets or sets the unique identifier for this vehicle.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the vehicle number.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets an optional external identifier for this vehicle.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the number of units that make up this vehicle (for multiple-unit trainsets).
    /// </summary>
    public int NumberOfUnits { get; set; }

    public int ReplaceOrder { get; set; }

    /// <summary>
    /// Gets or sets an optional remark about this vehicle.
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets the type of this vehicle.
    /// </summary>
    public ScheduledObjectType ObjectType { get; set; }

    /// <summary>
    /// Gets or sets the class designation of this vehicle.
    /// </summary>
    public string Class { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this vehicle can operate in both directions.
    /// </summary>
    public bool IsDoubleDirected { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the owning company. Optional.
    /// </summary>
    public int? CompanyId { get; set; }

    /// <summary>
    /// Gets or sets the company that owns this vehicle.
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the owning schedule. Required.
    /// </summary>
    public int PlanId { get; set; }

    /// <summary>
    /// Gets or sets the schedule this vehicle belongs to.
    /// </summary>
    public Plan Plan { get; set; } = default!;

    /// <summary>
    /// Gets or sets the collection of schedule assignments for this vehicle.
    /// </summary>
    [JsonInclude]
    public ICollection<ScheduleAssignment> ScheduleAssignments { get; private set; } = [];

    /// <inheritdoc/>
    /// <remarks>
    /// A vehicle is identified solely by its source <see cref="ExternalId"/>, which is how vehicles are
    /// uniquely identified in XPLN (the raw text of the locomotive/trainset column). The parsed
    /// <see cref="Number"/> and company are deliberately not used: the identifier format differs between
    /// XPLN files (e.g. "Co-LOK 123" versus "Co_GLok"), so the number cannot be parsed reliably and some
    /// vehicles have no number at all, which would otherwise merge distinct vehicles.
    /// </remarks>
    public bool Equals(ScheduledObject? other) =>
        other is not null &&
        ObjectType == other.ObjectType &&
        string.Equals(ExternalId, other.ExternalId, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ScheduledObject other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(ObjectType, ExternalId?.ToUpperInvariant());

    [JsonConstructor]
    private ScheduledObject()
    {
        ScheduleAssignments = new HashSet<ScheduleAssignment>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ScheduledObject"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the vehicle.</param>
    /// <param name="vehicleType">The type of vehicle.</param>
    /// <param name="number">The vehicle number.</param>
    public ScheduledObject(int id, ScheduledObjectType vehicleType, int number)
    {
        Id = id;
        ObjectType = vehicleType;
        Number = number;
        ScheduleAssignments = new HashSet<ScheduleAssignment>();
    }
}

/// <summary>
/// Specifies the type of railway vehicle.
/// </summary>
public enum ScheduledObjectType
{
    /// <summary>
    /// Vehicle type is not specified.
    /// </summary>
    Unknown,

    /// <summary>
    /// A locomotive that pulls or pushes other vehicles.
    /// </summary>
    Locomotive,

    /// <summary>
    /// A self-propelled trainset (multiple unit).
    /// </summary>
    Trainset,

    /// <summary>
    /// A self-propelled railcar. In XPLN this is identified by the same identifier appearing in both
    /// the locomotive and the trainset section; such entries are merged into a single railcar.
    /// </summary>
    Railcar,
    PassengerCar,
    CargoWagon,
    Cargo,
}

/// <summary>
/// 
/// </summary>
public static class ScheduledObjectTypeExtensions
{
    extension(string? value)
    {
        /// <summary>
        /// Tries to convert a string value to a <see cref="ScheduledObjectType"/>.
        /// </summary>
        public ScheduledObjectType ToScheduledObjectType => Enum.TryParse<ScheduledObjectType>(value, out var result) ? result : ScheduledObjectType.Unknown;
    }
}
