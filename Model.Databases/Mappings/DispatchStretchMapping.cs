using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class DispatchStretchMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A dispatch stretch and the track stretches it is made of, in order.</summary>
        internal void MapDispatchStretch() =>
            builder.Entity<DispatchStretch>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.From)
                      .WithMany()
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.To)
                      .WithMany()
                      .OnDelete(DeleteBehavior.Restrict);

                // IntermediateLocations is a computed view over Stretches, not stored data.
                entity.Ignore(e => e.IntermediateLocations);

                // The ordered track stretches the dispatch stretch comprises; the track stretches
                // themselves belong to the layout, so this is a reference-only many-to-many.
                entity.HasMany(e => e.Stretches)
                      .WithMany()
                      .UsingEntity<Dictionary<string, object>>(
                          ListPositions.Joins.DispatchStretchTrackStretch,
                          item => item.HasOne<TrackStretch>().WithMany().HasForeignKey("TrackStretchId").OnDelete(DeleteBehavior.Restrict),
                          owner => owner.HasOne<DispatchStretch>().WithMany().HasForeignKey("DispatchStretchId").OnDelete(DeleteBehavior.Cascade),
                          j =>
                          {
                              j.HasKey("DispatchStretchId", "TrackStretchId");
                              j.Property<int>(ListPositions.Column);
                          });
            });
    }
}
