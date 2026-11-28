using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Timetables;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// What the planner decides for an operation location in a timetable (<c>ModelSurfaceAnalysis.md</c> 5.1): its timings,
/// staffing, what reports leave out, its instructions and phone number. Kept by the timetable
/// (<see cref="Timetable.LocationPlanning"/>), keyed by the location's id, so that the shared layout has only what the
/// location is. Read and written through <c>timetable.PlanningOf(location)</c>.
/// </summary>
public sealed class LocationPlanning
{
    /// <summary>The <see cref="OperationLocation.Id"/> of the location.</summary>
    public int LocationId { get; set; }

    /// <summary>The operational times of the location; a value that is null takes the timetable's default.</summary>
    public StationTimings Timings { get; set; } = new();

    /// <summary>The phone number of whoever operates the location. Optional.</summary>
    public int? PhoneNumber { get; set; }

    /// <summary>
    /// Instructions, as markdown, for how the location is worked in this layout: which tracks are used for what, how
    /// the shunting is arranged. Not the owner's general instructions for the module.
    /// </summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>Suppresses notes about trains crossing or overtaking at this location.</summary>
    public bool HideMeets { get; set; }

    /// <summary>Suppresses trains that don't stop at this location, in some reports. Never for a halt.</summary>
    public bool HidePassings { get; set; }

    /// <summary>
    /// The people staffing a station, each with the roles they fill. Only in force while the station is manned; kept
    /// either way, so unmanning a station and manning it again does not lose them.
    /// </summary>
    public IList<StaffPost> StaffPosts { get; set; } = [];

    /// <summary>How many shifts a long session is divided into at a station (1–3).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int ShiftCount { get; set; } = 1;

    /// <summary>Which trains starting at a station get a start label; a train's category can narrow it further.</summary>
    public StartLabelPrinting StartLabels { get; set; }

    /// <summary>A station's dispatch list is divided into one list per direction instead of one for the whole station.</summary>
    public bool SplitDispatchByDirection { get; set; }

    /// <summary>Passengers change trains at a station; in force only while the layout is worked with passenger tickets.</summary>
    public bool IsPassengerInterchange { get; set; }

    /// <summary>A copy for the location <paramref name="locationId"/>, with copies of the timings and staff posts.</summary>
    public LocationPlanning CopyFor(int locationId)
    {
        var copy = (LocationPlanning)MemberwiseClone();
        copy.LocationId = locationId;
        copy.Timings = Timings.Copy();
        copy.StaffPosts = [.. StaffPosts.Select(post => new StaffPost { Roles = post.Roles, ExtensionData = post.ExtensionData })];
        return copy;
    }
}

/// <summary>
/// Which way through its location a station track is for (<c>ModelSurfaceAnalysis.md</c> 5.1): the planner's decision,
/// kept by the timetable (<see cref="Timetable.StationTrackRoutes"/>) keyed by the location's and the track's ids. Read
/// and written through <c>timetable.RouteOf(track)</c>.
/// </summary>
public sealed class StationTrackRoute
{
    /// <summary>The <see cref="OperationLocation.Id"/> of the location the track belongs to.</summary>
    public int LocationId { get; set; }

    /// <summary>The <see cref="StationTrack.Id"/> of the track.</summary>
    public int TrackId { get; set; }

    /// <summary>Whether this is the route of <paramref name="track"/>.</summary>
    public bool IsOf(StationTrack track) => track is not null && TrackId == track.Id && LocationId == LocationIdOf(track);

    /// <summary>The id of the location <paramref name="track"/> belongs to.</summary>
    internal static int LocationIdOf(StationTrack track) => track.Station?.Id ?? track.StationId;

    /// <summary>
    /// The <see cref="OperationLocation.Id"/> of the location a train comes from for this to be its track, or
    /// <c>null</c> where the track says nothing about where a train comes from. With <see cref="NextLocationId"/>, it
    /// says which way through the location the track is for.
    /// </summary>
    public int? PreviousLocationId { get; set; }

    /// <summary>The <see cref="OperationLocation.Id"/> of the location a train goes on to for this to be its track; see <see cref="PreviousLocationId"/>.</summary>
    public int? NextLocationId { get; set; }

    /// <summary>The track's route counts the other way round as well: a track for trains from A to B is then equally the track for trains from B to A.</summary>
    public bool AppliesInBothDirections { get; set; }

    /// <summary>Whether the route says anything: a location before or after.</summary>
    [JsonIgnore]
    public bool IsGiven => PreviousLocationId is not null || NextLocationId is not null;

    /// <summary>
    /// How well this route fits a train that comes from <paramref name="previousLocationId"/> and goes on to
    /// <paramref name="nextLocationId"/> — either of them <c>null</c> where the train starts or ends its run here. The
    /// bigger the number the better the fit, and <c>null</c> says the train's route contradicts one of the track's named
    /// locations, so this is not its track at all.
    /// </summary>
    /// <remarks>
    /// A named location the train's route confirms is what makes a track the one to use, so it weighs more than anything
    /// else; a named location the train has none of — it starts or ends its run here — counts a little against the
    /// track, leaving it behind one that names only what the train does confirm. A route naming no location comes to
    /// zero: it fits every train, claiming and contradicting none.
    /// </remarks>
    public int? Match(int? previousLocationId, int? nextLocationId)
    {
        var asNamed = Fit(previousLocationId, nextLocationId);
        if (!AppliesInBothDirections) return asNamed;
        var reversed = Fit(nextLocationId, previousLocationId);
        return asNamed is null ? reversed :
            reversed is null ? asNamed :
            Math.Max(asNamed.Value, reversed.Value);

        // Both ends have to hold at once, so a fit is the two sides added together, and nothing at all as soon as
        // either side is contradicted.
        int? Fit(int? from, int? to) =>
            Side(PreviousLocationId, from) is { } fromSide && Side(NextLocationId, to) is { } toSide
                ? fromSide + toSide
                : null;

        // What one side of the track's route is worth against the train's.
        static int? Side(int? named, int? actual) =>
            named is null ? 0 :     // the track says nothing about this side: neither for nor against
            actual is null ? -1 :   // the train has no location here, starting or ending its run
            named == actual ? 2 :   // the train's route confirms the track's
            null;                   // the route contradicts it: not this train's track
    }

    /// <summary>A copy for <paramref name="track"/>.</summary>
    public StationTrackRoute CopyFor(StationTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        var copy = (StationTrackRoute)MemberwiseClone();
        copy.TrackId = track.Id;
        copy.LocationId = LocationIdOf(track);
        return copy;
    }
}

/// <summary>Reads and writes what the planner decides for a location and a track (<see cref="LocationPlanning"/>, <see cref="StationTrackRoute"/>).</summary>
public static class LocationPlanningExtensions
{
    extension(Timetable timetable)
    {
        /// <summary>
        /// What the planner has decided for <paramref name="location"/>. When nothing is decided, a record with the
        /// defaults that is not kept; use <see cref="EnsurePlanningOf"/> to change it.
        /// </summary>
        public LocationPlanning PlanningOf(OperationLocation location)
        {
            ArgumentNullException.ThrowIfNull(location);
            return timetable.FindPlanningOf(location.Id) ?? new LocationPlanning { LocationId = location.Id };
        }

        /// <summary>What the planner has decided for the location <paramref name="locationId"/>, if anything.</summary>
        public LocationPlanning? FindPlanningOf(int locationId) => timetable.LocationPlanning.FirstOrDefault(p => p.LocationId == locationId);

        /// <summary>What the planner has decided for <paramref name="location"/>, kept by the timetable so that it can be changed.</summary>
        public LocationPlanning EnsurePlanningOf(OperationLocation location)
        {
            ArgumentNullException.ThrowIfNull(location);
            if (timetable.FindPlanningOf(location.Id) is { } existing) return existing;
            var planning = new LocationPlanning { LocationId = location.Id };
            timetable.LocationPlanning.Add(planning);
            return planning;
        }

        /// <summary>
        /// Brings what is decided for <paramref name="location"/> in line with its kind, as after a change of kind:
        /// instructions only where the kind can have them, and no hiding of passing trains at a halt.
        /// </summary>
        public void ConformPlanningTo(OperationLocation location)
        {
            ArgumentNullException.ThrowIfNull(location);
            if (timetable.FindPlanningOf(location.Id) is not { } planning) return;
            if (!OperationLocationRules.CanHaveInstructions(location.Kind)) planning.Instructions = string.Empty;
            if (location is OtherLocation) planning.HidePassings = false;
        }

        /// <summary>
        /// The rules that what the planner has decided for <paramref name="location"/> breaks: instructions where its kind
        /// can't have them.
        /// </summary>
        public IReadOnlyList<LocationProblem> PlanningProblems(OperationLocation location)
        {
            ArgumentNullException.ThrowIfNull(location);
            return !string.IsNullOrWhiteSpace(timetable.PlanningOf(location).Instructions) && !OperationLocationRules.CanHaveInstructions(location.Kind)
                ? [new(LocationProblemCode.InstructionsNotAllowed, nameof(LocationPlanning.Instructions))]
                : [];
        }

        /// <summary>Forgets what was decided for <paramref name="location"/>, as when it is removed.</summary>
        public void ForgetPlanningOf(OperationLocation location)
        {
            ArgumentNullException.ThrowIfNull(location);
            foreach (var planning in timetable.LocationPlanning.Where(p => p.LocationId == location.Id).ToList()) timetable.LocationPlanning.Remove(planning);
            foreach (var track in location.Tracks) timetable.ForgetRouteOf(track);
        }

        /// <summary>
        /// Which way through its location <paramref name="track"/> is for. When nothing is decided, a route with the
        /// defaults that is not kept; use <see cref="EnsureRouteOf"/> to change it.
        /// </summary>
        public StationTrackRoute RouteOf(StationTrack track)
        {
            ArgumentNullException.ThrowIfNull(track);
            return timetable.StationTrackRoutes.FirstOrDefault(r => r.IsOf(track)) ?? new StationTrackRoute { TrackId = track.Id, LocationId = StationTrackRoute.LocationIdOf(track) };
        }

        /// <summary>Which way through its location <paramref name="track"/> is for, kept by the timetable so that it can be changed.</summary>
        public StationTrackRoute EnsureRouteOf(StationTrack track)
        {
            ArgumentNullException.ThrowIfNull(track);
            if (timetable.StationTrackRoutes.FirstOrDefault(r => r.IsOf(track)) is { } existing) return existing;
            var route = new StationTrackRoute { TrackId = track.Id, LocationId = StationTrackRoute.LocationIdOf(track) };
            timetable.StationTrackRoutes.Add(route);
            return route;
        }

        /// <summary>
        /// Clears every track route naming <paramref name="location"/> (<see cref="StationTrackRoute.PreviousLocationId"/>),
        /// so no track is left reserved for trains to or from somewhere the layout no longer has. Call it when a location
        /// is removed. Returns <c>true</c> when a route was changed.
        /// </summary>
        public bool ForgetTrackRoutesTo(OperationLocation? location)
        {
            if (location is null) return false;
            var changed = false;
            foreach (var route in timetable.StationTrackRoutes)
            {
                if (route.PreviousLocationId == location.Id) { route.PreviousLocationId = null; changed = true; }
                if (route.NextLocationId == location.Id) { route.NextLocationId = null; changed = true; }
                if (!route.IsGiven) route.AppliesInBothDirections = false;
            }
            return changed;
        }

        /// <summary>Forgets the route of <paramref name="track"/>, as when it is removed.</summary>
        public void ForgetRouteOf(StationTrack track)
        {
            ArgumentNullException.ThrowIfNull(track);
            foreach (var route in timetable.StationTrackRoutes.Where(r => r.IsOf(track)).ToList()) timetable.StationTrackRoutes.Remove(route);
        }
    }
}
