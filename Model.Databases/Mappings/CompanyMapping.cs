using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class CompanyMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A company in the layout's catalogue.</summary>
        internal void MapCompany() =>
            builder.Entity<Company>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Signature).HasMaxLength(20).IsRequired();
                entity.Property(e => e.CountryId);
                // A data URI, capped in the editor at roughly 64 KB encoded.
                entity.Property(e => e.Logo);
                entity.Property(e => e.OperatesFromYear);
                entity.Property(e => e.OperatesToYear);
                entity.Ignore(e => e.OperatingPeriodText);

                entity.HasIndex(e => new { e.TimetableId, e.Signature }); // Not unique: the database holds any plan a plan file can; the model reports duplicates.
            });
    }
}
