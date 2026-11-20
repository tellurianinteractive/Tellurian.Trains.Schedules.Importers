using Microsoft.EntityFrameworkCore;
using Tellurian.Trains.Schedules.Importers.Interfaces;

namespace Tellurian.Trains.Schedules.Model.Databases;

/// <summary>
/// Provides services for exporting schedule data to a database using Entity Framework Core.
/// </summary>
/// <remarks>
/// This service implements <see cref="IExportService"/> to persist schedule data
/// to any database supported by Entity Framework Core.
/// </remarks>
/// <param name="options">The database context options used to create the <see cref="ScheduleDbContext"/>.</param>
public class DatabaseExportService(DbContextOptions<ScheduleDbContext> options) : IExportService
{
    private readonly DbContextOptions<ScheduleDbContext> _options = options;

    /// <summary>
    /// Exports <paramref name="schedule"/> to the database, which holds one plan: whatever it held is replaced.
    /// </summary>
    /// <remarks>
    /// A plan's ids are local to it and are the keys in the database, so a database can't hold two plans. Before
    /// saving, the plan is prepared as for writing to a file (<see cref="Plan"/>'s <c>PrepareForWriting</c>): every
    /// track, call and train part is given an id of its own, and the references by id are brought in line.
    /// </remarks>
    /// <param name="schedule">The plan to export.</param>
    /// <returns>The result of the export, with the exported plan or the error.</returns>
    public async Task<ExportResult<Plan>> ExportScheduleAsync(Plan schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        try
        {
            schedule.PrepareForWriting();
            await using var context = new ScheduleDbContext(_options);
            await context.Database.EnsureDeletedAsync().ConfigureAwait(false);
            await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
            context.Plans.Add(schedule);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return ExportResult<Plan>.Success(schedule);
        }
        catch (DbUpdateException ex)
        {
            return ExportResult<Plan>.Failure(ex.InnerException?.Message ?? ex.Message);
        }
    }
}
