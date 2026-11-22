using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class StationTrackMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A track of an operation location.</summary>
        internal void MapStationTrack() =>
            builder.Entity<StationTrack>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Designation).HasMaxLength(10).IsRequired();
                entity.Property<int>(ListPositions.Column);
                entity.Property(e => e.Usage).HasMaxLength(100);
                entity.Property(e => e.ExtensionData).HasJsonProperties();

                entity.HasIndex(e => new { e.StationId, e.Designation }); // Not unique: the database holds any plan a plan file can; the model reports duplicates.
            });
    }
}
