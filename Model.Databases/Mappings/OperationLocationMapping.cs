using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class OperationLocationMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>The operation locations, all kinds in one table with the kind in its own column.</summary>
        internal void MapOperationLocation() =>
            builder.Entity<OperationLocation>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Signature).HasMaxLength(10).IsRequired();
                entity.Property(e => e.CountryCode).HasMaxLength(2);

                entity.HasIndex(e => new { e.LayoutId, e.Signature }); // Not unique: the database holds any plan a plan file can; the model reports duplicates.

                entity.Property(e => e.ExtensionData).HasJsonProperties();
                entity.Property(e => e.ExternalReferences).HasJsonProperties();

                // The shunting yard whose local freight covers this location. Restrict rather than cascade:
                // deleting a shunting yard must not take the stations it served with it.
                // By its id, the CargoServedFromId the JSON writes too, read through the property (its getter gives the
                // station's id), never its backing field.
                entity.Property(e => e.CargoServedFromId).UsePropertyAccessMode(PropertyAccessMode.Property);
                entity.HasOne(e => e.CargoServedFrom)
                      .WithMany()
                      .HasForeignKey(e => e.CargoServedFromId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                // The manned station working this location from afar. Restrict, like CargoServedFrom.
                entity.Property(e => e.ControlledById).UsePropertyAccessMode(PropertyAccessMode.Property);
                entity.HasOne(e => e.ControlledBy)
                      .WithMany()
                      .HasForeignKey(e => e.ControlledById)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                entity.HasMany(e => e.Tracks)
                      .WithOne(e => e.Station)
                      .HasForeignKey(e => e.StationId)
                      .OnDelete(DeleteBehavior.Cascade);

                // The lock key, and the station it is held at, in the contract's own JSON (heldAtId), restored by
                // layout.RestoreReferences() after reading. Not an owned type: EF doesn't let a type owned by a location
                // refer to another location besides its owner.
                entity.Property(e => e.LockKey).HasContractJson();

                // Per-station timing overrides: structured data worth real columns (owned, not JSON).
                entity.OwnsOne(e => e.Timings, timings => timings.Property(x => x.ExtensionData).HasJsonProperties());

                // One table for the kinds of location; the column is the kind, with the values a plan file has in "kind".
                entity.HasDiscriminator<string>("Kind")
                      .HasValue<Station>(nameof(OperationLocationKind.Station))
                      .HasValue<SignalControlledLocation>(nameof(OperationLocationKind.SignalControlled))
                      .HasValue<IndustrialArea>(nameof(OperationLocationKind.IndustrialArea))
                      .HasValue<OtherLocation>(nameof(OperationLocationKind.Other));
            });
    }
}
