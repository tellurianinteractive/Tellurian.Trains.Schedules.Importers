using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class DriverDutyMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A driver duty, the train parts it works, in order, and its notes.</summary>
        internal void MapDriverDuty() =>
            builder.Entity<DriverDuty>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Identity).HasMaxLength(50).IsRequired();

                entity.Property(e => e.Sessions).HasConversion(Conversions.Sessions);

                // Effective start/end derive from the parts; only the optional manual overrides are stored.
                entity.Property(e => e.OverriddenStartTime).HasConversion(Conversions.Time);
                entity.Property(e => e.OverriddenEndTime).HasConversion(Conversions.Time);

                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // A train part may be worked by several duties (one per session), so this is a many-to-many via
                // a join table, in order. The part stays owned by its vehicle schedule (Schedule/ScheduleId).
                entity.HasMany(e => e.Parts)
                      .WithMany()
                      .UsingEntity<Dictionary<string, object>>(
                          ListPositions.Joins.DriverDutyTrainPart,
                          item => item.HasOne<ScheduledTrainPart>().WithMany().HasForeignKey("TrainPartId").OnDelete(DeleteBehavior.Restrict),
                          owner => owner.HasOne<DriverDuty>().WithMany().HasForeignKey("DriverDutyId").OnDelete(DeleteBehavior.Cascade),
                          j =>
                          {
                              j.HasKey("DriverDutyId", "TrainPartId");
                              j.Property<int>(ListPositions.Column);
                          });

                entity.HasMany(e => e.Notes)
                      .WithOne(e => e.DriverDuty)
                      .HasForeignKey(e => e.DriverDutyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
    }
}
