using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Takes the layout of a hall plan (<see cref="HallPlanFile.ReadLayout"/>, <c>drawnLayout.ToLayout()</c>), with its track
/// stretches worked out from its lines and what was measured, into the planner: as the layout of a new plan
/// (<see cref="PlanFactory.CreatePlan(Layout, string)"/>), or as a newer revision of the hall plan an existing plan was
/// made from (<c>plan.UpdateFromHallPlan(layout)</c>).
/// </summary>
public static class HallPlanImport
{
    extension(Plan plan)
    {
        /// <summary>
        /// Whether the plan's layout was made from the same source as <paramref name="source"/>: both name the same
        /// <see cref="Layout.SourceId"/>. Such a layout is a revision of the plan's own, to be taken in with
        /// <c>UpdateFromHallPlan</c> rather than made a new plan of.
        /// </summary>
        public bool IsMadeFrom(Layout source)
        {
            ArgumentNullException.ThrowIfNull(source);
            return plan.Layout.SourceId is Guid id && id == source.SourceId;
        }

        /// <summary>
        /// Compares the plan's layout with <paramref name="hallPlan"/>, a newer revision of its hall plan, and applies the
        /// differences that change only distances and times (<see cref="LayoutUpdate.ApplyAutomatic"/>). The differences
        /// the planner must confirm are left for <c>plan.PreviewChange</c> and <c>plan.ApplyChange</c>; those that break
        /// the order of the locations are only reported.
        /// </summary>
        /// <returns>What was compared, what was applied, and what is left.</returns>
        public HallPlanUpdate UpdateFromHallPlan(Layout hallPlan)
        {
            ArgumentNullException.ThrowIfNull(hallPlan);
            var comparison = LayoutComparison.Compare(plan.Layout, hallPlan);
            var applied = LayoutUpdate.ApplyAutomatic(comparison);
            foreach (var change in applied.Where(c => c.Kind == LayoutChangeKind.StretchChanged))
            {
                var stretch = change.Stretch!;
                var isTaken = (!stretch.IsDistanceEntered && change.Properties.Contains(nameof(TrackStretch.Distance))) ||
                    (!stretch.IsSpeedEntered && change.Properties.Contains(nameof(TrackStretch.Speed)));
                if (isTaken && !stretch.IsTimeEntered) stretch.Time = TrackStretchTiming.RunningMinutes(stretch, plan.Timetable.Settings.TimeAndSpeed);
            }
            plan.Timetable.EnsureCountries();
            return new(comparison, applied);
        }
    }

    /// <summary>
    /// Moves what <paramref name="hallPlan"/> has into <paramref name="layout"/>, a new layout with the planner's
    /// defaults: the locations with their tracks, the stretches with a running time worked out from distance and speed,
    /// the modules, the order of the locations along the lines, and the hall plan's identity, scale and standard.
    /// </summary>
    internal static void TakeInto(Timetable timetable, Layout hallPlan)
    {
        var layout = timetable.Layout;
        foreach (var location in hallPlan.OperationLocations)
        {
            layout.Add(location);
            TakePlanningFrom(timetable, location);
        }
        foreach (var name in TimetableData) layout.ExtensionData?.Remove(name);
        foreach (var stretch in hallPlan.TrackStretches)
        {
            stretch.Layout = layout;
            stretch.LayoutId = layout.Id;
            if (!stretch.IsTimeEntered) stretch.Time = TrackStretchTiming.RunningMinutes(stretch, timetable.Settings.TimeAndSpeed);
            layout.Add(stretch);
        }
        layout.StretchLines = [.. hallPlan.StretchLines.Select(l => l.Copy())];
        layout.Modules = [.. hallPlan.Modules.Select(m => m.Copy())];
        layout.SourceId = hallPlan.SourceId;
        layout.SourceRevision = hallPlan.SourceRevision;
        layout.Scale = hallPlan.Scale;
        layout.StandardName = hallPlan.StandardName;
        if (Scale.Known(hallPlan.Scale)?.Id is int scaleId) timetable.Settings.Identity.ScaleId = scaleId;
        timetable.EnsureCountries();
    }

    // What older hall plans wrote in their layout block that is now the timetable's (ModelSurfaceAnalysis.md 5.1):
    // nothing a hall plan decides, so it is left out of the new plan.
    private static readonly string[] TimetableData = ["settings", "companies", "timetableStretches", "dispatchStretches", "topologyPositions"];

    private static readonly string[] LocationPlanningData =
        ["timings", "phoneNumber", "instructions", "hideMeets", "hidePassings", "staffPosts", "shiftCount", "startLabels", "splitDispatchByDirection", "isPassengerInterchange"];

    private static readonly string[] TrackRouteData = ["previousLocationId", "nextLocationId", "appliesInBothDirections"];

    /// <summary>
    /// Takes what an older hall plan wrote on <paramref name="location"/> and its tracks that is now the timetable's
    /// (instructions entered in Module Draw, for one) out of their unknown properties into
    /// <paramref name="timetable"/>, where it differs from the defaults.
    /// </summary>
    private static void TakePlanningFrom(Timetable timetable, OperationLocation location)
    {
        var options = PlanJson.CreateOptions();
        if (Taken(location, LocationPlanningData, "locationId") is { } planningJson &&
            planningJson.Deserialize<LocationPlanning>(options) is { } planning &&
            !IsDefault(planning, new LocationPlanning { LocationId = location.Id }, options))
        {
            timetable.ForgetPlanningOf(location);
            timetable.LocationPlanning.Add(planning);
        }
        foreach (var track in location.Tracks)
        {
            if (Taken(track, TrackRouteData, "trackId") is not { } routeJson || routeJson.Deserialize<StationTrackRoute>(options) is not { } route || !route.IsGiven) continue;
            route.LocationId = location.Id;
            timetable.ForgetRouteOf(track);
            timetable.StationTrackRoutes.Add(route);
        }
    }

    private static JsonObject? Taken(LayoutPart part, string[] names, string key)
    {
        if (part.ExtensionData is not { Count: > 0 } extra) return null;
        JsonObject? taken = null;
        foreach (var name in names)
        {
            if (!extra.Remove(name, out var value)) continue;
            taken ??= [];
            taken[name] = JsonNode.Parse(value.GetRawText());
        }
        if (taken is null) return null;
        taken[key] = part switch { OperationLocation location => location.Id, StationTrack track => track.Id, _ => 0 };
        return taken;
    }

    private static bool IsDefault<T>(T value, T defaults, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(value, options) == JsonSerializer.Serialize(defaults, options);

    /// <summary>The country most of the hall plan's locations are in, if any of them has one the planner knows.</summary>
    internal static int? MostCommonCountryId(Layout hallPlan) =>
        hallPlan.OperationLocations
            .Select(l => l.CountryId)
            .OfType<int>()
            .GroupBy(id => id)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => (int?)g.Key)
            .FirstOrDefault();
}

/// <summary>
/// What loading a newer revision of a hall plan into a plan did (<see cref="Applied"/>), and what is left: the changes
/// the planner must confirm (<see cref="ToConfirm"/>) and those that break the order of the locations
/// (<see cref="Manual"/>).
/// </summary>
public sealed record HallPlanUpdate(LayoutComparison Comparison, IReadOnlyList<LayoutChange> Applied)
{
    /// <summary>The changes that alter calls or trains in a known way, for the planner to preview and confirm.</summary>
    public IReadOnlyList<LayoutChange> ToConfirm => [.. Comparison.Changes.Where(c => c.Handling == LayoutChangeHandling.Confirm)];

    /// <summary>The changes that break the order of the locations; reported, but not applied by the planner.</summary>
    public IReadOnlyList<LayoutChange> Manual => [.. Comparison.Changes.Where(c => c.Handling == LayoutChangeHandling.Manual)];

    /// <summary>True when the hall plan had no difference from the plan's layout.</summary>
    public bool IsUnchanged => Comparison.IsUnchanged;
}
