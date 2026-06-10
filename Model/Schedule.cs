using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a schedule of train parts that a vehicle is assigned to operate.
/// </summary>
public sealed class Schedule : IEquatable<Schedule>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private Schedule()
    {
        Parts = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Model.Schedule"/> with the specified id.
    /// </summary>
    /// <param name="id">The unique identifier for the vehicle schedule.</param>
    public Schedule(int id)
    {
        Id = id;
        Parts = [];
    }

    /// <summary>
    /// Gets or sets the unique identifier for this vehicle schedule.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the owning schedule. Required.
    /// </summary>
    public int PlanId { get; set; }

    /// <summary>
    /// Gets or sets the schedule this vehicle schedule belongs to.
    /// </summary>
    public Plan Plan { get; set; } = default!;

    /// <summary>
    /// Gets or sets the collection of train parts in this vehicle schedule.
    /// </summary>
    public ICollection<TrainPart> Parts { get; set; }

    /// <inheritdoc/>
    public bool Equals(Schedule? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Schedule other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => $"Schedule {Id}";
}

/// <summary>
/// Provides extension methods for <see cref="Schedule"/>.
/// </summary>
public static class VehicleScheduleExtensions
{
    /// <summary>
    /// Adds a train part to the vehicle schedule.
    /// </summary>
    /// <param name="me">The vehicle schedule to add to.</param>
    /// <param name="part">The train part to add.</param>
    /// <returns>The added train part.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the vehicle schedule or part is null.</exception>
    public static TrainPart? Add(this Schedule me, TrainPart? part)
    {
        if (me == null || part is null) throw new ArgumentNullException(nameof(part));
        part.Schedule = me;
        part.ScheduleId = me.Id;
        if (!me.Parts.Contains(part))
        {
            me.Parts.Add(part);
        }
        return part;
    }
}
