using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Tellurian.Trains.Schedules.Model.Databases;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{

    // Layout layer
    public DbSet<Layout> Layouts => Set<Layout>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<OperationLocation> OperationLocations => Set<OperationLocation>();
    public DbSet<StationTrack> StationTracks => Set<StationTrack>();
    public DbSet<TrackStretch> TrackStretches => Set<TrackStretch>();
    public DbSet<TimetableStretch> TimetableStretches => Set<TimetableStretch>();

    // Timetable layer
    public DbSet<Timetable> Timetables => Set<Timetable>();
    public DbSet<TrainCategory> TrainCategories => Set<TrainCategory>();
    public DbSet<Train> Trains => Set<Train>();
    public DbSet<StationCall> StationCalls => Set<StationCall>();

    // Schedule layer
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<VehicleSchedule> VehicleSchedules => Set<VehicleSchedule>();
    public DbSet<LocoSchedule> LocoSchedules => Set<LocoSchedule>();
    public DbSet<TrainsetSchedule> TrainsetSchedules => Set<TrainsetSchedule>();
    public DbSet<DriverDuty> DriverDuties => Set<DriverDuty>();
    public DbSet<TrainPart> TrainParts => Set<TrainPart>();

    // Supporting entities
    public DbSet<CallNote> CallNotes => Set<CallNote>();
    public DbSet<TextCallNote> TextCallNotes => Set<TextCallNote>();
    public DbSet<DriverDutyNote> DriverDutyNotes => Set<DriverDutyNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureLayout(modelBuilder);
        ConfigureTimetable(modelBuilder);
        ConfigureSchedule(modelBuilder);
        ConfigureSupportingEntities(modelBuilder);
    }

    private static void ConfigureLayout(ModelBuilder modelBuilder)
    {
        // Layout
        modelBuilder.Entity<Layout>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasMany(e => e.Companies)
                  .WithOne(e => e.Layout)
                  .HasForeignKey(e => e.LayoutId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Stations)
                  .WithOne(e => e.Layout)
                  .HasForeignKey(e => e.LayoutId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TrackStretches)
                  .WithOne(e => e.Layout)
                  .HasForeignKey(e => e.LayoutId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.TimetableStretches)
                  .WithOne()
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Signature).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CountryCode).HasMaxLength(5);

            entity.HasIndex(e => new { e.LayoutId, e.Signature }).IsUnique();
        });

        // OperationLocation (Station)
        modelBuilder.Entity<OperationLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Signature).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasIndex(e => new { e.LayoutId, e.Signature }).IsUnique();

            entity.HasMany(e => e.Tracks)
                  .WithOne(e => e.Station)
                  .HasForeignKey(e => e.StationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // StationTrack
        modelBuilder.Entity<StationTrack>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Usage).HasMaxLength(100);

            entity.HasIndex(e => new { e.StationId, e.Number }).IsUnique();

            entity.HasMany(e => e.Calls)
                  .WithOne(e => e.Track)
                  .HasForeignKey(e => e.TrackId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TrackStretch
        modelBuilder.Entity<TrackStretch>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Start)
                  .WithMany()
                  .HasForeignKey(e => e.StartId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.End)
                  .WithMany()
                  .HasForeignKey(e => e.EndId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.LayoutId, e.StartId, e.EndId }).IsUnique();

            entity.Ignore(e => e.Passings);
        });

        // TimetableStretch
        modelBuilder.Entity<TimetableStretch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);

            entity.HasMany(e => e.Stretches)
                  .WithMany();

            entity.Ignore(e => e.Starts);
            entity.Ignore(e => e.Ends);
            entity.Ignore(e => e.Stations);
        });
    }

    private static void ConfigureTimetable(ModelBuilder modelBuilder)
    {
        // Value converters
        var timeConverter = new ValueConverter<Time, long>(
            v => v.Value.Ticks,
            v => Time.FromTimeSpan(TimeSpan.FromTicks(v)));

        var sessionsConverter = new ValueConverter<Sessions, int>(
            v => v.Flags,
            v => new Sessions(v));

        // Timetable
        modelBuilder.Entity<Timetable>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

            entity.HasOne(e => e.Layout)
                  .WithMany()
                  .HasForeignKey(e => e.LayoutId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Trains)
                  .WithOne(e => e.Timetable)
                  .HasForeignKey(e => e.TimetableId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TrainCategory
        modelBuilder.Entity<TrainCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Prefix).HasMaxLength(10);
            entity.Property(e => e.Suffix).HasMaxLength(10);
            entity.Property(e => e.ResourceName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(20);
        });

        // Train
        modelBuilder.Entity<Train>(entity =>
        {
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

            entity.Property(e => e.Sessions).HasConversion(sessionsConverter);

            // Length - store as pipe-delimited string
            entity.Property(e => e.Length)
                  .HasConversion(
                      v => $"{v.Axles}|{v.Meters}",
                      v => ParseTrainLength(v));

            entity.Ignore(e => e.Tracks);
            entity.Ignore(e => e.Layout);
            entity.Ignore(e => e.AsTrainPart);
        });

        // StationCall
        modelBuilder.Entity<StationCall>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Arrival).HasConversion(timeConverter);
            entity.Property(e => e.Departure).HasConversion(timeConverter);

            entity.HasMany(e => e.Notes)
                  .WithOne(e => e.StationCall)
                  .HasForeignKey(e => e.StationCallId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(e => e.Station);
            entity.Ignore(e => e.IsStop);
            entity.Ignore(e => e.SortTime);
        });
    }

    private static void ConfigureSchedule(ModelBuilder modelBuilder)
    {
        var sessionsConverter = new ValueConverter<Sessions, int>(
            v => v.Flags,
            v => new Sessions(v));

        // Schedule
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

            entity.HasOne(e => e.Timetable)
                  .WithMany()
                  .HasForeignKey(e => e.TimetableId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.LocoSchedules)
                  .WithOne(e => e.Schedule)
                  .HasForeignKey(e => e.ScheduleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TrainsetSchedules)
                  .WithOne(e => e.Schedule)
                  .HasForeignKey(e => e.ScheduleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.DriverDuties)
                  .WithOne(e => e.Schedule)
                  .HasForeignKey(e => e.ScheduleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // VehicleSchedule (TPH inheritance)
        modelBuilder.Entity<VehicleSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Class).HasMaxLength(50);
            entity.Property(e => e.Remark).HasMaxLength(500);

            entity.HasDiscriminator<string>("ScheduleType")
                  .HasValue<LocoSchedule>("Loco")
                  .HasValue<TrainsetSchedule>("Trainset");

            entity.Property(e => e.Sessions).HasConversion(sessionsConverter);

            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Parts)
                  .WithOne(e => e.Schedule)
                  .HasForeignKey(e => e.ScheduleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // DriverDuty
        modelBuilder.Entity<DriverDuty>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Identity).HasMaxLength(50).IsRequired();

            entity.Property(e => e.Sessions).HasConversion(sessionsConverter);

            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Parts)
                  .WithOne(e => e.Duty)
                  .HasForeignKey(e => e.DutyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Notes)
                  .WithOne(e => e.DriverDuty)
                  .HasForeignKey(e => e.DriverDutyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TrainPart
        modelBuilder.Entity<TrainPart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalKey).HasMaxLength(100);

            entity.HasOne(e => e.From)
                  .WithMany()
                  .HasForeignKey(e => e.FromId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.To)
                  .WithMany()
                  .HasForeignKey(e => e.ToId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.Train);
            entity.Ignore(e => e.Departure);
            entity.Ignore(e => e.Arrival);
        });
    }

    private static void ConfigureSupportingEntities(ModelBuilder modelBuilder)
    {
        // CallNote (TPH inheritance)
        modelBuilder.Entity<CallNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasDiscriminator<string>("NoteType")
                  .HasValue<TextCallNote>("Text");
        });

        // TextCallNote
        modelBuilder.Entity<TextCallNote>(entity =>
        {
            entity.Property(e => e.Text).HasMaxLength(1000);
            entity.Property(e => e.LanguageCode).HasMaxLength(5);
        });

        // DriverDutyNote
        modelBuilder.Entity<DriverDutyNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).HasMaxLength(1000);
        });
    }

    private static TrainLenght ParseTrainLength(string value)
    {
        var parts = value.Split('|');
        return new TrainLenght
        {
            Axles = int.TryParse(parts[0], out var axles) ? axles : null,
            Meters = parts.Length > 1 && int.TryParse(parts[1], out var meters) ? meters : null
        };
    }
}
