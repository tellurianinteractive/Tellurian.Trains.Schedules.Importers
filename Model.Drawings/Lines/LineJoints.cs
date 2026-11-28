namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>The joints of the lines of a <see cref="DrawnLayout"/> and how each works out (<c>LayoutsLayerSurvey.md</c> 2.8, 2.10).</summary>
public static class LineJoints
{
    extension(DrawnLayout layout)
    {
        /// <summary>
        /// Every joint of the lines once, seen from the exit the line runs from, with the fit of its ends, its join method
        /// and its height step. Joints that name a module or exit that doesn't exist are left out.
        /// </summary>
        public IReadOnlyList<JointCheck> JointChecks() => JointChecks(layout, new LineJoins(layout, new LineProblemLog()));
    }

    internal static IReadOnlyList<JointCheck> JointChecks(DrawnLayout layout, LineJoins joins) =>
        [.. joins.Forward.Select(j => Check(layout, joins, j.Key, j.Value))];

    private static JointCheck Check(DrawnLayout layout, LineJoins joins, ExitReference first, LineJoin join)
    {
        var second = join.Other;
        var exitA = first.Place.In(joins.Modules[first.ModuleId].Module.Drawing);
        var exitB = second.Place.In(joins.Modules[second.ModuleId].Module.Drawing);
        var profileA = exitA is null ? null : layout.FindEndProfile(exitA.EndProfile.Id);
        var profileB = exitB is null ? null : layout.FindEndProfile(exitB.EndProfile.Id);
        return new()
        {
            First = first,
            Second = second,
            LineId = join.LineId,
            IsClosing = join.IsClosing,
            Fit = profileA is null || profileB is null
                ? null
                : JointFitEvaluator.Evaluate(profileA, exitA!.Orientation, profileB, exitB!.Orientation, layout.Overrides, layout.EffectiveFitTolerances),
            Decision = layout.FindJoint(first, second),
            HeightStep = exitA is null || exitB is null
                ? null
                : RailTop(layout, exitB, second.ModuleId) - RailTop(layout, exitA, first.ModuleId),
        };
    }

    /// <summary>The rail top at <paramref name="exit"/> as set up: the exit's offset plus the module's leg adjustment, mm.</summary>
    private static double RailTop(DrawnLayout layout, ModuleExit exit, int moduleId) =>
        exit.RailTopOffset + (layout.FindEntry(moduleId)?.LegAdjustment ?? 0);
}
