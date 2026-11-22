using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class TimetableMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>The timetable: its trains and its catalogues of categories, cargo flows and sessions.</summary>
        internal void MapTimetable() =>
            builder.Entity<Timetable>(entity =>
            {
                entity.HasKey(e => e.Id);
                // One in a database (see DatabaseExportService), so its id is kept as the plan has it, often 0.
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

                entity.Property(e => e.Sessions)
                      .HasConversion(Conversions.SessionsCatalogue, Conversions.SessionsCatalogueComparer);

                entity.HasOne(e => e.Layout)
                      .WithMany()
                      .HasForeignKey(e => e.LayoutId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Trains)
                      .WithOne(e => e.Timetable)
                      .HasForeignKey(e => e.TimetableId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                // The category catalogue belongs to the timetable (1:N). TrainCategory has no Timetable
                // navigation, so the foreign key is a shadow property.
                entity.HasMany(e => e.TrainCategories)
                      .WithOne()
                      .HasForeignKey("TimetableId")
                      .OnDelete(DeleteBehavior.Cascade);

                // The cargo flow description catalogue belongs to the timetable (1:N). CargoFlowOptions is a
                // referenced entity (a CargoFlowTrainPart points at one), configured separately below; it has
                // no Timetable navigation, so the foreign key is a shadow property.
                entity.HasMany(e => e.CargoFlowOptions)
                      .WithOne()
                      .HasForeignKey("TimetableId")
                      .OnDelete(DeleteBehavior.Cascade);
            });
    }
}
