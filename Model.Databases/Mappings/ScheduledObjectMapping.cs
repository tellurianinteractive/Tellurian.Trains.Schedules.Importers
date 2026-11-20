using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class ScheduledObjectMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A vehicle and the schedules it is assigned to.</summary>
        internal void MapScheduledObject() =>
            builder.Entity<ScheduledObject>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Class).HasMaxLength(50);
                entity.Property(e => e.Remark).HasMaxLength(500);
                entity.Property(e => e.ExternalId).HasMaxLength(100);

                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ScheduleAssignments)
                      .WithOne(e => e.ScheduledObject)
                      .HasForeignKey(e => e.ScheduledObjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                // The ordered rake of scheduled units. ScheduledUnit is a polymorphic hierarchy (Wagon,
                // TractionUnit), which owned types cannot express, so it is a regular TPH entity related to
                // the scheduled object through a shadow foreign key.
                entity.HasMany(e => e.Units)
                      .WithOne()
                      .HasForeignKey("ScheduledObjectId")
                      .OnDelete(DeleteBehavior.Cascade);
            });
    }
}
