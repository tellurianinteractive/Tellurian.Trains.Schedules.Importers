namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The rules of operation locations that every system checks (<c>Documentation/OperationLocations.md</c>, section 6.9).
/// What a kind can have at all is given by its type: only a <see cref="Station"/> can be a shadow station, have a
/// turntable or be manned, and the exchange that a kind never or always has is fixed by it. What is left are the rules
/// between properties, checked here with a code per broken rule.
/// </summary>
public static class OperationLocationRules
{
    /// <summary>The number of letters of a country code.</summary>
    public const int CountryCodeLength = 2;

    /// <summary>Whether a location of <paramref name="kind"/> can be a shadow station, have a turntable and be manned.</summary>
    public static bool IsStation(OperationLocationKind kind) => kind == OperationLocationKind.Station;

    /// <summary>Whether passengers can be exchanged at a location of <paramref name="kind"/>.</summary>
    public static bool CanHavePassengerExchange(OperationLocationKind kind) => kind is OperationLocationKind.Station or OperationLocationKind.Other;

    /// <summary>Whether cargo can be exchanged at a location of <paramref name="kind"/>.</summary>
    public static bool CanHaveCargoExchange(OperationLocationKind kind) => kind is OperationLocationKind.Station or OperationLocationKind.IndustrialArea;

    /// <summary>Whether cargo is always exchanged at a location of <paramref name="kind"/>.</summary>
    public static bool AlwaysHasCargoExchange(OperationLocationKind kind) => kind == OperationLocationKind.IndustrialArea;

    /// <summary>Whether trains can change direction at a location of <paramref name="kind"/>.</summary>
    public static bool CanChangeTrainDirection(OperationLocationKind kind) => kind is OperationLocationKind.Station or OperationLocationKind.SignalControlled;

    /// <summary>Whether trains ever stop at a location of <paramref name="kind"/>.</summary>
    public static bool TrainsCanStop(OperationLocationKind kind) => kind != OperationLocationKind.SignalControlled;

    /// <summary>Whether a location of <paramref name="kind"/> can be worked from a manned station.</summary>
    public static bool CanBeControlled(OperationLocationKind kind) => kind != OperationLocationKind.Other;

    /// <summary>Whether a location of <paramref name="kind"/> can have instructions for a layout.</summary>
    public static bool CanHaveInstructions(OperationLocationKind kind) => kind is OperationLocationKind.Station or OperationLocationKind.IndustrialArea;

    /// <summary>
    /// Whether trains can cross at <paramref name="location"/>: at a station when more than one of its tracks is
    /// scheduled, at a signal controlled location when it says so, otherwise not.
    /// </summary>
    public static bool TrainsCanCrossAt(OperationLocation location) => TrainsCanCrossAt(location, location?.Tracks ?? []);

    /// <summary>
    /// Whether trains can cross at <paramref name="location"/> with <paramref name="tracks"/>, for a location whose
    /// tracks are kept elsewhere, such as on its module in a module file.
    /// </summary>
    public static bool TrainsCanCrossAt(OperationLocation location, IEnumerable<StationTrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(tracks);
        return location switch
        {
            Station => tracks.Count(t => t.IsScheduled) > 1,
            SignalControlledLocation signalControlled => signalControlled.TrainsCanCross,
            IndustrialArea or OtherLocation => false,
        };
    }

    /// <summary>The rules that <paramref name="location"/> itself breaks; those of its tracks are checked by <see cref="Validate(IEnumerable{StationTrack})"/>.</summary>
    public static IReadOnlyList<LocationProblem> Validate(OperationLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        var problems = new List<LocationProblem>();
        void Add(bool isBroken, LocationProblemCode code, string property)
        {
            if (isBroken) problems.Add(new(code, property));
        }

        Add(string.IsNullOrWhiteSpace(location.Name), LocationProblemCode.NameMissing, nameof(OperationLocation.Name));
        Add(string.IsNullOrWhiteSpace(location.Signature), LocationProblemCode.SignatureMissing, nameof(OperationLocation.Signature));
        Add(location.CountryCode is { } code && !(code.Length == CountryCodeLength && code.All(char.IsAsciiLetterUpper)), LocationProblemCode.CountryCodeInvalid, nameof(OperationLocation.CountryCode));
        Add(location.Centre < 0, LocationProblemCode.CentreNegative, nameof(OperationLocation.Centre));
        return problems;
    }

    /// <summary>The rules that <paramref name="tracks"/>, the tracks of one location or module, break.</summary>
    public static IReadOnlyList<LocationProblem> Validate(IEnumerable<StationTrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        var problems = new List<LocationProblem>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var track in tracks)
        {
            var designation = track.Designation?.Trim() ?? string.Empty;
            if (designation.Length == 0)
                problems.Add(new(LocationProblemCode.TrackDesignationMissing, nameof(StationTrack.Designation), designation));
            else if (!seen.Add(designation) && reported.Add(designation))
                problems.Add(new(LocationProblemCode.TrackDesignationNotUnique, nameof(StationTrack.Designation), designation));
            if (!track.IsLockable && !string.IsNullOrWhiteSpace(track.KeyName)) problems.Add(new(LocationProblemCode.KeyNameWithoutLock, nameof(StationTrack.KeyName), designation));
            if (track.Length < 0) problems.Add(new(LocationProblemCode.TrackLengthNegative, nameof(StationTrack.Length), designation));
            if (track.PlatformLength < 0) problems.Add(new(LocationProblemCode.TrackLengthNegative, nameof(StationTrack.PlatformLength), designation));
        }
        return problems;
    }

    /// <summary>
    /// The rules that what is decided for <paramref name="location"/> in <paramref name="layout"/> breaks: who controls
    /// it. The planner's own decisions, such as instructions, are checked by the planner.
    /// </summary>
    public static IReadOnlyList<LocationProblem> Validate(OperationLocation location, Layout layout)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(layout);
        var problems = new List<LocationProblem>();
        var kind = location.Kind;
        if (location.ControlledBy is { } controller)
        {
            if (!CanBeControlled(kind) || location is Station { IsManned: true })
                problems.Add(new(LocationProblemCode.ControlledByNotAllowed, nameof(OperationLocation.ControlledBy)));
            else if (!controller.IsManned || ReferenceEquals(controller, location) || !layout.OperationLocations.Contains(controller))
                problems.Add(new(LocationProblemCode.ControlledByNotManned, nameof(OperationLocation.ControlledBy)));
        }
        return problems;
    }
    /// <summary>
    /// <paramref name="location"/> as a location of <paramref name="kind"/>: a new object of that kind with what the
    /// kind can have, its tracks moved to it. What only the old kind could have is left behind
    /// (<see cref="ClearedBy"/>), and what the new kind always or never has follows from its type. When the location is
    /// already of <paramref name="kind"/>, a copy of it. For a change of kind, after the user has agreed to what is cleared.
    /// </summary>
    public static OperationLocation Conforming(OperationLocation location, OperationLocationKind kind)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (location.Kind == kind) return location.Copy();
        var source = location.Copy();
        var target = OperationLocation.Create(kind);
        target.Id = source.Id;
        target.Name = source.Name;
        target.Signature = source.Signature;
        target.CountryCode = source.CountryCode;
        target.Owner = source.Owner;
        target.IsChangingTrainDirectionPossible = source.IsChangingTrainDirectionPossible;
        target.HasPassengerExchange = source.HasPassengerExchange;
        target.HasCargoExchange = source.HasCargoExchange;
        target.IsTerminus = source.IsTerminus;
        target.IsHarbour = source.IsHarbour;
        target.IsKeyRequired = source.IsKeyRequired;
        target.Centre = source.Centre;
        target.ExternalReferences = source.ExternalReferences;
        target.ExtensionData = source.ExtensionData;
        target.Layout = source.Layout;
        target.LayoutId = source.LayoutId;
        if (CanBeControlled(kind)) target.ControlledBy = source.ControlledBy;
        if (CanHaveCargoExchange(kind))
        {
            target.CargoServedFrom = source.CargoServedFrom;
            target.LockKey = source.LockKey;
        }
        if (source is SignalControlledLocation { TrainsCanCross: true } && target is SignalControlledLocation crossing) crossing.TrainsCanCross = true;
        if (source.HasSignals)
        {
            if (target is Station signalledStation) signalledStation.HasSignals = true;
            if (target is IndustrialArea signalledArea) signalledArea.HasSignals = true;
        }
        target.Tracks = source.Tracks;
        foreach (var track in target.Tracks) track.Station = target;
        return target;
    }

    /// <summary>
    /// The properties of <paramref name="location"/> that are set now and would be cleared by changing it to
    /// <paramref name="kind"/>, by name; empty when nothing is lost.
    /// </summary>
    public static IReadOnlyList<string> ClearedBy(OperationLocation location, OperationLocationKind kind)
    {
        ArgumentNullException.ThrowIfNull(location);
        var after = Conforming(location, kind);
        var cleared = new List<string>();
        void Add(bool before, bool now, string property)
        {
            if (before && !now) cleared.Add(property);
        }

        Add(location is Station { IsShadow: true }, after is Station { IsShadow: true }, nameof(Station.IsShadow));
        Add(location is Station { HasTurntable: true }, after is Station { HasTurntable: true }, nameof(Station.HasTurntable));
        Add(location.HasSignals, after.HasSignals, nameof(Station.HasSignals));
        Add(location is Station { IsManned: true }, after is Station { IsManned: true }, nameof(Station.IsManned));
        Add(location.HasPassengerExchange, after.HasPassengerExchange, nameof(OperationLocation.HasPassengerExchange));
        Add(location.HasCargoExchange, after.HasCargoExchange, nameof(OperationLocation.HasCargoExchange));
        Add(location.IsChangingTrainDirectionPossible, after.IsChangingTrainDirectionPossible, nameof(OperationLocation.IsChangingTrainDirectionPossible));
        Add(location is SignalControlledLocation { TrainsCanCross: true }, after is SignalControlledLocation { TrainsCanCross: true }, nameof(SignalControlledLocation.TrainsCanCross));
        Add(location.ControlledBy is not null, after.ControlledBy is not null, nameof(OperationLocation.ControlledBy));
        return cleared;
    }
}
