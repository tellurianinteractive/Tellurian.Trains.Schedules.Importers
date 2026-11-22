using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class DriverDutyNoteMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A note of a driver duty.</summary>
        internal void MapDriverDutyNote() =>
            builder.Entity<DriverDutyNote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).HasMaxLength(1000);
                entity.Property<int>(ListPositions.Column);
            });
    }
}
