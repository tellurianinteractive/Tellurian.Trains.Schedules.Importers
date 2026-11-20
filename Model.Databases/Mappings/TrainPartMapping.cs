using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class TrainPartMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>The train parts, both kinds in one table.</summary>
        internal void MapTrainPart() =>
            builder.Entity<TrainPart>(entity =>
            {
                // TrainPart: the abstract base carries the from/to geometry shared by both kinds and is the root
                // of the table-per-hierarchy mapping. The kinds are siblings deriving from it, so the key, the
                // station-call relationships and the discriminator must all be configured here — configuring them
                // on ScheduledTrainPart would make that sealed leaf the root and leave CargoFlowTrainPart outside
                // the hierarchy.
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExternalKey).HasMaxLength(100);
                // A scheduled part's position in its schedule, a cargo flow's among its train's.
                entity.Property<int>(ListPositions.Column);

                entity.HasOne(e => e.From)
                      .WithMany()
                      .HasForeignKey(e => e.FromId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.To)
                      .WithMany()
                      .HasForeignKey(e => e.ToId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Ignore(e => e.Train);
                entity.Ignore(e => e.Departure);
                entity.Ignore(e => e.Arrival);

                // Table-per-hierarchy over the train-part kinds.
                entity.HasDiscriminator<string>("PartType")
                      .HasValue<ScheduledTrainPart>("Scheduled")
                      .HasValue<CargoFlowTrainPart>("CargoFlow");
            });
    }
}
