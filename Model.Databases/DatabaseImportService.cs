using Microsoft.EntityFrameworkCore;
using Tellurian.Trains.Schedules.Importers.Interfaces;

namespace Tellurian.Trains.Schedules.Model.Databases;

/// <summary>
/// Reads a plan saved by <see cref="DatabaseExportService"/> back from the database.
/// </summary>
/// <param name="options">The database context options used to create the <see cref="ScheduleDbContext"/>.</param>
public class DatabaseImportService(DbContextOptions<ScheduleDbContext> options) : IImportService
{
    private readonly DbContextOptions<ScheduleDbContext> _options = options;

    /// <summary>Reads the plan named <paramref name="name"/>.</summary>
    public async Task<ImportResult<Plan>> ImportScheduleAsync(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        await using var context = new ScheduleDbContext(_options);
        var plan = await context.ReadPlanAsync(name).ConfigureAwait(false);
        return plan is null
            ? ImportResult<Plan>.Failure(Message.Error($"There is no plan named '{name}'."))
            : ImportResult<Plan>.Success(plan);
    }
}

/// <summary>Reading a whole plan from a <see cref="ScheduleDbContext"/>.</summary>
public static class PlanReading
{
    extension(ScheduleDbContext context)
    {
        /// <summary>
        /// The plan named <paramref name="name"/> with all it holds, or null when there is none. Each part of the plan
        /// is loaded with a query of its own into the same context, which joins them; then the plan is reconciled as
        /// one read from a file is.
        /// </summary>
        public async Task<Plan?> ReadPlanAsync(string name, CancellationToken cancellationToken = default)
        {
            var plan = await context.Plans
                .Include(p => p.Timetable).ThenInclude(t => t.Layout)
                .SingleOrDefaultAsync(p => p.Name == name, cancellationToken).ConfigureAwait(false);
            if (plan is null) return null;

            var planId = plan.Id;
            var timetableId = plan.TimetableId;
            var layoutId = plan.Timetable.LayoutId;

            await context.Regions.Where(r => EF.Property<int>(r, "LayoutId") == layoutId).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.OperationLocations.Where(l => l.LayoutId == layoutId).Include(l => l.Tracks).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.Stations.Where(s => s.LayoutId == layoutId).Include(s => s.Regions).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.TrackStretches.Where(s => s.LayoutId == layoutId).LoadAsync(cancellationToken).ConfigureAwait(false);

            await context.Companies.Where(c => c.TimetableId == timetableId).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.TimetableStretches.Where(s => EF.Property<int>(s, "TimetableId") == timetableId).Include(s => s.Stretches).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.DispatchStretches.Where(s => EF.Property<int>(s, "TimetableId") == timetableId).Include(s => s.Stretches).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.TrainCategories.Where(c => EF.Property<int>(c, "TimetableId") == timetableId).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.Set<CargoFlowOptions>().Where(o => EF.Property<int>(o, "TimetableId") == timetableId).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.Trains.Where(t => t.TimetableId == timetableId).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.StationCalls.Where(c => c.Train.TimetableId == timetableId).Include(c => c.Notes).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.TrainParts.Where(p => p.From.Train.TimetableId == timetableId).LoadAsync(cancellationToken).ConfigureAwait(false);

            await context.ScheduledObjects.Where(o => o.PlanId == planId).Include(o => o.Units).Include(o => o.ScheduleAssignments).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.Schedules.Where(s => s.PlanId == planId).LoadAsync(cancellationToken).ConfigureAwait(false);
            await context.DriverDuties.Where(d => d.PlanId == planId).Include(d => d.Parts).Include(d => d.Notes).LoadAsync(cancellationToken).ConfigureAwait(false);

            // The lists in the order they were saved in; the plan is then no longer tracked.
            ListPositions.RestoreAndDetach(context.ChangeTracker);
            // What the layout refers to by id in its JSON columns: the location of a hall plan's module, the station a
            // lock key is held at.
            plan.Timetable.Layout.RestoreReferences();
            plan.Reconcile();
            return plan;
        }
    }
}
