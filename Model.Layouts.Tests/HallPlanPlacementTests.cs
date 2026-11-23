using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;
using Tellurian.Trains.Schedules.Model.Layouts.Halls;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// The engine of a hall plan (<c>LayoutsLayerSurvey.md</c> 2.8): modules placed from the anchors along the lines, the
/// closure gaps of loops, and the joints' fits and height steps.
/// </summary>
[TestClass]
public sealed class HallPlanPlacementTests
{
    private const double ModuleLength = 1000;
    private const double Radius = 2000;

    private static readonly EndProfile SingleTrack = Profile("Single", 0);
    private static readonly EndProfile DoubleTrack = Profile("Double", -23, 23);

    [TestMethod]
    public void ModulesArePlacedExitAgainstExitFromTheAnchor()
    {
        var plan = Plan();
        var (s1, s2, s3) = (Straight(plan, "s1"), Straight(plan, "s2"), Straight(plan, "s3"));
        var line = plan.Layout.Add(new Line { Entries = [Entry(s1), Entry(s2), Entry(s3)] });
        plan.Anchors.Add(new() { LineId = line.Id, ModuleId = s2.Id, Pose = new(new(5000, 2000), Angle.Quarter) });

        var placement = plan.Placement();

        Assert.IsEmpty(placement.Problems);
        AssertPose(new(new(5000, 1000), Angle.Quarter), placement.Find(s1.Id)!.Pose);
        AssertPose(new(new(5000, 2000), Angle.Quarter), placement.Find(s2.Id)!.Pose);
        AssertPose(new(new(5000, 3000), Angle.Quarter), placement.Find(s3.Id)!.Pose);
    }

    [TestMethod]
    public void ATurnedModuleIsPlacedTheOtherWayRound()
    {
        var plan = Plan();
        var (s1, s2) = (Straight(plan, "s1"), Straight(plan, "s2"));
        var line = plan.Layout.Add(new Line { Entries = [Entry(s1), Entry(s2, isTurned: true)] });
        plan.Anchors.Add(new() { LineId = line.Id, ModuleId = s1.Id });

        var placement = plan.Placement();

        AssertPose(new(new(2 * ModuleLength, 0), Angle.Half), placement.Find(s2.Id)!.Pose);
    }

    [TestMethod]
    [DataRow(90.0, true, DisplayName = "four quarter circles close")]
    [DataRow(85.0, false, DisplayName = "a curve 5° short leaves a gap")]
    public void ALoopsClosureGapIsMeasured(double lastAngle, bool closes)
    {
        var plan = Plan();
        var curves = new[] { 90.0, 90, 90, lastAngle }.Select((angle, i) => Curved(plan, $"c{i}", angle)).ToList();
        var line = plan.Layout.Add(new Line { Entries = [.. curves.Select(c => Entry(c))], EndAt = new(curves[0].Id, ExitSide.Left) });
        plan.Anchors.Add(new() { LineId = line.Id, ModuleId = curves[0].Id });

        var placement = plan.Placement();

        var closure = placement.Closures.Single();
        Assert.AreEqual(closes, closure.IsWithin);
        Assert.AreEqual(new ExitReference(curves[3].Id, ExitSide.Right), closure.First);
        if (closes)
        {
            Assert.AreEqual(0, closure.Gap.Distance, Precision);
            Assert.IsEmpty(placement.Problems);
        }
        else
        {
            Assert.AreEqual(5, Math.Abs(closure.Gap.Dθ.Signed.Degrees), Precision);
            Assert.AreEqual(new LineProblem(LineProblemCode.ClosureGapTooLarge, curves[3].Id, ExitPlace.Right), placement.Problems.Single());
        }
    }

    [TestMethod]
    public void AGroupWithoutAnAnchorIsNotPlaced()
    {
        var plan = Plan();
        var (s1, s2) = (Straight(plan, "s1"), Straight(plan, "s2"));
        plan.Layout.Add(new Line { Entries = [Entry(s1), Entry(s2)] });

        var placement = plan.Placement();

        Assert.IsEmpty(placement.Modules);
        CollectionAssert.AreEquivalent(
            new[] { new LineProblem(LineProblemCode.ModuleNotPlaced, s1.Id), new LineProblem(LineProblemCode.ModuleNotPlaced, s2.Id) },
            placement.Problems.ToList());
    }

    [TestMethod]
    public void JointsReportTheirFitHeightStepAndJoinMethod()
    {
        var plan = Plan();
        var s1 = Straight(plan, "s1");
        var s2 = Straight(plan, "s2");
        var s3 = Straight(plan, "s3", DoubleTrack);
        var line = plan.Layout.Add(new Line { Entries = [Entry(s1), new LineEntry { ModuleId = s2.Id, LegAdjustment = 2 }, Entry(s3)] });
        plan.Anchors.Add(new() { LineId = line.Id, ModuleId = s1.Id });
        plan.Layout.Joints.Add(new Joint { First = new(s1.Id, ExitSide.Right), Second = new(s2.Id, ExitSide.Left), JoinMethodOverride = JoinMethod.Clamped, NumberOfClamps = 3 });

        var placement = plan.Placement();

        var (first, second) = (placement.Joints[0], placement.Joints[1]);
        Assert.AreEqual(JointFit.Fits, first.Fit!.Fit);
        Assert.AreEqual(JoinMethod.Clamped, first.JoinMethod, "the planner's choice");
        Assert.AreEqual(3, first.NumberOfClamps);
        Assert.AreEqual(2, first.HeightStep!.Value, Precision, "s2 is raised 2 mm");
        Assert.AreEqual(JointFit.Incompatible, second.Fit!.Fit);
        Assert.IsNull(second.JoinMethod);
        CollectionAssert.AreEquivalent(
            new[]
            {
                new LineProblem(LineProblemCode.HeightStepTooLarge, s1.Id, ExitPlace.Right),
                new LineProblem(LineProblemCode.HeightStepTooLarge, s2.Id, ExitPlace.Right),
                new LineProblem(LineProblemCode.JointDoesNotFit, s2.Id, ExitPlace.Right),
            },
            placement.Problems.ToList());
    }

    private static void AssertPose(Pose expected, Pose actual)
    {
        Assert.AreEqual(expected.Position.X, actual.Position.X, Precision);
        Assert.AreEqual(expected.Position.Y, actual.Position.Y, Precision);
        Assert.AreEqual(0, (actual.Heading - expected.Heading).Signed.Degrees, Precision);
    }

    private static HallPlan Plan()
    {
        var layout = new Layout();
        layout.Include(SingleTrack);
        layout.Include(DoubleTrack);
        var hall = new Hall { Id = Guid.NewGuid(), Name = "Hall", Outline = PathDrawing.Rectangle(20000, 10000, Transform.Identity) };
        return new HallPlan { Id = Guid.NewGuid(), Name = "Plan", Hall = hall, Layout = layout };
    }

    private static LineEntry Entry(LayoutModule module, bool isTurned = false) => new() { ModuleId = module.Id, IsTurned = isTurned };

    private static EndProfile Profile(string designation, params double[] offsets) => new()
    {
        Id = Guid.NewGuid(),
        Designation = designation,
        Scale = "H0",
        Tracks = [.. offsets.Select(o => new EndProfileTrack { Offset = o, Gauges = [16.5] })],
    };

    private static LayoutModule Straight(HallPlan plan, string name, EndProfile? profile = null) =>
        Module(plan, name, new PathBuilder(Pose.Origin).Straight(ModuleLength).Build(), profile ?? SingleTrack);

    private static LayoutModule Curved(HallPlan plan, string name, double angle) =>
        Module(plan, name, new PathBuilder(Pose.Origin).Curve(Radius, angle).Build(), SingleTrack);

    private static LayoutModule Module(HallPlan plan, string name, SegmentPath mainPath, EndProfile profile)
    {
        var reference = new CatalogueReference(profile.Id, profile.Designation);
        var module = new Module { Id = Guid.NewGuid(), Name = name };
        module.Drawing.MainPath = PathDrawing.From(mainPath);
        module.Drawing.Exits.Add(new() { Label = "L", Direction = ExitSide.Left, EndProfile = reference, Pose = mainPath.StartPose.Reversed() });
        module.Drawing.Exits.Add(new() { Label = "R", Direction = ExitSide.Right, EndProfile = reference, Pose = mainPath.EndPose });
        return plan.Layout.Place(module);
    }
}
