using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class TimetableStretchMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A timetable stretch and the track stretches it runs over, in order.</summary>
        internal void MapTimetableStretch() =>
            builder.Entity<TimetableStretch>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Number).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);

                // The ordered track stretches the timetable stretch runs over, which belong to the layout.
                entity.HasMany(e => e.Stretches)
                      .WithMany()
                      .UsingEntity<Dictionary<string, object>>(
                          ListPositions.Joins.TimetableStretchTrackStretch,
                          item => item.HasOne<TrackStretch>().WithMany().HasForeignKey("TrackStretchId").OnDelete(DeleteBehavior.Restrict),
                          owner => owner.HasOne<TimetableStretch>().WithMany().HasForeignKey("TimetableStretchId").OnDelete(DeleteBehavior.Cascade),
                          j =>
                          {
                              j.HasKey("TimetableStretchId", "TrackStretchId");
                              j.Property<int>(ListPositions.Column);
                          });

                entity.Ignore(e => e.Stations);
                // The stretches are the join table; StretchIds is how a file writes them.
                entity.Ignore(e => e.StretchIds);
            });
    }
}
