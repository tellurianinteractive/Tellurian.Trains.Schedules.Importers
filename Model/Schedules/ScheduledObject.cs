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
    /// Gets or sets the scheduled object instance number.
    /// Must be unique withing a <see cref="Company"/> or unique withing the <see cref="Plan"/> if no company is specified. 
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets an optional external identifier for this vehicle.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the number of units that make up this vehicle (for multiple-unit trainsets).
    /// </summary>
    public int NumberOfUnits { get { return Units.Count != 0 ? Units.Count : field; } set; }

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

    /// <summary>
    /// The ordered rake of individual <see cref="ScheduledUnit"/> that make up this vehicle when it is a
    /// runs the whole schedule the wagonset is assigned to; <see cref="ScheduledUnit.Position"/> is the arranged
    /// order in the first train of the schedule (reversed in reports for segments run in the opposite
    /// direction — see <c>WagonOrderByLeg</c> in the Planning layer).
    /// </summary>
    [JsonInclude]
    public ICollection<ScheduledUnit> Units { get; private set; } = [];

    /// <inheritdoc/>
    public bool Equals(ScheduledObject? other)
    {
        if (other is null) return false;
        if (Id > 0 && other.Id > 0) return Id == other.Id;
        return ObjectType == other.ObjectType &&
            ExternalId.EqualsCaseInsensitive(other.ExternalId);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ScheduledObject other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HasExternalId ? HashCode.Combine(ObjectType, ExternalId!.ToUpperInvariant()) : Id.GetHashCode();

    /// <summary>
    /// True when the vehicle carries an <see cref="ExternalId"/>. Its <see cref="Designation"/> is then the
    /// external id (as imported, and usually worth editing into a tidy designation); when false the
    /// designation is the composed company signature, class and number.
    /// </summary>
    public bool HasExternalId => !string.IsNullOrWhiteSpace(ExternalId);

    /// <summary>
    /// The vehicle's identity for display and validation messages: its <see cref="ExternalId"/> (how a
    /// vehicle is identified everywhere else in the app, e.g. "DBSCH EG 01") when it has one, otherwise a
    /// composed identity of the operating company's signature, class and number. More informative and
    /// consistent than <see cref="Number"/> alone, which is not unique across vehicle types.
    /// </summary>
    public string Designation =>
        !string.IsNullOrWhiteSpace(ExternalId) ? ExternalId : ComposedIdentity;

    // The identity composed when a vehicle carries no external id: the operating company's signature
    // followed by the class and number (e.g. "DB BR 218 05"). The number is padded to at least two digits
    // so short numbers line up. A vehicle with no class shows just the number; surrounding whitespace is
    // trimmed away.
    private string ComposedIdentity
    {
        get
        {
            var number = Number.ToString("D2");
            var @class = string.IsNullOrWhiteSpace(Class) ? number : $"{Class} {number}";
            return $"{Company?.Signature} {@class}".Trim();
        }
    }

    /// <summary>
    /// Full description of <see cref="ScheduledObject"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString() => ComposedIdentity;

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

        /// <summary>
        /// <see cref="TractionUnit">Traction units</see> that makes up this <see cref="ScheduledObject"/>
        /// </summary>
        public IEnumerable<TractionUnit> TractionUnits => scheduledObject.Units.OfType<TractionUnit>();

        /// <summary>
        /// <see cref="Wagon">Wagons</see> that makes up this <see cref="ScheduledObject"/>
        /// </summary>
        public IEnumerable<Wagon> Wagons => scheduledObject.Units.OfType<Wagon>();
        /// <summary>
        /// Adds a wagon to the end of the rake and re-sequences <see cref="ScheduledUnit.Position"/> to 1..n.
        /// </summary>
        /// <param name="class">The wagon class (e.g. a UIC class letter code); required.</param>
        /// <param name="number">An optional individual wagon number.</param>
        /// <param name="isPassenger">True for a passenger wagon.</param>
        /// <param name="isCargo">True for a freight wagon (a wagon may be both).</param>
        /// <returns>The added wagon.</returns>
        public Wagon AddWagon(string @class, string? number = null, bool isPassenger = false, bool isCargo = false)
        {
            var unit = new Wagon
            {
                Class = @class,
                Position = scheduledObject.Units.Count + 1,
                Number = number,
                IsPassenger = isPassenger,
                IsCargo = isCargo,
            };
            scheduledObject.Units.Add(unit);
            scheduledObject.ResequenceUnits();
            return unit;
        }

        /// <summary>
        /// Adds a traction unit to the end of the rake and re-sequences <see cref="ScheduledUnit.Position"/> to 1..n.
        /// </summary>
        /// <param name="class">The traction unit class (e.g. a UIC class letter code); required.</param>
        /// <param name="number">An optional individual wagon number.</param>
        /// <param name="tractionType"></param>
        /// <returns></returns>
        public TractionUnit AddTractionUnit(string @class, string? number = null, TractionType tractionType = TractionType.Undefined)
        {
            var unit = new TractionUnit
            {
                Class = @class,
                Position = scheduledObject.Units.Count + 1,
                Number = number,
                TractionType = tractionType,
            };
            scheduledObject.Units.Add(unit);
            scheduledObject.ResequenceUnits();
            return unit;
        }

        /// <summary>
        /// Removes a wagon from the rake and re-sequences <see cref="ScheduledUnit.Position"/> to 1..n.
        /// </summary>
        public void RemoveUnit(ScheduledUnit unit)
        {
            scheduledObject.Units.Remove(unit);
            scheduledObject.ResequenceUnits();
        }

        /// <summary>
        /// Renumbers <see cref="ScheduledUnit.Position"/> to a contiguous 1..n sequence in the rake's current
        /// order, after an add, remove or reorder. The arranged order is the order in the first train of the
        /// schedule; reports reverse it per travel direction.
        /// </summary>
        public void ResequenceUnits()
        {
            var position = 1;
            foreach (var wagon in scheduledObject.Units.OrderBy(w => w.Position).ToList())
                wagon.Position = position++;
        }

        /// <summary>
        /// Gets the union of the sessions across all this vehicle's <see cref="ScheduleAssignment"/> — the
        /// sessions on which it is already working somewhere. Empty when the vehicle is not yet assigned.
        /// </summary>
        public Sessions AssignedSessions =>
            scheduledObject.ScheduleAssignments.Select(a => a.Sessions).Aggregate(new Sessions(), (acc, s) => acc.Or(s));

        /// <summary>
        /// Gets the distinct session/day combinations this vehicle works, each paired with the train parts it
        /// works on those sessions in departure order — one turnus card per combination. Every session/day of
        /// the operating period is attributed to the parts the vehicle actually works on it, gathered across
        /// <em>all</em> its <see cref="ScheduledObject.ScheduleAssignments"/> and restricted, per assignment, to the sessions the
        /// assignment covers and to each part's own <see cref="Train.Sessions"/>. Sessions on which the vehicle
        /// works the identical set of parts form one combination; sessions on which it works nothing yield none.
        /// The combinations are ordered by their first session/day.
        /// </summary>
        /// <param name="useDays">Whether the operating period is measured in weekdays rather than session numbers.</param>
        /// <param name="maxSessions">The number of sessions/days in the operating period (1-14).</param>
        public IReadOnlyList<SessionCombination> SessionCombinations(bool useDays, int maxSessions)
        {
            var periodMax = Math.Clamp(maxSessions, 1, useDays ? 7 : 14);
            var groups = new List<SessionCombinationBuilder>();
            for (var number = 1; number <= periodMax; number++)
            {
                var parts = scheduledObject.ScheduleAssignments
                    .Where(a => a.Sessions.Includes(number))
                    .SelectMany(a => a.Schedule?.Parts ?? [])
                    .Where(p => p.Train.Sessions.Includes(number))
                    .ToHashSet();
                if (parts.Count == 0) continue;
                var group = groups.FirstOrDefault(g => g.Parts.SetEquals(parts));
                if (group is null) groups.Add(group = new SessionCombinationBuilder(parts));
                group.Numbers.Add(number);
            }
            return [.. groups.Select(g => new SessionCombination(
                SessionsExtensions.FromPeriodNumbers(g.Numbers, useDays),
                [.. g.Parts.OrderBy(p => p.Departure?.Value)]))];
        }
    }

    // Accumulates the operating-period numbers on which a vehicle works one identical set of parts, while
    // the numbers are grouped into session/day combinations.
    private sealed class SessionCombinationBuilder(HashSet<ScheduledTrainPart> parts)
    {
        public HashSet<ScheduledTrainPart> Parts { get; } = parts;
        public List<int> Numbers { get; } = [];
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
