using Microsoft.EntityFrameworkCore;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

internal static class CallNoteMapping
{
    extension(ModelBuilder builder)
    {
        /// <summary>The notes of a call, all kinds in one table.</summary>
        internal void MapCallNote() =>
            builder.Entity<CallNote>(entity =>
            {
                // CallNote (TPH inheritance)
                entity.HasKey(e => e.Id);
                entity.Property<int>(ListPositions.Column);
                entity.HasDiscriminator<string>("NoteType")
                      .HasValue<TextCallNote>("Text");
            });
    }
}
