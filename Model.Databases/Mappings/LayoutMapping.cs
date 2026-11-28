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

                entity.HasMany(e => e.OperationLocations)
                      .WithOne(e => e.Layout)
                      .HasForeignKey(e => e.LayoutId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ExtensionData).HasJsonProperties();

                // What the hall plan the layout was made from gave it: the modules without their drawings, the order of
                // the locations along each line, and the scale. Read and written as a whole and never queried, so kept as
                // JSON in the contract's own form (Layout.JsonSerializerOptions).
                entity.Property(e => e.Modules).HasContractJson();
                entity.Property(e => e.StretchLines).HasContractJson();
                entity.Property(e => e.Scale).HasContractJson();
                entity.Property(e => e.StandardName).HasMaxLength(100);

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
            });
    }
}
