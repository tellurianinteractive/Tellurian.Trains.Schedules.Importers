using Microsoft.EntityFrameworkCore;
using Tellurian.Trains.Schedules.Model.Databases.Mappings;

namespace Tellurian.Trains.Schedules.Model.Databases;

/// <summary>
/// Entity Framework Core database context for persisting railway schedule data.
/// </summary>
/// <remarks>
/// This context provides access to all schedule-related entities organized into three layers:
/// <list type="bullet">
/// <item><description>Layout layer: Physical infrastructure (layouts, stations, tracks, stretches)</description></item>
/// <item><description>Timetable layer: Train scheduling (timetables, trains, station calls)</description></item>
/// <item><description>Schedule layer: Operational planning (schedules, vehicles, driver duties)</description></item>
/// </list>
/// </remarks>
/// <param name="options">The options to be used by this <see cref="DbContext"/>.</param>
public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{

    /// <summary>
    /// Gets the set of railway layouts in the database.
    /// </summary>
    public DbSet<Layout> Layouts => Set<Layout>();

    /// <summary>
    /// Gets the set of railway companies in the database.
    /// </summary>
    public DbSet<Company> Companies => Set<Company>();

    /// <summary>
    /// Gets the set of regions (the per-layout catalogue) in the database.
    /// </summary>
    public DbSet<Region> Regions => Set<Region>();

    /// <summary>
    /// Gets the set of operation locations (stations) in the database.
    /// </summary>
    public DbSet<OperationLocation> OperationLocations => Set<OperationLocation>();

    /// <summary>
    /// Gets the set of stations in the database.
    /// </summary>
    public DbSet<Station> Stations => Set<Station>();

    /// <summary>
    /// Gets the set of signal controlled locations in the database.
    /// </summary>
    public DbSet<SignalControlledLocation> SignalControlledLocations => Set<SignalControlledLocation>();

    /// <summary>
    /// Gets the set of other locations in the database.
    /// </summary>
    public DbSet<OtherLocation> OtherLocations => Set<OtherLocation>();

    /// <summary>
    /// Gets the set of dispatch stretches in the database.
    /// </summary>
    public DbSet<DispatchStretch> DispatchStretches => Set<DispatchStretch>();

    /// <summary>
    /// Gets the set of station tracks in the database.
    /// </summary>
    public DbSet<StationTrack> StationTracks => Set<StationTrack>();

    /// <summary>
    /// Gets the set of track stretches between stations in the database.
    /// </summary>
    public DbSet<TrackStretch> TrackStretches => Set<TrackStretch>();

    /// <summary>
    /// Gets the set of timetable stretches (logical groupings of track stretches) in the database.
    /// </summary>
    public DbSet<TimetableStretch> TimetableStretches => Set<TimetableStretch>();

    /// <summary>
    /// Gets the set of timetables in the database.
    /// </summary>
    public DbSet<Timetable> Timetables => Set<Timetable>();

    /// <summary>
    /// Gets the set of train categories in the database.
    /// </summary>
    public DbSet<TrainCategory> TrainCategories => Set<TrainCategory>();

    /// <summary>
    /// Gets the set of trains in the database.
    /// </summary>
    public DbSet<Train> Trains => Set<Train>();

    /// <summary>
    /// Gets the set of station calls (scheduled stops) in the database.
    /// </summary>
    public DbSet<StationCall> StationCalls => Set<StationCall>();

    /// <summary>
    /// Gets the set of schedules in the database.
    /// </summary>
    public DbSet<Plan> Plans => Set<Plan>();

    /// <summary>
    /// Gets the set of vehicles (locomotives and trainsets) in the database.
    /// </summary>
    public DbSet<ScheduledObject> ScheduledObjects => Set<ScheduledObject>();

    /// <summary>
    /// Gets the set of vehicle schedule assignments in the database.
    /// </summary>
    public DbSet<ScheduleAssignment> ScheduleAssignments => Set<ScheduleAssignment>();

    /// <summary>
    /// Gets the set of vehicle schedules in the database.
    /// </summary>
    public DbSet<Schedule> Schedules => Set<Schedule>();

    /// <summary>
    /// Gets the set of driver duties in the database.
    /// </summary>
    public DbSet<DriverDuty> DriverDuties => Set<DriverDuty>();

    /// <summary>
    /// Gets the set of train parts in the database. This is the root of the train-part hierarchy;
    /// use <c>OfType&lt;ScheduledTrainPart&gt;()</c> or <c>OfType&lt;CargoFlowTrainPart&gt;()</c> to
    /// query a single kind.
    /// </summary>
    public DbSet<TrainPart> TrainParts => Set<TrainPart>();

    /// <summary>
    /// Gets the set of call notes in the database.
    /// </summary>
    public DbSet<CallNote> CallNotes => Set<CallNote>();

    /// <summary>
    /// Gets the set of text call notes in the database.
    /// </summary>
    public DbSet<TextCallNote> TextCallNotes => Set<TextCallNote>();

    /// <summary>
    /// Gets the set of driver duty notes in the database.
    /// </summary>
    public DbSet<DriverDutyNote> DriverDutyNotes => Set<DriverDutyNote>();

    /// <inheritdoc/>
    /// <remarks>The ordered lists are numbered first (see <see cref="ListPositions"/>).</remarks>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ListPositions.Number(ChangeTracker);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc/>
    /// <remarks>The ordered lists are numbered first (see <see cref="ListPositions"/>).</remarks>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ListPositions.Number(ChangeTracker);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Configures the entity model: one mapping per entity, in <c>Mappings/</c>.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.MapCallNote();
        modelBuilder.MapCargoFlowOptions();
        modelBuilder.MapCargoFlowTrainPart();
        modelBuilder.MapCompany();
        modelBuilder.MapDispatchStretch();
        modelBuilder.MapDriverDuty();
        modelBuilder.MapDriverDutyNote();
        modelBuilder.MapLayout();
        modelBuilder.MapOperationLocation();
        modelBuilder.MapPlan();
        modelBuilder.MapRegion();
        modelBuilder.MapSchedule();
        modelBuilder.MapScheduleAssignment();
        modelBuilder.MapScheduledObject();
        modelBuilder.MapScheduledTrainPart();
        modelBuilder.MapScheduledUnit();
        modelBuilder.MapStation();
        modelBuilder.MapStationCall();
        modelBuilder.MapStationTrack();
        modelBuilder.MapTimetable();
        modelBuilder.MapTimetableStretch();
        modelBuilder.MapTrackStretch();
        modelBuilder.MapTrain();
        modelBuilder.MapTrainCategory();
        modelBuilder.MapTrainPart();
    }
}
