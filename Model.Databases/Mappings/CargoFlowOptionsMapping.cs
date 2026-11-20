using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class CargoFlowOptionsMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A cargo flow description in the timetable's catalogue, with its origins and destinations.</summary>
        internal void MapCargoFlowOptions() =>
            builder.Entity<CargoFlowOptions>(entity =>
            {
                // CargoFlowOptions: a reusable cargo flow description in the timetable catalogue, referenced by
                // CargoFlowTrainParts. Owns its origin and destination collections (each referencing a cargo-
                // exchanging OperationLocation, which may be a Station or an IndustrialArea).
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OnlyWagonClasses).HasMaxLength(50);
                entity.Property(e => e.SpecificCargo).HasMaxLength(50);

                entity.OwnsMany(c => c.Origins, origin =>
                {
                    origin.ToTable("CargoFlowOrigins");
                    origin.Property<int>("Id").ValueGeneratedOnAdd();
                    origin.HasKey("Id");
                    origin.Property<int>(ListPositions.Column);
                    origin.Property<int?>("LocationId").UsePropertyAccessMode(PropertyAccessMode.Property);
                    origin.HasOne(x => x.Location).WithMany().HasForeignKey("LocationId").IsRequired().OnDelete(DeleteBehavior.Restrict);
                });

                entity.OwnsMany(c => c.Destinations, destination =>
                {
                    destination.ToTable("CargoFlowDestinations");
                    destination.Property<int>("Id").ValueGeneratedOnAdd();
                    destination.HasKey("Id");
                    destination.Property<int>(ListPositions.Column);
                    destination.Property<int?>("LocationId").UsePropertyAccessMode(PropertyAccessMode.Property);
                    destination.HasOne(x => x.Location).WithMany().HasForeignKey("LocationId").IsRequired().OnDelete(DeleteBehavior.Restrict);
                    // The renderings are extension properties, so EF never discovers them and there is
                    // nothing here to ignore.
                });
            });
    }
}
