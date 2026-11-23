using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>
/// Places the modules of a <see cref="HallPlan"/> in its hall (<c>LayoutsLayerSurvey.md</c> 2.8, the engine): each
/// connected group from its <see cref="Anchor"/>, module by module across the joints of its lines, exit against exit
/// (<see cref="Join.PlacementFor"/>). A <see cref="Line.EndAt"/> joint doesn't place a module: it closes a loop or a
/// rejoining line, and its gap is measured (<see cref="Join.Gap"/>).
/// </summary>
public static class HallPlanPlacing
{
    extension(HallPlan plan)
    {
        /// <summary>Where every module stands, the closure gaps, the joints, and the problems found. Nothing is changed.</summary>
        public HallPlanPlacement Placement()
        {
            var layout = plan.Layout;
            var log = new LineProblemLog();
            var joins = new LineJoins(layout, log);
            var placements = new Dictionary<int, Transform>();
            foreach (var mainLine in layout.MainLines) PlaceGroup(plan, joins, log, mainLine, placements);
            var closures = Closures(plan.Settings, joins, log, placements);
            var joints = LineJoints.JointChecks(layout, joins);
            foreach (var joint in joints)
            {
                if (joint.Fit?.Fit == JointFit.Incompatible) log.Report(new(LineProblemCode.JointDoesNotFit, joint.First.ModuleId, joint.First.Place));
                if (joint.HeightStep is double step && Math.Abs(step) > plan.Settings.Height) log.Report(new(LineProblemCode.HeightStepTooLarge, joint.First.ModuleId, joint.First.Place));
            }
            return new([.. placements.Select(p => new ModulePlacement(p.Key, p.Value))], closures, joints, log.Problems);
        }
    }

    /// <summary>Places the group of <paramref name="mainLine"/> from its anchor, across every joint that isn't closing.</summary>
    private static void PlaceGroup(HallPlan plan, LineJoins joins, LineProblemLog log, Line mainLine, Dictionary<int, Transform> placements)
    {
        var modules = plan.Layout.GroupOf(mainLine).SelectMany(l => l.Entries).Select(e => e.ModuleId).Distinct().ToList();
        if (plan.FindAnchor(mainLine.Id) is { } anchor && modules.Contains(anchor.ModuleId) && joins.Modules.ContainsKey(anchor.ModuleId))
        {
            placements[anchor.ModuleId] = Transform.FromPose(anchor.Pose);
            var queue = new Queue<int>([anchor.ModuleId]);
            while (queue.TryDequeue(out var moduleId))
            {
                foreach (var place in LineJoins.ExitsOf(joins.Modules[moduleId]))
                {
                    var exit = ExitReference.To(moduleId, place);
                    if (!joins.TryGet(exit, out var join) || join.IsClosing || placements.ContainsKey(join.Other.ModuleId)) continue;
                    if (DrawnPose(joins, exit) is not { } from)
                    {
                        log.Report(new(LineProblemCode.ExitNotDrawn, exit.ModuleId, exit.Place));
                        continue;
                    }
                    if (DrawnPose(joins, join.Other) is not { } to)
                    {
                        log.Report(new(LineProblemCode.ExitNotDrawn, join.Other.ModuleId, join.Other.Place));
                        continue;
                    }
                    placements[join.Other.ModuleId] = Join.PlacementFor(placements[moduleId].Apply(from), to);
                    queue.Enqueue(join.Other.ModuleId);
                }
            }
        }
        foreach (var moduleId in modules.Where(id => !placements.ContainsKey(id) && joins.Modules.ContainsKey(id)))
            log.Report(new(LineProblemCode.ModuleNotPlaced, moduleId));
    }

    private static List<ClosureCheck> Closures(HallPlanSettings settings, LineJoins joins, LineProblemLog log, Dictionary<int, Transform> placements)
    {
        var closures = new List<ClosureCheck>();
        foreach (var (first, join) in joins.Forward.Where(j => j.Value.IsClosing))
        {
            var second = join.Other;
            if (!placements.TryGetValue(first.ModuleId, out var a) || !placements.TryGetValue(second.ModuleId, out var b)) continue;
            if (DrawnPose(joins, first) is not { } poseA || DrawnPose(joins, second) is not { } poseB) continue;
            var gap = Join.Gap(a.Apply(poseA), b.Apply(poseB));
            var isWithin = gap.IsWithin(settings.JointGap, settings.JointAngle);
            if (!isWithin) log.Report(new(LineProblemCode.ClosureGapTooLarge, first.ModuleId, first.Place));
            closures.Add(new(first, second, gap, isWithin));
        }
        return closures;
    }

    /// <summary>The pose of the exit in its module's drawing; null when the drawing has no such exit.</summary>
    private static Pose? DrawnPose(LineJoins joins, ExitReference exit) =>
        exit.Place.In(joins.Modules[exit.ModuleId].Module.Drawing)?.Pose;
}
