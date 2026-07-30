using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Tellurian.Trains.Schedules.Model.Databases;

/// <summary>
/// Entity Framework Core database context for persisting railway schedule data.
/// </summary>
/// <remarks>
/// This context provides access to all schedule-related entities organized into three layers:
/// <list type="bullet">
/// <item><description>Layout layer: Physical infrastructure (layouts, stations, tracks, stretches)</description></item>
/// <item><description>Timetable layer: Train scheduling (timetables, trains, station calls)</description></item>
/// <item><description>Schedule layer: Operational planning (schedules, vehicles, driver duties)</description></item>
/// </list>
/// </remarks>
/// <param name="options">The options to be used by this <see cref="DbContext"/>.</param>
public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    #region Layout Layer

    /// <summary>
    /// Gets the set of railway layouts in the database.
    /// </summary>
    public DbSet<Layout> Layouts => Set<Layout>();

    /// <summary>
    /// Gets the set of railway companies in the database.
    /// </summary>
    public DbSet<Company> Companies => Set<Company>();

    /// <summary>
    /// Gets the set of regions (the per-layout catalogue) in the database.
    /// </summary>
    public DbSet<Region> Regions => Set<Region>();

    /// <summary>
    /// Gets the set of operation locations (stations) in the database.
    /// </summary>
    public DbSet<OperationLocation> OperationLocations => Set<OperationLocation>();

    /// <summary>
    /// Gets the set of stations in the database.
    /// </summary>
    public DbSet<Station> Stations => Set<Station>();

    /// <summary>
    /// Gets the set of signal controlled locations in the database.
    /// </summary>
    public DbSet<SignalControlledLocation> SignalControlledLocations => Set<SignalControlledLocation>();

    /// <summary>
    /// Gets the set of other locations in the database.
    /// </summary>
    public DbSet<OtherLocation> OtherLocations => Set<OtherLocation>();

    /// <summary>
    /// Gets the set of dispatch stretches in the database.
    /// </summary>
    public DbSet<DispatchStretch> DispatchStretches => Set<DispatchStretch>();

    /// <summary>
    /// Gets the set of station tracks in the database.
    /// </summary>
    public DbSet<StationTrack> StationTracks => Set<StationTrack>();

    /// <summary>
    /// Gets the set of track stretches between stations in the database.
    /// </summary>
    public DbSet<TrackStretch> TrackStretches => Set<TrackStretch>();

    /// <summary>
    /// Gets the set of timetable stretches (logical groupings of track stretches) in the database.
    /// </summary>
    public DbSet<TimetableStretch> TimetableStretches => Set<TimetableStretch>();

    #endregion

    #region Timetable Layer

    /// <summary>
    /// Gets the set of timetables in the database.
    /// </summary>
    public DbSet<Timetable> Timetables => Set<Timetable>();

    /// <summary>
    /// Gets the set of train categories in the database.
    /// </summary>
    public DbSet<TrainCategory> TrainCategories => Set<TrainCategory>();

    /// <summary>
    /// Gets the set of trains in the database.
    /// </summary>
    public DbSet<Train> Trains => Set<Train>();

    /// <summary>
    /// Gets the set of station calls (scheduled stops) in the database.
    /// </summary>
    public DbSet<StationCall> StationCalls => Set<StationCall>();

    #endregion

    #region Schedule Layer

    /// <summary>
    /// Gets the set of schedules in the database.
    /// </summary>
    public DbSet<Plan> Plans => Set<Plan>();

    /// <summary>
    /// Gets the set of vehicles (locomotives and trainsets) in the database.
    /// </summary>
    public DbSet<ScheduledObject> ScheduledObjects => Set<ScheduledObject>();

    /// <summary>
    /// Gets the set of vehicle schedule assignments in the database.
    /// </summary>
    public DbSet<ScheduleAssignment> ScheduleAssignments => Set<ScheduleAssignment>();

    /// <summary>
    /// Gets the set of vehicle schedules in the database.
    /// </summary>
    public DbSet<Schedule> Schedules => Set<Schedule>();

    /// <summary>
    /// Gets the set of driver duties in the database.
    /// </summary>
    public DbSet<DriverDuty> DriverDuties => Set<DriverDuty>();

    /// <summary>
    /// Gets the set of train parts in the database. This is the root of the train-part hierarchy;
    /// use <c>OfType&lt;ScheduledTrainPart&gt;()</c> or <c>OfType&lt;CargoFlowTrainPart&gt;()</c> to
    /// query a single kind.
    /// </summary>
    public DbSet<TrainPart> TrainParts => Set<TrainPart>();

    #endregion

    #region Supporting Entities

    /// <summary>
    /// Gets the set of call notes in the database.
    /// </summary>
    public DbSet<CallNote> CallNotes => Set<CallNote>();

    /// <summary>
    /// Gets the set of text call notes in the database.
    /// </summary>
    public DbSet<TextCallNote> TextCallNotes => Set<TextCallNote>();

    /// <summary>
    /// Gets the set of driver duty notes in the database.
    /// </summary>
    public DbSet<DriverDutyNote> DriverDutyNotes => Set<DriverDutyNote>();

    #endregion

    // A Time is a value object wrapping a TimeSpan; it is persisted as ticks (long). Shared across
    // every Time property in the model, including the nullable duty overrides (EF Core applies a
    // non-nullable converter to Time? automatically).
    private static readonly ValueConverter<Time, long> TimeConverter = new(
        v => v.Value.Ticks,
        v => Time.FromTimeSpan(TimeSpan.FromTicks(v)));

    /// <summary>
    /// Configures the entity model for the database context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
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

            // The country catalogue saved with the layout. Stored as a JSON column rather than a
            // table: the entries are reference data referenced elsewhere only by their stable
            // Country.Id (a plain int), so no separate table or foreign keys are needed.
            entity.OwnsMany(e => e.Countries, b => b.ToJson());

            entity.HasMany(e => e.Companies)
                  .WithOne(e => e.Layout)
                  .HasForeignKey(e => e.LayoutId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.OperationLocations)
                  .WithOne(e => e.Layout)
                  .HasForeignKey(e => e.LayoutId)
                  .OnDelete(DeleteBehavior.Cascade);

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

            entity.HasMany(e => e.TimetableStretches)
                  .WithOne()
                  .OnDelete(DeleteBehavior.Cascade);

            // Layout-wide settings are a planning-time configuration aggregate, never queried
            // relationally, so persist the whole graph in a single JSON column. The nested owned
            // types are declared explicitly so the JSON-owned StationTimings stays distinct from the
            // column-owned OperationLocation.Timings of the same CLR type.
            entity.OwnsOne(e => e.Settings, settings =>
            {
                settings.ToJson();
                settings.OwnsOne(s => s.Identity);
                settings.OwnsOne(s => s.General);
                settings.OwnsOne(s => s.GraphicTimetable);
                settings.OwnsOne(s => s.TimeAndSpeed, timeAndSpeed =>
                {
                    timeAndSpeed.OwnsOne(t => t.Slow);
                    timeAndSpeed.OwnsOne(t => t.Normal);
                    timeAndSpeed.OwnsOne(t => t.High);
                    timeAndSpeed.OwnsOne(t => t.StationTimings);
                });
                settings.OwnsOne(s => s.Validation);
                settings.OwnsOne(s => s.Integration);
            });
        });

        // Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Signature).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CountryId);
            // A data URI, capped in the editor at roughly 64 KB encoded.
            entity.Property(e => e.Logo);

            entity.HasIndex(e => new { e.LayoutId, e.Signature }).IsUnique();
        });

        // Region (the per-layout catalogue; a Station references a subset via a many-to-many)
        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.BackgroundColor).HasMaxLength(20);
        });

        // OperationLocation (Station) - TPH inheritance
        modelBuilder.Entity<OperationLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Signature).HasMaxLength(10).IsRequired();
            entity.Property(e => e.CountryId);

            entity.HasIndex(e => new { e.LayoutId, e.Signature }).IsUnique();

            // The shunting yard whose local freight covers this location. Restrict rather than cascade:
            // deleting a shunting yard must not take the stations it served with it.
            entity.HasOne(e => e.CargoServedFrom)
                  .WithMany()
                  .HasForeignKey("CargoServedFromStationId")
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);

            entity.HasMany(e => e.Tracks)
                  .WithOne(e => e.Station)
                  .HasForeignKey(e => e.StationId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Per-station timing overrides: structured data worth real columns (owned, not JSON).
            entity.OwnsOne(e => e.Timings);

            // TPH discriminator for OperationLocation hierarchy
            entity.HasDiscriminator<string>("LocationType")
                  .HasValue<Station>("Station")
                  .HasValue<SignalControlledLocation>("SignalControlled")
                  .HasValue<OtherLocation>("Other");
        });

        // Register derived types
        modelBuilder.Entity<Station>(entity =>
        {
            // A station is associated with zero, one, or many regions from the layout's catalogue.
            // EF Core creates the StationRegion join table for this many-to-many.
            entity.HasMany(e => e.Regions)
                  .WithMany();
        });
        modelBuilder.Entity<SignalControlledLocation>(entity =>
        {
            entity.HasOne(e => e.ControlledBy)
                  .WithMany()
                  .HasForeignKey("ControlledByStationId")
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);
        });
        modelBuilder.Entity<OtherLocation>();

        // DispatchStretch
        modelBuilder.Entity<DispatchStretch>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.From)
                  .WithMany()
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.To)
                  .WithMany()
                  .OnDelete(DeleteBehavior.Restrict);

            // IntermediateLocations is a computed view over Stretches, not stored data.
            entity.Ignore(e => e.IntermediateLocations);

            // The ordered track stretches the dispatch stretch comprises; the track stretches
            // themselves belong to the layout, so this is a reference-only many-to-many.
            entity.HasMany(e => e.Stretches).WithMany();
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

            entity.Ignore(e => e.Stations);
        });
    }

    private static void ConfigureTimetable(ModelBuilder modelBuilder)
    {
        // Value converters
        var sessionsConverter = new ValueConverter<Sessions, int>(
            v => v.Flags,
            v => new Sessions(v));

        // The timetable's session catalogue is a small list of value objects (each a bit pattern),
        // referenced by value only, so it is persisted as a single comma-separated column of flags
        // rather than a table. A value comparer is required because the property is a mutable collection.
        var sessionsCatalogueConverter = new ValueConverter<IList<Sessions>, string>(
            v => string.Join(',', v.Select(s => (int)s.Flags)),
            v => ParseSessionsCatalogue(v));

        var sessionsCatalogueComparer = new ValueComparer<IList<Sessions>>(
            (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
            v => v.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.Flags)),
            v => v.ToList());

        // Timetable
        modelBuilder.Entity<Timetable>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

            entity.Property(e => e.Sessions)
                  .HasConversion(sessionsCatalogueConverter, sessionsCatalogueComparer);

            entity.HasOne(e => e.Layout)
                  .WithMany()
                  .HasForeignKey(e => e.LayoutId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Trains)
                  .WithOne(e => e.Timetable)
                  .HasForeignKey(e => e.TimetableId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            // The category catalogue belongs to the timetable (1:N). TrainCategory has no Timetable
            // navigation, so the foreign key is a shadow property.
            entity.HasMany(e => e.TrainCategories)
                  .WithOne()
                  .HasForeignKey("TimetableId")
                  .OnDelete(DeleteBehavior.Cascade);

            // The cargo flow description catalogue belongs to the timetable (1:N). CargoFlowOptions is a
            // referenced entity (a CargoFlowTrainPart points at one), configured separately below; it has
            // no Timetable navigation, so the foreign key is a shadow property.
            entity.HasMany(e => e.CargoFlowOptions)
                  .WithOne()
                  .HasForeignKey("TimetableId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TrainCategory
        modelBuilder.Entity<TrainCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Prefix).HasMaxLength(10);
            entity.Property(e => e.Suffix).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
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

            // Cargo flows belong to the train (1:N). CargoFlowTrainPart has no Train navigation of its
            // own (Train is derived from its from-call), so the foreign key is a shadow property.
            entity.HasMany(e => e.CargoFlows)
                  .WithOne()
                  .HasForeignKey("TrainId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // StationCall
        modelBuilder.Entity<StationCall>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Arrival).HasConversion(TimeConverter);
            entity.Property(e => e.Departure).HasConversion(TimeConverter);

            entity.HasMany(e => e.Notes)
                  .WithOne(e => e.StationCall)
                  .HasForeignKey(e => e.StationCallId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(e => e.OperationLocation);
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
        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.Id);
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
        });

        // Vehicle
        modelBuilder.Entity<ScheduledObject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Class).HasMaxLength(50);
            entity.Property(e => e.Remark).HasMaxLength(500);
            entity.Property(e => e.ExternalId).HasMaxLength(100);

            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.ScheduleAssignments)
                  .WithOne(e => e.ScheduledObject)
                  .HasForeignKey(e => e.ScheduledObjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            // The ordered rake of scheduled units. ScheduledUnit is a polymorphic hierarchy (Wagon,
            // TractionUnit), which owned types cannot express, so it is a regular TPH entity related to
            // the scheduled object through a shadow foreign key.
            entity.HasMany(e => e.Units)
                  .WithOne()
                  .HasForeignKey("ScheduledObjectId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ScheduledUnit (TPH: Wagon, TractionUnit). The record hierarchy carries no key of its own, so a
        // shadow key and shadow discriminator keep persistence concerns out of the domain type.
        modelBuilder.Entity<ScheduledUnit>(entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd();
            entity.HasKey("Id");
            entity.HasDiscriminator<string>("UnitType")
                  .HasValue<Wagon>("Wagon")
                  .HasValue<TractionUnit>("TractionUnit");
        });

        // ScheduleAssignment
        modelBuilder.Entity<ScheduleAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Sessions).HasConversion(sessionsConverter);

            entity.HasOne(e => e.Schedule)
                  .WithMany()
                  .HasForeignKey(e => e.ScheduleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // VehicleSchedule
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);

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

            // Effective start/end derive from the parts; only the optional manual overrides are stored.
            entity.Property(e => e.OverriddenStartTime).HasConversion(TimeConverter);
            entity.Property(e => e.OverriddenEndTime).HasConversion(TimeConverter);

            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            // A train part may be worked by several duties (one per session), so this is a many-to-many via
            // an implicit join table. The part stays owned by its vehicle schedule (Schedule/ScheduleId).
            entity.HasMany(e => e.Parts)
                  .WithMany();

            entity.HasMany(e => e.Notes)
                  .WithOne(e => e.DriverDuty)
                  .HasForeignKey(e => e.DriverDutyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TrainPart: the abstract base carries the from/to geometry shared by both kinds and is the root
        // of the table-per-hierarchy mapping. The kinds are siblings deriving from it, so the key, the
        // station-call relationships and the discriminator must all be configured here — configuring them
        // on ScheduledTrainPart would make that sealed leaf the root and leave CargoFlowTrainPart outside
        // the hierarchy.
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

            // Table-per-hierarchy over the train-part kinds.
            entity.HasDiscriminator<string>("PartType")
                  .HasValue<ScheduledTrainPart>("Scheduled")
                  .HasValue<CargoFlowTrainPart>("CargoFlow");
        });

        // ScheduledTrainPart per-part options: four optional, independent owned types, each mapped to
        // their own table sharing the train part primary key (an absent row means the option is null).
        modelBuilder.Entity<ScheduledTrainPart>(entity =>
        {
            entity.OwnsOne(e => e.TractionOptions, o => o.ToTable("TractionOptions"));

            entity.OwnsOne(e => e.WagonSetOptions, o => o.ToTable("NonTractionOptions"));

            entity.OwnsOne(e => e.CargoOnlyOptions, o =>
            {
                o.ToTable("CargoOnlyOptions");
                // Load/Unload are computed from the base couple/uncouple flags.
                o.Ignore(c => c.Load);
                o.Ignore(c => c.Unload);
            });
        });

        // CargoFlowOptions: a reusable cargo flow description in the timetable catalogue, referenced by
        // CargoFlowTrainParts. Owns its origin and destination collections (each referencing a Station).
        modelBuilder.Entity<CargoFlowOptions>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OnlyWagonClasses).HasMaxLength(50);

            entity.OwnsMany(c => c.Origins, origin =>
            {
                origin.ToTable("CargoFlowOrigins");
                origin.Property<int>("Id").ValueGeneratedOnAdd();
                origin.HasKey("Id");
                origin.HasOne(x => x.Station).WithMany().OnDelete(DeleteBehavior.Restrict);
            });

            entity.OwnsMany(c => c.Destinations, destination =>
            {
                destination.ToTable("CargoFlowDestinations");
                destination.Property<int>("Id").ValueGeneratedOnAdd();
                destination.HasKey("Id");
                destination.HasOne(x => x.Station).WithMany().OnDelete(DeleteBehavior.Restrict);
                // Computed markup rendering, not persisted.
                destination.Ignore(x => x.ToHtml);
            });
        });

        // CargoFlowTrainPart: belongs to its Train (Train.CargoFlows, shadow TrainId FK) and references a
        // catalogue CargoFlowOptions. It is not part of a vehicle schedule or driver duty.
        modelBuilder.Entity<CargoFlowTrainPart>(entity =>
        {
            entity.HasOne(e => e.CargoFlowOptions)
                  .WithMany()
                  .HasForeignKey(e => e.CargoFlowOptionsId)
                  .OnDelete(DeleteBehavior.Restrict);
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
        });

        // DriverDutyNote
        modelBuilder.Entity<DriverDutyNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).HasMaxLength(1000);
        });
    }

    private static IList<Sessions> ParseSessionsCatalogue(string value) =>
        string.IsNullOrEmpty(value)
            ? []
            : [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => new Sessions(int.Parse(s)))];

    private static TrainCapacity ParseTrainLength(string value)
    {
        var parts = value.Split('|');
        return new TrainCapacity
        {
            Axles = int.TryParse(parts[0], out var axles) ? axles : null,
            Meters = parts.Length > 1 && int.TryParse(parts[1], out var meters) ? meters : null
        };
    }
}
