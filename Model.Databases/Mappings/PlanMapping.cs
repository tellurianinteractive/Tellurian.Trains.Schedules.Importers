using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class PlanMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>The plan, one in a database: its vehicles, schedules, duties and who brings which vehicle.</summary>
        internal void MapPlan() =>
            builder.Entity<Plan>(entity =>
            {
                entity.HasKey(e => e.Id);
                // One in a database (see DatabaseExportService), so its id is kept as the plan has it, often 0.
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                // Authored markdown of unbounded length; printed as its own booklet.
                entity.Property(e => e.GeneralInstructions);
                // Authored markdown of unbounded length; printed on that booklet's front page.
                entity.Property(e => e.Program);

                entity.HasOne(e => e.Timetable)
                      .WithMany()
                      .HasForeignKey(e => e.TimetableId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ScheduledObjects)
                      .WithOne(e => e.Plan)
                      .HasForeignKey(e => e.PlanId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Schedules)
                      .WithOne(e => e.Plan)
                      .HasForeignKey(e => e.PlanId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.DriverDuties)
                      .WithOne(e => e.Plan)
                      .HasForeignKey(e => e.PlanId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Who brings which rolling stock. Both are stored as JSON columns rather than tables, like the
                // layout's country catalogue: a participant and a vehicle are referred to by a plain int id, and
                // the order of a vehicle's contributors — the first is the primary one — is the order of the array.
                entity.OwnsMany(e => e.Participants, b => b.ToJson());
                entity.OwnsMany(e => e.OtherVehicles, b => b.ToJson());
                entity.OwnsMany(e => e.VehicleContributions, b =>
                {
                    b.ToJson();
                    b.OwnsMany(c => c.Contributors);
                });
            });
    }
}
