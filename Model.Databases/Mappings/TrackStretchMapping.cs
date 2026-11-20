using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class TrackStretchMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A track stretch between two operation locations.</summary>
        internal void MapTrackStretch() =>
            builder.Entity<TrackStretch>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Start)
                      .WithMany()
                      .HasForeignKey(e => e.StartId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.End)
                      .WithMany()
                      .HasForeignKey(e => e.EndId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.LayoutId, e.StartId, e.EndId }); // Not unique: the database holds any plan a plan file can; the model reports duplicates.

                entity.Property(e => e.ExtensionData).HasJsonProperties();
                // The hall plan's modules the stretch runs over, by id: a primitive collection.
                entity.PrimitiveCollection(e => e.ModuleIds);
            });
    }
}
