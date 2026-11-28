using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class TimetableMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>
        /// The timetable: its trains, its catalogues of categories, cargo flows and sessions, and what the planner decides
        /// for the layout: settings, companies, timetable and dispatch stretches, topology positions, and each location's
        /// and track's planning.
        /// </summary>
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

                // Timetable-wide settings are a planning-time configuration aggregate, never queried
                // relationally, so persist the whole graph in a single JSON column. The nested owned
                // types are declared explicitly so the JSON-owned StationTimings stays distinct from the
                // StationTimings of each location's planning, also JSON.
                entity.OwnsOne(e => e.Settings, settings =>
                {
                    settings.ToJson();
                    settings.OwnsOne(s => s.Identity);
                    settings.OwnsOne(s => s.General);
                    settings.OwnsOne(s => s.GraphicTimetable);
                    settings.OwnsOne(s => s.TimeAndSpeed, timeAndSpeed =>
                    {
                        timeAndSpeed.OwnsOne(t => t.Slow);
                        timeAndSpeed.OwnsOne(t => t.Normal);
                        timeAndSpeed.OwnsOne(t => t.High);
                        timeAndSpeed.OwnsOne(t => t.StationTimings, timings => timings.Property(x => x.ExtensionData).HasJsonProperties());
                    });
                    settings.OwnsOne(s => s.Validation);
                    settings.OwnsOne(s => s.Integration);
                });

                entity.HasMany(e => e.Companies)
                      .WithOne(e => e.Timetable)
                      .HasForeignKey(e => e.TimetableId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.TimetableStretches)
                      .WithOne(e => e.Timetable)
                      .HasForeignKey("TimetableId")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.DispatchStretches)
                      .WithOne()
                      .HasForeignKey("TimetableId")
                      .OnDelete(DeleteBehavior.Cascade);

                // The topology schematic's node positions: plain coordinates per location, kept as JSON.
                entity.OwnsMany(e => e.TopologyPositions, b => b.ToJson());

                // What the planner decides for each location and track, by id: read and written with the timetable and
                // never queried, so kept as JSON in the contract's own form.
                entity.OwnsMany(e => e.LocationPlanning, planning =>
                {
                    planning.ToJson();
                    planning.OwnsOne(p => p.Timings, timings => timings.Property(x => x.ExtensionData).HasJsonProperties());
                    planning.OwnsMany(p => p.StaffPosts, posts => posts.Property(x => x.ExtensionData).HasJsonProperties());
                });
                entity.OwnsMany(e => e.StationTrackRoutes, b => b.ToJson());

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
