using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class CargoFlowTrainPartMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>What only a cargo flow has: the description it follows.</summary>
        internal void MapCargoFlowTrainPart() =>
            builder.Entity<CargoFlowTrainPart>(entity =>
            {
                // CargoFlowTrainPart: belongs to its Train (Train.CargoFlows, shadow TrainId FK) and references a
                // catalogue CargoFlowOptions. It is not part of a vehicle schedule or driver duty.
                entity.HasOne(e => e.CargoFlowOptions)
                      .WithMany()
                      .HasForeignKey(e => e.CargoFlowOptionsId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
    }
}
