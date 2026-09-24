using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Who brings one rolling stock item of a <see cref="Plan"/> to the meeting: its
/// <see cref="Contributors"/> in order, the first of them the primary one, and a note about the item as a
/// whole. Kept in <see cref="Plan.VehicleContributions"/> and tied to its vehicle by
/// <see cref="ScheduledObjectId"/> alone.
/// </summary>
public sealed class VehicleContribution
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private VehicleContribution()
    {
        Note = string.Empty;
        Contributors = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="VehicleContribution"/> for a vehicle with no contributors yet.
    /// </summary>
    /// <param name="scheduledObjectId">The <see cref="ScheduledObject.Id"/> of the rolling stock item.</param>
    public VehicleContribution(int scheduledObjectId)
    {
        ScheduledObjectId = scheduledObjectId;
        Note = string.Empty;
        Contributors = [];
    }

    /// <summary>
    /// Gets or sets the <see cref="ScheduledObject.Id"/> of the rolling stock item this is about.
    /// </summary>
    public int ScheduledObjectId { get; set; }

    /// <summary>
    /// Gets or sets a note about the item as a whole, whoever brings it. Empty when there is none.
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// The participants bringing the item, in order: the first brings the unit and sets it up on the layout,
    /// every later one brings a spare. Empty while nobody has been found for it.
    /// </summary>
    [JsonInclude]
    public IList<VehicleContributor> Contributors { get; private set; }
}

/// <summary>
/// Provides extension members for <see cref="VehicleContribution"/>.
/// </summary>
public static class VehicleContributionExtensions
{
    extension(VehicleContribution contribution)
    {
        /// <summary>
        /// The contributor who brings the unit and sets it up on the layout, or <c>null</c> when there is none.
        /// </summary>
        public VehicleContributor? PrimaryContributor => contribution.Contributors.FirstOrDefault();

        /// <summary>
        /// The contributors who bring a spare unit, in the order they were entered.
        /// </summary>
        public IEnumerable<VehicleContributor> SpareContributors => contribution.Contributors.Skip(1);

        /// <summary>
        /// Whether <paramref name="contributor"/> is the primary contributor.
        /// </summary>
        /// <param name="contributor">The contributor to ask about.</param>
        public bool IsPrimary(VehicleContributor contributor) =>
            ReferenceEquals(contribution.PrimaryContributor, contributor);

        /// <summary>
        /// Whether the contribution says nothing at all — no contributor and no note — so that it need not be
        /// kept.
        /// </summary>
        public bool IsEmpty => contribution.Contributors.Count == 0 && contribution.Note.IsEmpty;
    }
}
