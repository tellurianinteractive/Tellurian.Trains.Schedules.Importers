using System.Globalization;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Duties;

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
    public ICollection<ScheduledTrainPart> Parts { get; set; }

    /// <summary>
    /// Gets or sets the collection of notes for this duty.
    /// </summary>
    public ICollection<DriverDutyNote> Notes { get; set; }

    /// <summary>
    /// Gets or sets an explicit start time that overrides the derived one. When <c>null</c> the duty
    /// starts at its first part's first-call arrival (see the <c>StartTime</c> extension).
    /// </summary>
    public Time? OverriddenStartTime { get; set; }

    /// <summary>
    /// Gets or sets an explicit end time that overrides the derived one. When <c>null</c> the duty
    /// ends at its last part's last-call departure (see the <c>EndTime</c> extension).
    /// </summary>
    public Time? OverriddenEndTime { get; set; }

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
    public int PlanId { get; set; }

    /// <summary>
    /// Gets or sets the schedule this duty belongs to.
    /// </summary>
    public Plan Plan { get; set; } = default!;

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
        /// Adds a train part to the driver duty without overlap or session checks.
        /// </summary>
        /// <remarks>
        /// This is the unconditional insert used when reconstructing duties from trusted data (such as the
        /// XPLN import). A part is a shared <see cref="ScheduledTrainPart"/> that stays owned by its vehicle
        /// <see cref="Schedule"/> (which resolves its traction unit); the same part may be referenced by
        /// several duties. Interactive building goes through <see cref="Append"/>, which enforces the
        /// duty invariants. A part already present is returned unchanged.
        /// </remarks>
        /// <param name="part">The train part to add.</param>
        /// <returns>The added part (or the already-present equal part).</returns>
        public ScheduledTrainPart Add(ScheduledTrainPart part)
        {
            duty = duty.ValueOrException(nameof(duty));
            part = part.ValueOrException(nameof(part));
            if (!duty.Parts.Contains(part)) duty.Parts.Add(part);
            return part;
        }

        /// <summary>
        /// Appends a train part to the duty, enforcing the duty invariants: a part may not overlap in time
        /// with the parts already in the duty (a driver cannot be in two places at once), and a part may be
        /// worked by another duty only on sessions this duty does not run (a segment is driven by exactly
        /// one driver per session). Parts need not be geographically contiguous — the driver walks between
        /// them. A part already present is returned unchanged.
        /// </summary>
        /// <param name="part">The train part to append.</param>
        /// <returns>A <see cref="Maybe{T}"/> with the part when appended (or already present), or an error
        /// message explaining why it was rejected.</returns>
        public Maybe<ScheduledTrainPart> Append(ScheduledTrainPart part)
        {
            duty = duty.ValueOrException(nameof(duty));
            part = part.ValueOrException(nameof(part));
            if (duty.Parts.Contains(part)) return new Maybe<ScheduledTrainPart>(part);
            if (part.IsOverlapping(duty.Parts))
                return new Maybe<ScheduledTrainPart>($"Part {part} overlaps existing parts in driver duty '{duty.Identity}'.");
            if (duty.Plan is { } plan && plan.DriverDuties.Any(other =>
                    !other.Equals(duty) && other.Parts.Contains(part) && other.Sessions.Overlaps(duty.Sessions)))
                return new Maybe<ScheduledTrainPart>($"Part {part} is already worked by another duty on a session driver duty '{duty.Identity}' also runs.");
            duty.Parts.Add(part);
            return new Maybe<ScheduledTrainPart>(part);
        }

        /// <summary>Gets the duty's parts ordered by departure, i.e. as the driver works them.</summary>
        public IReadOnlyList<ScheduledTrainPart> OrderedParts =>
            [.. duty.Parts.OrderBy(p => p.From.Departure)];

        /// <summary>Gets the first part the driver works (earliest departure), or <c>null</c> when empty.</summary>
        public ScheduledTrainPart? FirstPart =>
            duty.Parts.Count == 0 ? null : duty.Parts.MinBy(p => p.From.Departure);

        /// <summary>Gets the last part the driver works (latest arrival), or <c>null</c> when empty.</summary>
        public ScheduledTrainPart? LastPart =>
            duty.Parts.Count == 0 ? null : duty.Parts.MaxBy(p => p.To.Arrival);

        /// <summary>Gets the location the driver starts from, or <c>null</c> when the duty is empty.</summary>
        public OperationLocation? StartLocation => duty.FirstPart?.From.OperationLocation;

        /// <summary>Gets the location the driver ends at, or <c>null</c> when the duty is empty.</summary>
        public OperationLocation? EndLocation => duty.LastPart?.To.OperationLocation;

        /// <summary>Gets the departure time of the first part, or <c>null</c> when the duty is empty.</summary>
        public Time? FirstDeparture => duty.FirstPart?.From.Departure;

        /// <summary>Gets the arrival time of the last part, or <c>null</c> when the duty is empty.</summary>
        public Time? LastArrival => duty.LastPart?.To.Arrival;

        /// <summary>
        /// Gets the duty's derived start time: the first part's first-call arrival — the time the driver
        /// reports at the origin, before the train departs (usually a "not arrival"). <c>null</c> when the
        /// duty is empty. This is the default; see the <c>StartTime</c> extension for the effective value.
        /// </summary>
        public Time? DefaultStartTime => duty.FirstPart?.From.Arrival;

        /// <summary>
        /// Gets the duty's derived end time: the last part's last-call departure — the time the driver stands
        /// down at the destination, after the train arrives (usually a "not departure"). <c>null</c> when the
        /// duty is empty. This is the default; see the <c>EndTime</c> extension for the effective value.
        /// </summary>
        public Time? DefaultEndTime => duty.LastPart?.To.Departure;

        /// <summary>
        /// Gets the duty's effective start time: <see cref="DriverDuty.OverriddenStartTime"/> when set,
        /// otherwise the <c>DefaultStartTime</c> extension.
        /// </summary>
        public Time? StartTime => duty.OverriddenStartTime ?? duty.DefaultStartTime;

        /// <summary>
        /// Gets the duty's effective end time: <see cref="DriverDuty.OverriddenEndTime"/> when set,
        /// otherwise the <c>DefaultEndTime</c> extension.
        /// </summary>
        public Time? EndTime => duty.OverriddenEndTime ?? duty.DefaultEndTime;

        /// <summary>
        /// Removes a single part from the duty. The part itself is untouched (it stays owned by its vehicle
        /// schedule and may still be worked by other duties). A part not in the duty is ignored.
        /// </summary>
        public void RemovePart(ScheduledTrainPart part)
        {
            duty = duty.ValueOrException(nameof(duty));
            part = part.ValueOrException(nameof(part));
            duty.Parts.Remove(part);
        }

        /// <summary>Adds a manual duty-level note (applies to the whole duty, unlike per-call notes).</summary>
        public DriverDutyNote AddNote(string text)
        {
            duty = duty.ValueOrException(nameof(duty));
            var note = new DriverDutyNote(text ?? string.Empty)
            {
                Id = (duty.Notes.Count == 0 ? 0 : duty.Notes.Max(n => n.Id)) + 1,
                DriverDuty = duty,
                DriverDutyId = duty.Id,
            };
            duty.Notes.Add(note);
            return note;
        }

        /// <summary>Removes a manual duty-level note. A note not in the duty is ignored.</summary>
        public void RemoveNote(DriverDutyNote note)
        {
            duty = duty.ValueOrException(nameof(duty));
            duty.Notes.Remove(note);
        }

        /// <summary>
        /// Derives the traction-unit exchange notes for this duty: where two consecutive parts of the
        /// <em>same</em> train, joined at a station, are worked by <em>different</em> traction units, the
        /// driver stays with the train while the traction unit changes. One note is produced at the junction
        /// station (the earlier part's arrival). Computed, never stored. Empty when the duty is detached
        /// from its plan (traction cannot be resolved without it).
        /// </summary>
        public IEnumerable<TractionUnitExchangeNote> TractionExchangeNotes
        {
            get
            {
                if (duty.Plan is not { } plan) yield break;
                var ordered = duty.OrderedParts;
                for (var i = 0; i < ordered.Count - 1; i++)
                {
                    var a = ordered[i];
                    var b = ordered[i + 1];
                    if (!a.Train.Equals(b.Train)) continue;
                    if (!a.To.OperationLocation.Equals(b.From.OperationLocation)) continue;
                    var fromUnit = plan.ScheduledObjectsFor(a).FirstOrDefault(so => so.IsTraction);
                    var toUnit = plan.ScheduledObjectsFor(b).FirstOrDefault(so => so.IsTraction);
                    if (fromUnit is null || toUnit is null || fromUnit.Equals(toUnit)) continue;
                    yield return new TractionUnitExchangeNote(a.To.OperationLocation, fromUnit, toUnit);
                }
            }
        }
    }
}
