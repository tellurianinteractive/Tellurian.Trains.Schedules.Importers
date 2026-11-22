using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class ScheduleAssignmentMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A vehicle's assignment to a schedule.</summary>
        internal void MapScheduleAssignment() =>
            builder.Entity<ScheduleAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property<int>(ListPositions.Column);

                entity.Property(e => e.Sessions).HasConversion(Conversions.Sessions);

                entity.HasOne(e => e.Schedule)
                      .WithMany()
                      .HasForeignKey(e => e.ScheduleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
    }
}
