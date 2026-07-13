using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

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
    /// Initializes a new instance of <see cref="Schedule"/> with the specified id.
    /// </summary>
    /// <param name="id">The unique identifier for the vehicle schedule.</param>
    public Schedule(int id)
    {
        Id = id;
        Parts = [];
    }

    /// <summary>
    /// Gets or sets the unique identifier for this vehicle schedule. This is the stable stored
    /// reference and never changes; equality is based on it. Editing <see cref="Number"/> does not
    /// affect identity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the displayed schedule number. It defaults to the number of the first train added
    /// to the schedule (see <see cref="ScheduleExtensions.Add"/> and
    /// <see cref="ScheduleExtensions.Append"/>) and may then be freely changed by the user. It is
    /// a label only and need not be unique — <see cref="Id"/> carries identity.
    /// </summary>
    public int Number { get; set; }

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
    public ICollection<ScheduledTrainPart> Parts { get; set; }

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
public static class ScheduleExtensions
{
    extension(Schedule schedule)
    {
        /// <summary>
        /// Gets the parts of the schedule ordered by departure, i.e. as the vehicle works them.
        /// </summary>
        public IReadOnlyList<ScheduledTrainPart> OrderedParts =>
            [.. schedule.Parts.OrderBy(p => p.From.Departure)];

        /// <summary>
        /// Gets the first part the vehicle works (earliest departure), or <c>null</c> when the
        /// schedule is empty.
        /// </summary>
        public ScheduledTrainPart? FirstPart =>
            schedule.Parts.Count == 0 ? null : schedule.Parts.MinBy(p => p.From.Departure);

        /// <summary>
        /// Gets the last part the vehicle works (latest arrival), or <c>null</c> when the schedule is
        /// empty. A new part is chained onto this one.
        /// </summary>
        public ScheduledTrainPart? LastPart =>
            schedule.Parts.Count == 0 ? null : schedule.Parts.MaxBy(p => p.To.Arrival);

        /// <summary>
        /// Gets the location the vehicle starts from, or <c>null</c> when the schedule is empty.
        /// </summary>
        public OperationLocation? StartLocation => schedule.FirstPart?.From.OperationLocation;

        /// <summary>
        /// Gets the location the vehicle ends at, or <c>null</c> when the schedule is empty.
        /// </summary>
        public OperationLocation? EndLocation => schedule.LastPart?.To.OperationLocation;

        /// <summary>
        /// Gets the departure time of the first part, or <c>null</c> when the schedule is empty.
        /// </summary>
        public Time? FirstDeparture => schedule.FirstPart?.From.Departure;

        /// <summary>
        /// Gets the arrival time of the last part, or <c>null</c> when the schedule is empty.
        /// </summary>
        public Time? LastArrival => schedule.LastPart?.To.Arrival;

        /// <summary>
        /// Gets the vehicles worked to this schedule, resolved through their
        /// <see cref="ScheduledObject.ScheduleAssignments"/>. An XPLN-imported schedule has exactly one
        /// vehicle; the model permits several to share a working. Empty when the schedule is detached
        /// from its plan.
        /// </summary>
        public IEnumerable<ScheduledObject> Vehicles =>
            schedule.Plan?.ScheduledObjects
                .Where(so => so.ScheduleAssignments.Any(a => schedule.Equals(a.Schedule)))
            ?? [];

        /// <summary>
        /// True when the schedule's vehicles are all cargo flows (freight wagons directed by waybills).
        /// Such a schedule is a circulation of cargo, not of a turning vehicle, and is therefore
        /// excluded from the vehicle turn chart (but still printed as a turnus card elsewhere).
        /// </summary>
        public bool IsCargoFlow =>
            schedule.Vehicles.Any() && schedule.Vehicles.All(v => v.IsCargoFlow);

        /// <summary>
        /// Gets the sessions/days on which the vehicle can actually work the whole schedule: the
        /// intersection of every part's train's <see cref="Train.Sessions"/>, i.e. the sessions on which
        /// all of them operate. An empty schedule operates on all sessions. A train can only be appended
        /// while its own sessions still overlap this common set (see <see cref="Append"/>).
        /// </summary>
        public Sessions EffectiveSessions =>
            schedule.Parts.Count == 0
                ? Sessions.All
                : schedule.Parts.Select(p => p.Train.Sessions).Aggregate((common, s) => common.And(s));

        /// <summary>
        /// Adds a train part to the schedule without continuity or overlap checks.
        /// </summary>
        /// <remarks>
        /// This is the unconditional insert used when reconstructing schedules from trusted data (such
        /// as the XPLN import), where the source is authoritative and no part may be dropped. Interactive
        /// building goes through <see cref="Append"/>, which enforces that the schedule stays a single
        /// contiguous, non-overlapping working. Adding sets the schedule's <see cref="Schedule.Number"/>
        /// from the first part's train number when the number has not yet been set; a part already
        /// present is returned unchanged.
        /// </remarks>
        /// <param name="part">The train part to add.</param>
        /// <returns>The added part (or the already-present equal part).</returns>
        /// <exception cref="ArgumentNullException">Thrown when the part is null.</exception>
        public ScheduledTrainPart Add(ScheduledTrainPart part)
        {
            part = part.ValueOrException(nameof(part));
            if (!schedule.Parts.Contains(part)) schedule.Attach(part);
            return part;
        }

        /// <summary>
        /// Appends a train part to the end of the vehicle's working, enforcing that the schedule stays a
        /// single contiguous, non-overlapping run.
        /// </summary>
        /// <remarks>
        /// A part is accepted only when it does not overlap the existing parts in time and, for a
        /// non-empty schedule, starts where the schedule currently ends and departs at or after that
        /// arrival. The first part of an empty schedule is unconstrained. Accepting the first part sets
        /// the schedule's <see cref="Schedule.Number"/> from its train number when the number has not
        /// yet been set. A part already present is returned unchanged. This is the method used when
        /// building schedules automatically or interactively.
        /// </remarks>
        /// <param name="part">The train part to append.</param>
        /// <returns>A <see cref="Maybe{T}"/> with the part when appended (or already present), or an
        /// error message explaining why it was rejected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the part is null.</exception>
        public Maybe<ScheduledTrainPart> Append(ScheduledTrainPart part)
        {
            part = part.ValueOrException(nameof(part));
            if (schedule.Parts.Contains(part)) return new Maybe<ScheduledTrainPart>(part);
            if (part.IsOverlapping(schedule.Parts))
                return new Maybe<ScheduledTrainPart>($"Part {part} overlaps existing parts in schedule {schedule.Number}.");
            var last = schedule.LastPart;
            if (last is not null)
            {
                if (!part.From.OperationLocation.Equals(last.To.OperationLocation))
                    return new Maybe<ScheduledTrainPart>($"Part {part} does not start where schedule {schedule.Number} ends ({last.To.OperationLocation.Signature}).");
                if (part.From.Departure < last.To.Arrival)
                    return new Maybe<ScheduledTrainPart>($"Part {part} departs before schedule {schedule.Number} arrives at {last.To.OperationLocation.Signature} {last.To.Arrival.HHMM()}.");
                // The vehicle can only work a part on sessions it also works the rest of the schedule; the
                // train must therefore still share at least one session/day with the parts already added.
                if (!schedule.EffectiveSessions.Overlaps(part.Train.Sessions))
                    return new Maybe<ScheduledTrainPart>($"Part {part} operates on no session/day common to schedule {schedule.Number}.");
            }
            schedule.Attach(part);
            return new Maybe<ScheduledTrainPart>(part);
        }

        private void Attach(ScheduledTrainPart part)
        {
            part.Schedule = schedule;
            part.ScheduleId = schedule.Id;
            schedule.Parts.Add(part);
            if (schedule.Number == 0) schedule.Number = part.Train.Number;
        }
    }

    extension(IEnumerable<Schedule> schedules)
    {
        internal bool HasSameVehicle(StationCall one, StationCall another)
        {
            var foundOne = schedules.FindVehicleSchedule(one);
            var foundOther = schedules.FindVehicleSchedule(another);
            return foundOne is not null && foundOther is not null && foundOne == foundOther;
        }

        internal Schedule? FindVehicleSchedule(StationCall call)
        {
            if (schedules == null) return null;
            foreach (var schedule in schedules)
            {
                foreach (var part in schedule.Parts)
                {
                    if (part.ContainsCall(call)) return schedule;
                }
            }
            return null;
        }
    }
}
