using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class TrainMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>A train, its calls and its cargo flows.</summary>
        internal void MapTrain() =>
            builder.Entity<Train>(entity =>
            {
                entity.Property<int>(ListPositions.Column);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExternalId).HasMaxLength(50);
                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Calls)
                      .WithOne(e => e.Train)
                      .HasForeignKey(e => e.TrainId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Sessions).HasConversion(Conversions.Sessions);

                // The longest the train may be: axles, wagons and metres, each a column of its own.
                entity.ComplexProperty(e => e.Length);

                entity.Ignore(e => e.Tracks);
                entity.Ignore(e => e.Layout);
                entity.Ignore(e => e.AsTrainPart);

                // Cargo flows belong to the train (1:N). CargoFlowTrainPart has no Train navigation of its
                // own (Train is derived from its from-call), so the foreign key is a shadow property.
                entity.HasMany(e => e.CargoFlows)
                      .WithOne()
                      .HasForeignKey("TrainId")
                      .OnDelete(DeleteBehavior.Cascade);
            });
    }
}
