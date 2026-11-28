namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>
/// Exactly what a module drawing shares with the module's row in the Module Registry (<c>ModelSurfaceAnalysis.md</c>
/// 4.2 and 4.3): the one statement of which properties are shared, used by both apps. The Registry maps its entities to
/// it; Module Draw compares with it.
/// </summary>
/// <remarks>
/// <para>
/// Each property says which way it goes. <b>Both ways</b>: what either app may change. <b>From the Registry</b>: what the
/// Registry owns, such as ownership and registration, which the drawing only shows. <b>From the drawing</b>: what the
/// drawing is the source of, measured or worked out from it, and the features the user enters in Module Draw until the
/// drawing can tell them.
/// </para>
/// <para>
/// <see cref="From"/> reads and derives everything from a module file. <c>content.WithRegistryData(data)</c> writes
/// back only what goes both ways or from the Registry; the drawing's geometry is never changed.
/// </para>
/// </remarks>
public sealed record ModuleRegistryData
{
    /// <summary>The module's name. Both ways.</summary>
    public required string Name { get; init; }

    /// <summary>The configuration label. Both ways.</summary>
    public string? ConfigurationLabel { get; init; }

    /// <summary>The label of the package the module belongs to. Both ways.</summary>
    public string? PackageLabel { get; init; }

    /// <summary>A note for the reader. Both ways.</summary>
    public string? Note { get; init; }

    /// <summary>The first year the module represents. Both ways.</summary>
    public int? RepresentsFromYear { get; init; }

    /// <summary>The last year the module represents. Both ways.</summary>
    public int? RepresentsUptoYear { get; init; }

    /// <summary>The catalogue id of the module's standard. Both ways, by id.</summary>
    public Guid? StandardId { get; init; }

    /// <summary>The short name of the module's standard, for writing it back by name when the id changes.</summary>
    public string? StandardName { get; init; }

    /// <summary>The module's speed limit, km/h of the prototype; the planner's stretch speed comes from it. Both ways.</summary>
    public int? SpeedLimit { get; init; }

    /// <summary>The owner's prefix. From the Registry.</summary>
    public string? Prefix { get; init; }

    /// <summary>The module's number. From the Registry.</summary>
    public int? Number { get; init; }

    /// <summary>Whether the number is a FREMO designation. From the Registry.</summary>
    public bool HasFremoDesignation { get; init; }

    /// <summary>Who owns the module, as written. From the Registry.</summary>
    public string? ModuleOwners { get; init; }

    /// <summary>The module turns an asymmetric end profile over. From the Registry.</summary>
    public bool IsEndProfileSwitcher { get; init; }

    /// <summary>The module can be shown on its own. From the Registry.</summary>
    public bool IsStandAlone { get; init; }

    /// <summary>The Registry's code for how far the module works. From the Registry.</summary>
    public int? FunctionalState { get; init; }

    /// <summary>The Registry's code for how far the landscape is finished. From the Registry.</summary>
    public int? LandscapeState { get; init; }

    /// <summary>The number of through tracks, as entered in the drawing. From the drawing.</summary>
    public int? NumberOfThroughTracks { get; init; }

    /// <summary>The gauges of the module's track, mm, from its exits' end profiles. From the drawing.</summary>
    public IReadOnlyList<double> Gauges { get; init; } = [];

    /// <summary>
    /// The rail systems of the module's track, from its exits' end profiles: the Registry's <c>Is2R</c> and <c>Is3R</c>.
    /// From the drawing.
    /// </summary>
    public IReadOnlyList<RailSystem> RailSystems { get; init; } = [];

    /// <summary>The operator passes under the module: the drawing has duck-under areas. From the drawing.</summary>
    public bool IsDuckunder { get; init; }

    /// <summary>The line ends on the module: the drawing has one exit. From the drawing.</summary>
    public bool IsEndOfLine { get; init; }

    /// <summary>The module is a turntable, as entered in Module Draw. From the drawing.</summary>
    public bool IsTurntable { get; init; }

    /// <summary>The module has a signal, as entered in Module Draw. From the drawing.</summary>
    public bool IsSignalModule { get; init; }

    /// <summary>The module's measures: radius, angle, straight, length, width, sections. From the drawing.</summary>
    public ModuleMeasures? Measures { get; init; }

    /// <summary>The module's exits. From the drawing.</summary>
    public IReadOnlyList<ModuleRegistryExit> Exits { get; init; } = [];

    /// <summary>The operation location the module is, if any.</summary>
    public ModuleRegistryLocation? Location { get; init; }

    /// <summary>
    /// What <paramref name="content"/> shares with the Registry, read and derived from it: the measures and features
    /// from its drawing, the gauges and rail systems from its exits' end profiles, and the lengths of the location's
    /// tracks as the drawing gives them, with the fouling distance of <paramref name="standard"/> when given.
    /// <paramref name="catalogue"/> finds end profiles the file doesn't embed.
    /// </summary>
    public static ModuleRegistryData From(ModuleFileContent content, ModuleStandard? standard = null, Func<Guid, EndProfile?>? catalogue = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        EndProfile? Lookup(Guid id) => content.FindEndProfile(id) ?? catalogue?.Invoke(id);
        var module = content.WithMeasuredTracks(standard, catalogue).Module;
        var drawing = module.Drawing;
        return new()
        {
            Name = module.Name,
            ConfigurationLabel = module.ConfigurationLabel,
            PackageLabel = module.PackageLabel,
            Note = module.Note,
            RepresentsFromYear = module.RepresentsFromYear,
            RepresentsUptoYear = module.RepresentsUptoYear,
            StandardId = module.Standard?.Id,
            StandardName = module.Standard?.ShortName,
            SpeedLimit = module.SpeedLimit,
            Prefix = module.Prefix,
            Number = module.Number,
            HasFremoDesignation = module.HasFremoDesignation,
            ModuleOwners = module.ModuleOwners,
            IsEndProfileSwitcher = module.IsEndProfileSwitcher,
            IsStandAlone = module.IsStandAlone,
            FunctionalState = module.FunctionalState,
            LandscapeState = module.LandscapeState,
            NumberOfThroughTracks = module.NumberOfThroughTracks,
            Gauges = drawing.Gauges(Lookup),
            RailSystems = drawing.RailSystems(Lookup),
            IsDuckunder = module.IsDuckunder,
            IsEndOfLine = module.IsEndOfLine,
            IsTurntable = module.IsTurntable,
            IsSignalModule = module.IsSignalModule,
            Measures = module.HasDrawing ? ModuleMeasurement.Derive(drawing, Lookup) : null,
            Exits = [.. drawing.Exits.Select(exit => new ModuleRegistryExit(exit.Label, exit.EndProfile.Id, exit.EndProfile.Name, exit.Direction))],
            Location = module.OperationLocation is { } location ? ModuleRegistryLocation.From(location) : null,
        };
    }
}

/// <summary>An exit as the Registry keeps it: its label, its end profile and the side it is on. From the drawing.</summary>
/// <param name="Label">The exit's label, unique within the module.</param>
/// <param name="EndProfileId">The catalogue id of the exit's end profile.</param>
/// <param name="EndProfileName">The name of the exit's end profile.</param>
/// <param name="Direction">The side of the module the exit is on; converted by name, never by number.</param>
public sealed record ModuleRegistryExit(string Label, Guid EndProfileId, string EndProfileName, ExitSide Direction);

/// <summary>The operation location a module is, as the Registry keeps it.</summary>
public sealed record ModuleRegistryLocation
{
    /// <summary>The full name. Both ways.</summary>
    public required string Name { get; init; }

    /// <summary>The signature. Both ways.</summary>
    public required string Signature { get; init; }

    /// <summary>The kind of location. From the drawing; the Registry's flags are mapped to and from it.</summary>
    public OperationLocationKind Kind { get; init; }

    /// <summary>A shadow station. Both ways.</summary>
    public bool IsShadow { get; init; }

    /// <summary>Trains end their run here. Both ways.</summary>
    public bool IsTerminus { get; init; }

    /// <summary>A harbour. Both ways.</summary>
    public bool IsHarbour { get; init; }

    /// <summary>A key is needed to unlock the switches. Both ways.</summary>
    public bool IsKeyRequired { get; init; }

    /// <summary>The name of the lock key, if any. Both ways.</summary>
    public string? LockKeyName { get; init; }

    /// <summary>A station with a turntable, as entered in Module Draw. From the drawing.</summary>
    public bool HasTurntable { get; init; }

    /// <summary>The location has signals. From the drawing; always so at a signal-controlled location.</summary>
    public bool HasSignals { get; init; }

    /// <summary>The location's tracks.</summary>
    public IReadOnlyList<ModuleRegistryTrack> Tracks { get; init; } = [];

    /// <summary>What <paramref name="location"/> shares with the Registry.</summary>
    public static ModuleRegistryLocation From(OperationLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        return new()
        {
            Name = location.Name,
            Signature = location.Signature,
            Kind = location.Kind,
            IsShadow = location is Station { IsShadow: true },
            IsTerminus = location.IsTerminus,
            IsHarbour = location.IsHarbour,
            IsKeyRequired = location.IsKeyRequired,
            LockKeyName = location.LockKey?.Name,
            HasTurntable = location is Station { HasTurntable: true },
            HasSignals = location.HasSignals,
            Tracks = [.. location.Tracks.OrderBy(track => track.DisplayOrder).Select(ModuleRegistryTrack.From)],
        };
    }
}

/// <summary>A station track as the Registry keeps it.</summary>
public sealed record ModuleRegistryTrack
{
    /// <summary>The track's designation, which matches it with the Registry's row. Both ways.</summary>
    public required string Designation { get; init; }

    /// <summary>The order the track is shown in. Both ways.</summary>
    public int DisplayOrder { get; init; }

    /// <summary>The direction the track is for. Both ways; converted by name, never by number.</summary>
    public TrackDirection Direction { get; init; }

    /// <summary>The usable length, metres, as the drawing gives it: the Registry's longest train. From the drawing.</summary>
    public double Length { get; init; }

    /// <summary>The track ends in a buffer stop. From the drawing.</summary>
    public bool IsStub { get; init; }

    /// <summary>The platform length, metres. Both ways.</summary>
    public double PlatformLength { get; init; }

    /// <summary>The track's speed limit, km/h. Both ways.</summary>
    public int? SpeedLimit { get; init; }

    /// <summary>The main track; the Registry's opposite of a siding. Both ways.</summary>
    public bool IsMain { get; init; }

    /// <summary>The track can be locked. Both ways.</summary>
    public bool IsLockable { get; init; }

    /// <summary>What the track is used for. Both ways.</summary>
    public string Usage { get; init; } = string.Empty;

    /// <summary>What <paramref name="track"/> shares with the Registry.</summary>
    public static ModuleRegistryTrack From(StationTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return new()
        {
            Designation = track.Designation,
            DisplayOrder = track.DisplayOrder,
            Direction = track.Direction,
            Length = track.Length,
            IsStub = track.IsStub,
            PlatformLength = track.PlatformLength,
            SpeedLimit = track.SpeedLimit,
            IsMain = track.IsMain,
            IsLockable = track.IsLockable,
            Usage = track.Usage,
        };
    }
}

/// <summary>Writes what the Registry owns or shares back into a module file (<see cref="ModuleRegistryData"/>).</summary>
public static class ModuleRegistryDataWriting
{
    extension(ModuleFileContent content)
    {
        /// <summary>
        /// The content with what <paramref name="data"/> has that goes both ways or from the Registry written onto a
        /// copy of its module, its location and the tracks with the same designation. What goes from the drawing
        /// (measures, features, exits, track lengths and stubs, the kind of location) and the drawing's geometry are
        /// left as they are. The content itself is not changed.
        /// </summary>
        public ModuleFileContent WithRegistryData(ModuleRegistryData data)
        {
            ArgumentNullException.ThrowIfNull(data);
            var module = content.Module.Copy();
            module.Name = data.Name;
            module.ConfigurationLabel = data.ConfigurationLabel;
            module.PackageLabel = data.PackageLabel;
            module.Note = data.Note;
            module.RepresentsFromYear = data.RepresentsFromYear;
            module.RepresentsUptoYear = data.RepresentsUptoYear;
            if (data.StandardId is Guid standardId && module.Standard?.Id != standardId)
                module.Standard = new ModuleStandard { Id = standardId, ShortName = data.StandardName ?? module.Standard?.ShortName ?? string.Empty };
            module.SpeedLimit = data.SpeedLimit;
            module.Prefix = data.Prefix;
            module.Number = data.Number;
            module.HasFremoDesignation = data.HasFremoDesignation;
            module.ModuleOwners = data.ModuleOwners;
            module.IsEndProfileSwitcher = data.IsEndProfileSwitcher;
            module.IsStandAlone = data.IsStandAlone;
            module.FunctionalState = data.FunctionalState;
            module.LandscapeState = data.LandscapeState;
            if (module.OperationLocation is { } location && data.Location is { } shared) WriteTo(location, shared);
            return content with { Module = module };
        }
    }

    private static void WriteTo(OperationLocation location, ModuleRegistryLocation shared)
    {
        location.Name = shared.Name;
        location.Signature = shared.Signature;
        location.IsTerminus = shared.IsTerminus;
        location.IsHarbour = shared.IsHarbour;
        location.IsKeyRequired = shared.IsKeyRequired;
        if (location is Station station) station.IsShadow = shared.IsShadow;
        if (location.LockKey is { } key && shared.LockKeyName is { Length: > 0 } keyName) key.Name = keyName;
        foreach (var track in location.Tracks)
        {
            if (shared.Tracks.FirstOrDefault(t => t.Designation.Equals(track.Designation, StringComparison.OrdinalIgnoreCase)) is not { } sharedTrack) continue;
            track.DisplayOrder = sharedTrack.DisplayOrder;
            track.Direction = sharedTrack.Direction;
            track.PlatformLength = sharedTrack.PlatformLength;
            track.SpeedLimit = sharedTrack.SpeedLimit;
            track.IsMain = sharedTrack.IsMain;
            track.IsLockable = sharedTrack.IsLockable;
            track.Usage = sharedTrack.Usage;
        }
    }
}
