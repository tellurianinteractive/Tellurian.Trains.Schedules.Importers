using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class ScheduledTrainPartMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>What only a scheduled train part has: its options and the other tracks of its vehicles.</summary>
        internal void MapScheduledTrainPart() =>
            builder.Entity<ScheduledTrainPart>(entity =>
            {
                // ScheduledTrainPart per-part options: four optional, independent owned types, each mapped to
                // their own table sharing the train part primary key (an absent row means the option is null).
                entity.OwnsOne(e => e.TractionOptions, o =>
                {
                    o.ToTable("TractionOptions");
                    o.Ignore(t => t.RunaroundLoco); // only there to read plans saved before Runaround
                });

                entity.OwnsOne(e => e.WagonSetOptions, o => o.ToTable("NonTractionOptions"));

                entity.OwnsOne(e => e.CargoOnlyOptions, o =>
                {
                    o.ToTable("CargoOnlyOptions");
                    // Load/Unload are computed from the base couple/uncouple flags.
                    o.Ignore(c => c.Load);
                    o.Ignore(c => c.Unload);
                });

                // The other tracks the vehicles are fetched from and put on: optional references to tracks the
                // layout owns, through shadow foreign keys. A track in use is not deleted (DeletionRules).
                // The internal FromTrackId and ToTrackId the JSON writes too, through the properties, whose getters give the
                // track's id, never through their backing fields.
                entity.Property<int?>("FromTrackId").UsePropertyAccessMode(PropertyAccessMode.Property);
                entity.Property<int?>("ToTrackId").UsePropertyAccessMode(PropertyAccessMode.Property);
                entity.HasOne(e => e.FromTrack)
                      .WithMany()
                      .HasForeignKey("FromTrackId")
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ToTrack)
                      .WithMany()
                      .HasForeignKey("ToTrackId")
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });
    }
}
