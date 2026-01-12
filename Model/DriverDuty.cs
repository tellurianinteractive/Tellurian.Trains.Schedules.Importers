using System.Globalization;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a driver's duty consisting of train parts and associated notes.
/// </summary>
public class DriverDuty : IEquatable<DriverDuty>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private DriverDuty()
    {
        Identity = string.Empty;
        Parts = [];
        Notes = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DriverDuty"/> with the specified id and identity.
    /// </summary>
    /// <param name="id">The unique identifier for the driver duty.</param>
    /// <param name="identity">The display identity for the duty. Uses id if null or empty.</param>
    public DriverDuty(int id, string? identity)
    {
        Id = id;
        Identity = identity.HasValue ? identity : id.ToString();
        Parts = [];
        Notes = [];
    }

    /// <summary>
    /// Gets or sets the unique identifier for this driver duty.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display identity for this duty.
    /// </summary>
    public string Identity { get; set; }

    /// <summary>
    /// Gets or sets the sessions during which this duty is active.
    /// </summary>
    public Sessions Sessions { get; set; } = Sessions.All;

    /// <summary>
    /// Gets or sets the collection of train parts in this duty.
    /// </summary>
    public ICollection<TrainPart> Parts { get; set; }

    /// <summary>
    /// Gets or sets the collection of notes for this duty.
    /// </summary>
    public ICollection<DriverDutyNote> Notes { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the company performing this duty. Optional.
    /// </summary>
    public int? CompanyId { get; set; }

    /// <summary>
    /// Gets or sets the company performing this duty.
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the owning schedule.
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the schedule this duty belongs to.
    /// </summary>
    public Schedule Schedule { get; set; } = default!;

    /// <inheritdoc/>
    public bool Equals(DriverDuty? other) => Identity.Equals(other?.Identity, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DriverDuty other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Identity.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() =>
        Parts.Count == 0 ? Identity :
        string.Format(CultureInfo.CurrentCulture,
            "{0}: {1} - {2}", Identity, Parts.First().Departure, Parts.Last().Arrival);
}

/// <summary>
/// Provides extension methods for <see cref="DriverDuty"/>.
/// </summary>
public static class DriverDutyExtensions
{
    extension(DriverDuty duty)
    {
        /// <summary>
        /// Adds a train part to the driver duty.
        /// </summary>
        /// <param name="part">The train part to add.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the part if added successfully, or an error message if overlapping.</returns>
        public Maybe<TrainPart> Add(TrainPart part)
        {
            duty = duty.ValueOrException(nameof(duty));
            part = part.ValueOrException(nameof(part));
            if (!duty.Parts.Contains(part))
            {
                if (part.IsOverlapping(duty.Parts)) return new Maybe<TrainPart>($"Part {part} overlaps existing parts in driver duty '{duty.Identity}'");
                part.Duty = duty;
                part.DutyId = duty.Id;
                duty.Parts.Add(part);
            }
            return new Maybe<TrainPart>(part);
        }
    }
}
