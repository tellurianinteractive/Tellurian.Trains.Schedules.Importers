using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class TrainCategoryMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A train category in the timetable's catalogue.</summary>
        internal void MapTrainCategory() =>
            builder.Entity<TrainCategory>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Prefix).HasMaxLength(10);
                entity.Property(e => e.Suffix).HasMaxLength(10);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Color).HasMaxLength(20);
            });
    }
}
