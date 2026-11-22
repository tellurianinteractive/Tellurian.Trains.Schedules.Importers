using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class LayoutMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>The layout: its catalogues, settings and what the hall plan it was made from gave it.</summary>
        internal void MapLayout() =>
            builder.Entity<Layout>(entity =>
            {
                entity.HasKey(e => e.Id);
                // One in a database (see DatabaseExportService), so its id is kept as the plan has it, often 0.
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).HasMaxLength(200);

                // The country catalogue saved with the layout. Stored as a JSON column rather than a
                // table: the entries are reference data referenced elsewhere only by their stable
                // Country.Id (a plain int), so no separate table or foreign keys are needed.
                entity.OwnsMany(e => e.Countries, b => b.ToJson());

                // The topology schematic's node positions: plain coordinates per location, kept as JSON too.
                entity.OwnsMany(e => e.TopologyPositions, b => b.ToJson());

                entity.HasMany(e => e.Companies)
                      .WithOne(e => e.Layout)
                      .HasForeignKey(e => e.LayoutId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.OperationLocations)
                      .WithOne(e => e.Layout)
                      .HasForeignKey(e => e.LayoutId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ExtensionData).HasJsonProperties();

                // The hall plan's modules and its standard: what the layout was made from, read and written as a whole
                // and never queried, so kept as JSON in the contract's own form (Layout.JsonSerializerOptions). A module
                // refers to its location by id there; ReadPlanAsync restores it with layout.RestoreReferences().
                entity.Property(e => e.Modules).HasContractJson();
                entity.Property(e => e.PreferredStandard).HasContractJson();

                // The region catalogue belongs to the layout (1:N). Region has no Layout navigation,
                // so the foreign key is a shadow property.
                entity.HasMany(e => e.Regions)
                      .WithOne()
                      .HasForeignKey("LayoutId")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.TrackStretches)
                      .WithOne(e => e.Layout)
                      .HasForeignKey(e => e.LayoutId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.TimetableStretches)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);

                // Layout-wide settings are a planning-time configuration aggregate, never queried
                // relationally, so persist the whole graph in a single JSON column. The nested owned
                // types are declared explicitly so the JSON-owned StationTimings stays distinct from the
                // column-owned OperationLocation.Timings of the same CLR type.
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
            });
    }
}
