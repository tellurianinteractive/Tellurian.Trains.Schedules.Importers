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
    /// Asynchronously exports the specified schedule to a database supported by Entity Framework Core.
    /// </summary>
    /// <param name="schedule">The schedule to export. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous export operation. The task result contains an <see
    /// cref="ExportResult{Schedule}"/> object with details about the export, including the exported schedule and any
    /// error messages.</returns>
    public async Task<ExportResult<Schedule>> ExportScheduleAsync(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        try
        {
            await using var context = new ScheduleDbContext(_options);
            await context.Database.EnsureCreatedAsync();

            ClearStretchesToAvoidConstraintIssues(schedule);
            await SaveTrainCategoriesAsync(context, schedule);
            await SaveScheduleAsync(context, schedule);

            return ExportResult<Schedule>.Success(schedule);
        }
        catch (DbUpdateException ex)
        {
            return ExportResult<Schedule>.Failure(ex.InnerException?.Message ?? ex.Message);
        }
    }

    private static void ClearStretchesToAvoidConstraintIssues(Schedule schedule)
    {
        schedule.Timetable.Layout.TrackStretches.Clear();
        schedule.Timetable.Layout.TimetableStretches.Clear();
    }

    private static async Task SaveTrainCategoriesAsync(ScheduleDbContext context, Schedule schedule)
    {
        var categories = schedule.Timetable.Trains
            .Where(t => t.Category is not null)
            .Select(t => t.Category!)
            .DistinctBy(c => c.Id)
            .ToList();

        foreach (var category in categories)
        {
            var existing = await context.TrainCategories.FindAsync(category.Id);
            if (existing is null)
            {
                context.TrainCategories.Add(category);
            }
        }

        if (categories.Count > 0)
        {
            await context.SaveChangesAsync();
        }
    }

    private static async Task SaveScheduleAsync(ScheduleDbContext context, Schedule schedule)
    {
        var existingSchedule = await context.Schedules
            .FirstOrDefaultAsync(s => s.Name == schedule.Name);

        if (existingSchedule is not null)
        {
            context.Schedules.Remove(existingSchedule);
            await context.SaveChangesAsync();
        }

        context.Schedules.Add(schedule);
        await context.SaveChangesAsync();
    }
}
