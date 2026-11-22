using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class ScheduledUnitMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A unit in a vehicle's rake.</summary>
        internal void MapScheduledUnit() =>
            builder.Entity<ScheduledUnit>(entity =>
            {
                // ScheduledUnit (TPH: Wagon, TractionUnit). The record hierarchy carries no key of its own, so a
                // shadow key and shadow discriminator keep persistence concerns out of the domain type.
                entity.Property<int>("Id").ValueGeneratedOnAdd();
                entity.HasKey("Id");
                entity.Property<int>(ListPositions.Column);
                entity.HasDiscriminator<string>("UnitType")
                      .HasValue<Wagon>("Wagon")
                      .HasValue<TractionUnit>("TractionUnit");
            });
    }
}
