using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class ScheduleMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A vehicle schedule and its train parts, in order.</summary>
        internal void MapSchedule() =>
            builder.Entity<Schedule>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);

                entity.HasMany(e => e.Parts)
                      .WithOne(e => e.Schedule)
                      .HasForeignKey(e => e.ScheduleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
    }
}
