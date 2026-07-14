using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Detailed info about a specific locomotive or wagon in a train.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Wagon), nameof(Wagon))]
[JsonDerivedType(typeof(TractionUnit), nameof(TractionUnit))]
public abstract class ScheduledUnit : ITranslatable
{
    /// <summary>
    /// The position in the <see cref="ScheduledObject.Units"/> train in the forward (arranged) direction, 1..n. The order is reversed in reports
    /// for segments run in the backward direction. Settable because the rake is re-sequenced on reorder.
    /// </summary>
    public int Position { get; set; }
    /// <summary>
    /// Type of wagon according to standard class definitions.
    /// </summary>
    public string Class { get; set; } = string.Empty;
    /// <summary>
    /// Optional indivdual number of wagon.
    /// </summary>
    public string? Number { get; set; }
    /// <summary>
    /// Name of type of <see cref="ScheduledUnit"/>
    /// </summary>
    string ITranslatable.TranslationKey => GetType().Name;
}

/// <summary>
/// Unit for <see cref="ScheduledObjectType.Wagonset"/>
/// </summary>
public sealed class Wagon : ScheduledUnit
{
    /// <summary>
    /// If true this is a passenger wagon.
    /// </summary>
    /// <remarks>A wagon can be both for passenger and cargo</remarks>
    public bool IsPassenger { get; set; }
    /// <summary>
    /// If true this is a freight wagon.
    /// </summary>
    /// <remarks>A wagon can be both for passenger and cargo</remarks>
    public bool IsCargo { get; set; }
}

/// <summary>
/// Unit for <see cref="ScheduledObjectType.Wagonset"/>
/// </summary>
public class TractionUnit : ScheduledUnit
{
    /// <summary>
    /// <see cref="TractionType"/> for the trainset.
    /// </summary>
    public TractionType TractionType { get; set; } = TractionType.Undefined;
}

/// <summary>
/// The presentation order of a wagon rake over a segment, relative to the order the wagons are arranged
/// in the first train of the schedule.
/// </summary>
public enum UnitOrder
{
    /// <summary>
    /// The wagons are shown in the order they are arranged in the first train (ascending <see cref="ScheduledUnit.Position"/>).
    /// </summary>
    AsArranged,

    /// <summary>
    /// The wagons are shown in the reverse of their arranged order, because the train travels the segment in
    /// the opposite direction (descending <see cref="ScheduledUnit.Position"/>).
    /// </summary>
    Reversed,
}
