using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Represents a railway vehicle (locomotive or trainset) that can be assigned to trains.
/// </summary>
public class ScheduledObject : IEquatable<ScheduledObject>, ITranslatable
{
    /// <summary>
    /// The display-label key for a vehicle is its <see cref="ScheduledObjectType"/> (e.g. Locomotive,
    /// Wagonset), not the class name, so each kind is labelled distinctly. See <see cref="ITranslatable"/>.
    /// </summary>
    string ITranslatable.TranslationKey => ObjectType.ToString();

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

    /// <summary>
    /// if greather that zero, defines a spare scheduled object.
    /// </summary>
    public int ReplaceOrder { get; set; }

    /// <summary>
    /// Gets or sets an optional remark about this vehicle.
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets the type of this vehicle.
    /// </summary>
    public ScheduledObjectType ObjectType
    {
        get; set
        {
            field = value;
            if (field.IsTraction && TractionType == TractionType.None) TractionType = TractionType.Undefined;
            else if (!field.IsTraction) TractionType = TractionType.None;
        }
    }

    /// <summary>
    /// Gets or sets the traction type for traction units; otherwise None.
    /// </summary>
    public TractionType TractionType { get; set; } = TractionType.Undefined;

    /// <summary>
    /// Gets or sets the class designation of this vehicle.
    /// </summary>
    public string Class { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this vehicle can operate in both directions.
    /// </summary>
    public bool IsDoubleDirected { get; set; }

    /// <summary>
    /// Gets or sets the DCC address of this vehicle. Applies to motorised vehicles only
    /// (locomotives, trainsets, railcars); <c>null</c> for non-motorised vehicles.
    /// </summary>
    public int? DccAddress { get; set; }

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

    /// <summary>
    /// Full description of <see cref="ScheduledObject"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{Company?.Signature} {Class} {Number}".Trim();

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
/// 
/// </summary>
public static class ScheduledObjectExtensions
{
    extension(ScheduledObject scheduledObject)
    {
        /// <summary>
        /// True if <see cref="ScheduledObject"/> is a traction unit.
        /// </summary>
        public bool IsTraction => scheduledObject.ObjectType.IsTraction;
        /// <summary>
        /// True if <see cref="ScheduledObject"/> is a non-traction unit.
        /// </summary>
        public bool IsWagonSet => scheduledObject.ObjectType.IsWagonSet;
        /// <summary>
        /// True if <see cref="ScheduledObject"/> is freight wagons directed by waybills.
        /// </summary>
        public bool IsCargoFlow => scheduledObject.ObjectType.IsCargoFlow;
        /// <summary>
        /// True if <see cref="ScheduledObject"/> is cargo without rolling stock.
        /// </summary>
        public bool IsCargoOnly => scheduledObject.ObjectType.IsCargoOnly;
        /// <summary>
        /// Determines if a turnus card should be printed for this object.
        /// </summary>
        public bool HasTurnusCard => !scheduledObject.IsCargoFlow;
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
    /// <remarks>In XPLN this is identified by the same identifier appearing in both
    /// the locomotive and the trainset section; such entries are merged into a single railcar.</remarks>
    Trainset,

    /// <summary>
    /// A multiple unit of wagons operated as one.
    /// </summary>
    Wagonset,

    /// <summary>
    /// A non-specified amount of wagons with origin and destination on waybills.
    /// </summary>
    CargoFlow,

    /// <summary>
    /// A unit of cargo (non-rolling stock)
    /// </summary>
    Cargo,
}

/// <summary>
/// Only applies to <see cref="ScheduledObject"/> that is a traction unit.
/// </summary>

public enum TractionType
{
    /// <summary>
    /// Undefined, assumed can operate on all stretches. Also the default value for traction units.
    /// </summary>
    Undefined,
    /// <summary>
    /// Default and only value for non-traction units.
    /// </summary>
    None,
    /// <summary>
    /// Steam locomotive(s) or trainset(s)
    /// </summary>
    Steam,
    /// <summary>
    /// Diesel locomotive(s) or trainset(s)
    /// </summary>
    Diesel,
    /// <summary>
    /// Electric locomotive(s) or trainset(s), restricted to operate on electrified stretches.
    /// </summary>
    Electric,
    /// <summary>
    /// Locomotive that can operate both etectric and diesel modes.
    /// </summary>
    Dual,
    /// <summary>
    /// Battery powered locomotive(s) or trainset(s)
    /// </summary>
    Battery
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

    extension(ScheduledObjectType type)
    {
        /// <summary>
        /// True if <see cref="ScheduledObjectType"/> is a kind of traction unit.
        /// </summary>
        public bool IsTraction => type == ScheduledObjectType.Locomotive || type == ScheduledObjectType.Trainset;

        /// <summary>
        /// True if <see cref="ScheduledObjectType"/> is cargo only, i.e.a nunit of cargo non-rolling stock.
        /// </summary>
        public bool IsCargoOnly => type == ScheduledObjectType.Cargo;

        /// <summary>
        /// True if <see cref="ScheduledObjectType"/> is a wagonset with one or several wagons. 
        /// </summary>
        public bool IsWagonSet => type == ScheduledObjectType.Wagonset;

        /// <summary>
        /// True if <see cref="ScheduledObjectType"/> is a cargo flow, i.e. wagons directed by waybill destinations.
        /// </summary>
        public bool IsCargoFlow => type == ScheduledObjectType.CargoFlow;
    }
}
