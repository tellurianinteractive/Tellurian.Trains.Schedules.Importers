using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class StationMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>What only a station has: its regions and its staffing.</summary>
        internal void MapStation() =>
            builder.Entity<Station>(entity =>
            {
                // A station is associated with zero, one, or many regions from the layout's catalogue, in order.
                entity.HasMany(e => e.Regions)
                      .WithMany()
                      .UsingEntity<Dictionary<string, object>>(
                          ListPositions.Joins.StationRegion,
                          item => item.HasOne<Region>().WithMany().HasForeignKey("RegionId").OnDelete(DeleteBehavior.Cascade),
                          owner => owner.HasOne<Station>().WithMany().HasForeignKey("StationId").OnDelete(DeleteBehavior.Cascade),
                          j =>
                          {
                              j.HasKey("StationId", "RegionId");
                              j.Property<int>(ListPositions.Column);
                          });

                // RegionIds is how that association is written to a plan (see PlanJson); the join table
                // holds it here, so mapping it as well would store it twice.
                entity.Ignore(e => e.RegionIds);
                entity.Ignore(e => e.WrittenRegionIds);

                // The staffing: a short list of posts, each a set of roles, read and written with the station.
                entity.OwnsMany(e => e.StaffPosts, posts =>
                {
                    posts.ToJson();
                    posts.Property(p => p.ExtensionData).HasJsonProperties();
                });
            });
    }
}
