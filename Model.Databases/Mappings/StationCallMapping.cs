using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class StationCallMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A train's call at a track.</summary>
        internal void MapStationCall() =>
            builder.Entity<StationCall>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property<int>(ListPositions.Column);
                entity.Property(e => e.Arrival).HasConversion(Conversions.Time);
                entity.Property(e => e.Departure).HasConversion(Conversions.Time);

                entity.HasMany(e => e.Notes)
                      .WithOne(e => e.StationCall)
                      .HasForeignKey(e => e.StationCallId)
                      .OnDelete(DeleteBehavior.Cascade);

                // The track's calls are an index the planner keeps, not a navigation of the shared track.
                entity.HasOne(e => e.Track)
                      .WithMany()
                      .HasForeignKey(e => e.TrackId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Ignore(e => e.OperationLocation);
                entity.Ignore(e => e.IsStop);
                entity.Ignore(e => e.SortTime);
                entity.Ignore(e => e.RunaroundLoco); // only there to read plans saved before Runaround
            });
    }
}
