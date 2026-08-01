using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Represents a complete railway schedule containing timetables, vehicles, vehicle schedules, and driver duties.
/// </summary>
/// <remarks>
/// A schedule is the top-level container for all railway operation planning data.
/// It associates a timetable with the vehicles and personnel needed to operate the trains.
/// </remarks>
public class Plan : IEquatable<Plan>, IJsonOnSerializing, IJsonOnDeserialized
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private Plan()
    {
        Name = string.Empty;
        Timetable = default!;
        ScheduledObjects = [];
        Schedules = [];
        DriverDuties = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Plan"/> with the specified name and timetable.
    /// </summary>
    /// <param name="name">The name of the schedule.</param>
    /// <param name="timetable">The timetable associated with this schedule.</param>
    public Plan(string name, Timetable timetable)
    {
        Name = name;
        Timetable = timetable;
        TimetableId = timetable.Id;
        ScheduledObjects = [];
        Schedules = [];
        DriverDuties = [];
    }

    /// <summary>
    /// Creates a new schedule with the specified name and timetable.
    /// </summary>
    /// <param name="name">The name of the schedule.</param>
    /// <param name="timetable">The timetable associated with this schedule.</param>
    /// <returns>A new <see cref="Plan"/> instance.</returns>
    public static Plan Create(string name, Timetable timetable) =>
        new(name, timetable);

    /// <summary>
    /// Gets or sets the unique identifier for this schedule.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this schedule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the foreign key to the associated timetable.
    /// </summary>
    public int TimetableId { get; set; }

    /// <summary>
    /// Gets or sets the timetable associated with this schedule.
    /// </summary>
    public Timetable Timetable { get; set; }

    /// <summary>
    /// Gets or sets the authored standing instructions for the whole meeting, as markdown: signalling
    /// practice, radio or telephone use, shunting rules, what to do when running late, who to ask.
    /// Empty when none have been written.
    /// </summary>
    /// <remarks>
    /// These are printed as their own booklet and handed to every participant before the first session,
    /// station staff included — not bound into each driver's duty booklet. Their audience is wider than
    /// the drivers, they are identical in every duty, and they are read once beforehand rather than
    /// carried through a session.
    /// </remarks>
    public string GeneralInstructions { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authored programme for the meeting, as markdown: session times, breaks, meals,
    /// and anything else participants need to know in advance. Empty when none has been written.
    /// </summary>
    /// <remarks>
    /// Printed on the front page of the general instructions booklet, alongside the meeting name and
    /// validity dates — the first thing a participant reads, before the standing instructions.
    /// </remarks>
    public string Program { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of vehicles available in this schedule.
    /// </summary>
    public ICollection<ScheduledObject> ScheduledObjects { get; set; }

    /// <summary>
    /// Gets or sets the collection of vehicle schedules defining how vehicles are assigned to trains.
    /// </summary>
    public ICollection<Schedule> Schedules { get; set; }

    /// <summary>
    /// Gets or sets the collection of driver duties for this schedule.
    /// </summary>
    public ICollection<DriverDuty> DriverDuties { get; set; }

    /// <summary>
    /// Completes a plan as soon as it has been read, so it is never looked at half-resolved.
    /// </summary>
    /// <remarks>
    /// The vehicles and the driver duties are read after the timetable, so their companies can only be
    /// resolved here — <see cref="Timetable"/> has already reconciled its own by the time this runs.
    /// The company catalogue is reconciled first, because a plan written by an earlier version stored
    /// its companies on whatever referred to them rather than in the catalogue.
    /// </remarks>
    /// <summary>
    /// Completes both catalogues before a plan is written, so that writing one can never lose what it
    /// holds.
    /// </summary>
    /// <remarks>
    /// A category and a company are written only in their catalogue (see <c>PlanJson</c>), so a plan
    /// whose catalogue does not yet hold everything its trains, vehicles and duties use would write
    /// those entries nowhere at all and read back without them. That is the state an importer leaves a
    /// plan in, and the state every plan saved before the catalogues were kept was read in — reconciling
    /// on the way out means no path can save a plan that has not been put right first.
    /// </remarks>
    void IJsonOnSerializing.OnSerializing()
    {
        Timetable?.RebuildTrainCategories();
        this.RebuildCompanies();
    }

    void IJsonOnDeserialized.OnDeserialized()
    {
        this.RebuildCompanies();
        this.ResolveCatalogueReferences();
        // Only here can the stop flags the train parts depend on be put right: the parts the vehicle
        // schedules and driver duties are planned over are read after the timetable.
        this.ApplyStopRules();
    }

    /// <inheritdoc/>
    public bool Equals(Plan? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Plan other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>
/// Provides extension methods for <see cref="Plan"/>.
/// </summary>
public static class PlanExtensions
{
    extension(Plan plan)
    {
        /// <summary>
        /// Returns the <see cref="Layout"/> for the plan.
        /// </summary>
        public Layout Layout => plan.Timetable.Layout;

        /// <summary>
        /// Reconciles the company catalogue (<see cref="Layouts.Layout.Companies"/>) with the companies
        /// the plan's trains, train categories, vehicles and driver duties actually refer to: a company
        /// referred to but not in the catalogue is added to it, and every company is then given an id
        /// that is unique and greater than zero. Call this whenever a plan is loaded or imported.
        /// </summary>
        /// <remarks>
        /// A company is stored once, in the catalogue, and everything that operates under it keeps only
        /// its id — so two companies sharing an id are one company as far as a saved plan is concerned.
        /// They already were as far as anything reading the id was: a plan carrying two companies left
        /// on id zero had their trains counted as one operator when checked for duplicate train numbers.
        /// A company that means something outside the plan keeps the id it came with; only those with
        /// none of their own are numbered.
        /// </remarks>
        public void RebuildCompanies()
        {
            plan = plan.ValueOrException(nameof(plan));
            if (plan.Timetable?.Layout is not { } layout) return;
            Catalogue.Reconcile(
                layout.Companies,
                plan.Timetable.TrainCategories.Select(c => c.Company)
                    .Concat(plan.Timetable.Trains.Select(t => t.Company))
                    .Concat(plan.ScheduledObjects.Select(v => v.Company))
                    .Concat(plan.DriverDuties.Select(d => d.Company)),
                company => company.Id,
                (company, id) => company.Id = id);
        }

        /// <summary>
        /// Re-establishes the company on each vehicle and driver duty from the id it is stored with.
        /// The timetable resolves its own (see
        /// <see cref="Timetables.TimetableExtensions.ResolveCatalogueReferences"/>); these are read
        /// after it, so they can only be resolved once the whole plan is there. A company that survived
        /// the reading — from a plan written by an earlier version — is left alone.
        /// </summary>
        public void ResolveCatalogueReferences()
        {
            plan = plan.ValueOrException(nameof(plan));
            if (plan.Timetable?.Layout?.Companies is not { } companies) return;
            foreach (var vehicle in plan.ScheduledObjects)
                vehicle.Company ??= companies.FirstOrDefault(c => c.Id == vehicle.CompanyId);
            foreach (var duty in plan.DriverDuties)
                duty.Company ??= companies.FirstOrDefault(c => c.Id == duty.CompanyId);
        }

        /// <summary>
        /// Brings a whole plan into a consistent state, before anything validates, displays or saves it:
        /// the per-track call index is rebuilt from the trains, the catalogue references are put back,
        /// the category and company catalogues are reconciled with what the plan actually uses, and the
        /// stop flags the train parts depend on are set (see <see cref="Validations.StopRules"/>).
        /// Idempotent.
        /// </summary>
        /// <remarks>
        /// Reading a plan does all of this on its way (see the <c>OnDeserialized</c> of
        /// <see cref="Plan"/> and <see cref="Timetables.Timetable"/>), so this is for the plans that
        /// arrive some other way — from an importer above all, which builds a plan rather than reading
        /// one, and need not have filled either catalogue in.
        /// </remarks>
        public void Reconcile()
        {
            plan = plan.ValueOrException(nameof(plan));
            if (plan.Timetable is { } timetable)
            {
                timetable.RebuildStationCalls();
                timetable.ResolveCatalogueReferences();
                timetable.RebuildTrainCategories();
            }
            plan.RebuildCompanies();
            plan.ResolveCatalogueReferences();
            plan.ApplyStopRules();
        }
        /// <summary>
        /// Adds a vehicle to the schedule.
        /// </summary>
        /// <param name="vehicle">The vehicle to add.</param>
        /// <returns>The added vehicle.</returns>
        public ScheduledObject AddVehicle(ScheduledObject vehicle)
        {
            plan = plan.ValueOrException(nameof(plan));
            vehicle = vehicle.ValueOrException(nameof(vehicle));
            if (!plan.ScheduledObjects.Contains(vehicle))
            {
                vehicle.Plan = plan;
                vehicle.PlanId = plan.Id;
                plan.ScheduledObjects.Add(vehicle);
            }
            return vehicle;
        }

        /// <summary>
        /// Adds a vehicle schedule to the schedule.
        /// </summary>
        /// <param name="vehicleSchedule">The vehicle schedule to add.</param>
        /// <returns>The added vehicle schedule.</returns>
        public Schedule AddVehicleSchedule(Schedule vehicleSchedule)
        {
            plan = plan.ValueOrException(nameof(plan));
            vehicleSchedule = vehicleSchedule.ValueOrException(nameof(vehicleSchedule));
            if (!plan.Schedules.Contains(vehicleSchedule))
            {
                vehicleSchedule.Plan = plan;
                vehicleSchedule.PlanId = plan.Id;
                plan.Schedules.Add(vehicleSchedule);
            }
            return vehicleSchedule;
        }

        /// <summary>
        /// Adds a driver duty to the schedule.
        /// </summary>
        /// <param name="driverDuty">The driver duty to add.</param>
        /// <returns>The added driver duty.</returns>
        public DriverDuty AddDriverDuty(DriverDuty driverDuty)
        {
            plan = plan.ValueOrException(nameof(plan));
            driverDuty = driverDuty.ValueOrException(nameof(driverDuty));
            if (!plan.DriverDuties.Contains(driverDuty))
            {
                driverDuty.Plan = plan;
                driverDuty.PlanId = plan.Id;
                plan.DriverDuties.Add(driverDuty);
            }
            return driverDuty;
        }

        /// <summary>
        /// Gets or creates call notes for train arriving at an operation location.
        /// </summary>
        /// <param name="call"></param>
        /// <returns></returns>
        public IEnumerable<CallNote> CallNotesForArrivalAt(StationCall call)
        {
            return call.Notes;
            // TODO: Add LINQ for creating the rest of the notes.
        }

        /// <summary>
        /// Gets all <see cref="ScheduledObject">schedule objects</see> associated with a <see cref="ScheduledTrainPart"/>.
        /// </summary>
        /// <param name="trainPart"></param>
        /// <returns></returns>
        public IEnumerable<ScheduledObject> ScheduledObjectsFor(ScheduledTrainPart trainPart)
        {
            return plan.ScheduledObjects
                .Where(so => so.ScheduleAssignments
                .Any(sa => sa.Schedule.Parts.Contains(trainPart)));
        }
    }





}
