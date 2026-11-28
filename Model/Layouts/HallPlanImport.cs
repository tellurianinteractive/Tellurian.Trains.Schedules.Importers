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
            plan.Layout.EnsureCountries();
            return new(comparison, applied);
        }
    }

    /// <summary>
    /// Moves what <paramref name="hallPlan"/> has into <paramref name="layout"/>, a new layout with the planner's
    /// defaults: the locations with their tracks, the stretches with a running time worked out from distance and speed,
    /// any timetable and dispatch stretches and companies, the modules, the order of the locations along the lines, and
    /// the hall plan's identity, scale and standard.
    /// </summary>
    internal static void TakeInto(Layout layout, Layout hallPlan)
    {
        foreach (var location in hallPlan.OperationLocations) layout.Add(location);
        foreach (var stretch in hallPlan.TrackStretches)
        {
            stretch.Layout = layout;
            stretch.LayoutId = layout.Id;
            if (!stretch.IsTimeEntered) stretch.Time = TrackStretchTiming.RunningMinutes(stretch, layout.Settings.TimeAndSpeed);
            layout.Add(stretch);
        }
        foreach (var stretch in hallPlan.TimetableStretches) layout.Add(stretch);
        foreach (var stretch in hallPlan.DispatchStretches) layout.DispatchStretches.Add(stretch);
        foreach (var company in hallPlan.Companies) layout.Add(company);
        layout.StretchLines = [.. hallPlan.StretchLines.Select(l => l.Copy())];
        layout.Modules = [.. hallPlan.Modules.Select(m => m.Copy())];
        layout.SourceId = hallPlan.SourceId;
        layout.SourceRevision = hallPlan.SourceRevision;
        layout.Scale = hallPlan.Scale;
        layout.StandardName = hallPlan.StandardName;
        if (Scale.Known(hallPlan.Scale)?.Id is int scaleId) layout.Settings.Identity.ScaleId = scaleId;
        layout.EnsureCountries();
    }

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
