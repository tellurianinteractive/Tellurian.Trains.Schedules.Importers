using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class RegionMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A region in the layout's catalogue.</summary>
        internal void MapRegion() =>
            builder.Entity<Region>(entity =>
            {
                // Region (the per-layout catalogue; a Station references a subset via a many-to-many)
                entity.Property<int>(ListPositions.Column);
                entity.Property(e => e.ExtensionData).HasJsonProperties();
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.BackgroundColor).HasMaxLength(20);
            });
    }
}
