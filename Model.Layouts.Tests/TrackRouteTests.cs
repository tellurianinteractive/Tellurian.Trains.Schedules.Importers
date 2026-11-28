using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;
using Tellurian.Trains.Schedules.Model.Layouts.Tracks;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// The route along the drawn tracks of a stretch and its measured distance (<c>LayoutsLayerSurvey.md</c> 3.2–3.5): the
/// track graph across joints, the shortest way from centre to centre, and what stops a route.
/// </summary>
[TestClass]
public sealed class TrackRouteTests
{
    private const double Gap = 23;

    private static readonly EndProfile SingleTrack = Profile("Single", 0);
    private static readonly EndProfile DoubleTrack = Profile("Double", -Gap, Gap);

    [TestMethod]
    public void TheRouteRunsAlongTheTracksFromCentreToCentre()
    {
        var layout = Layout();
        var a = Straight(layout, Station("Aby"), 2000, SingleTrack);
        var p = Straight(layout, Plain("p"), 1000, SingleTrack);
        var b = Straight(layout, Station("Bby"), 2000, SingleTrack);
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b)] });

        var result = layout.StretchesFromLines();

        Assert.IsEmpty(result.Problems);
        var route = result.Stretches.Single().Route!;
        CollectionAssert.AreEqual(new[] { a.Id, p.Id, b.Id }, route.Pieces.Select(x => x.ModuleId).ToArray());
        CollectionAssert.AreEqual(new[] { 1000.0, 1000.0, 1000.0 }, route.Pieces.Select(x => x.Length).ToArray());
        Assert.AreEqual(3.0, route.Distance, Precision);
        Assert.IsNull(route.SmallestRadius);
        Assert.AreEqual(3.0, result.Stretches.Single().Distance!.Value, Precision);
    }

    [TestMethod]
    public void OnADoubleTrackCurveTheRouteTakesTheInnerTrackAndTheDistanceIsTheAverage()
    {
        const double radius = 2000;
        const double angle = 30;
        var layout = Layout();
        var a = Straight(layout, Limited(Station("Aby"), 100), 2000, DoubleTrack);
        var curve = Curved(layout, Limited(Plain("curve"), 50), radius, angle);
        var b = Straight(layout, Station("Bby"), 2000, DoubleTrack);
        layout.Add(new Line { Entries = [Entry(a), Entry(curve), Entry(b)] });

        var stretch = layout.StretchesFromLines().Stretches.Single();

        var inner = (radius - Gap) * angle * Math.PI / 180;
        var average = radius * angle * Math.PI / 180;
        Assert.AreEqual((2000 + inner) / 1000, stretch.Route!.Distance, Precision, "the shortest way along the tracks");
        Assert.AreEqual(radius - Gap, stretch.Route.SmallestRadius!.Value, Precision);
        Assert.AreEqual((2000 + average) / 1000, stretch.Distance!.Value, Precision, "the module's average length: its main path");
        Assert.AreEqual(74, stretch.Speed, "weighted by the average lengths: (100 × 1000 + 50 × average) / (1000 + average)");
    }

    [TestMethod]
    public void ARouteDoesntTurnBackAtATrackEnd()
    {
        var layout = Layout();
        var a = Straight(layout, Station("Aby"), 2000, SingleTrack);
        var kink = Kinked(layout, Plain("kink"));
        var b = Straight(layout, Station("Bby"), 2000, SingleTrack);
        layout.Add(new Line { Entries = [Entry(a), Entry(kink), Entry(b)] });

        var result = layout.StretchesFromLines();

        var stretch = result.Stretches.Single();
        Assert.IsNull(stretch.Route, "every way through the kink turns back");
        Assert.AreEqual(new LineProblem(LineProblemCode.NoTrackPath, a.Id), result.Problems.Single());
        Assert.AreEqual(3.0, stretch.Distance!.Value, Precision, "the main paths still give a distance");
    }

    [TestMethod]
    public void TracksThatDontPairUpAcrossAJointAreReported()
    {
        var layout = Layout();
        var a = Straight(layout, Station("Aby"), 2000, SingleTrack);
        var b = Straight(layout, Station("Bby"), 2000, DoubleTrack);
        layout.Add(new Line { Entries = [Entry(a), Entry(b)] });

        var problems = layout.StretchesFromLines().Problems;

        CollectionAssert.Contains(problems.ToList(), new LineProblem(LineProblemCode.TracksDontMeetAtJoint, a.Id, ExitPlace.Right));
        CollectionAssert.Contains(problems.ToList(), new LineProblem(LineProblemCode.NoTrackPath, a.Id));
    }

    [TestMethod]
    public void ATrackThatStopsShortOfAnExitIsReported()
    {
        var layout = Layout();
        var a = Straight(layout, Station("Aby"), 2000, SingleTrack);
        var p = Straight(layout, Plain("p"), 1000, SingleTrack, trackLength: 900);
        var b = Straight(layout, Station("Bby"), 2000, SingleTrack);
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b)] });

        var problems = layout.StretchesFromLines().Problems;

        CollectionAssert.Contains(problems.ToList(), new LineProblem(LineProblemCode.TrackMissingAtExit, p.Id, ExitPlace.Right));
    }

    [TestMethod]
    public void ARouteRunsThroughATurnoutIntoItsPoints()
    {
        var layout = Layout();
        var a = Straight(layout, Station("Aby"), 2000, SingleTrack);
        var p = WithTurnout(layout, Plain("p"), throughThePoints: true);
        var b = Straight(layout, Station("Bby"), 2000, SingleTrack);
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b)] });

        var result = layout.StretchesFromLines();

        Assert.IsEmpty(result.Problems);
        var route = result.Stretches.Single().Route!;
        Assert.AreEqual(3.0, route.Distance, Precision);
        var turnout = route.Pieces.Single(x => x.ModuleId == p.Id && x.TrackElementId == TurnoutId);
        Assert.AreEqual(1, turnout.RouteNumber, "the straight route");
        Assert.AreEqual(TurnoutLength, turnout.Length, Precision);
    }

    [TestMethod]
    public void ARouteDoesntReverseAtATurnoutsPoints()
    {
        var layout = Layout();
        var a = Straight(layout, Station("Aby"), 2000, SingleTrack);
        var p = WithTurnout(layout, Plain("p"), throughThePoints: false);
        var b = Straight(layout, Station("Bby"), 2000, SingleTrack);
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b)] });

        var result = layout.StretchesFromLines();

        Assert.IsNull(result.Stretches.Single().Route, "the tracks are connected, but only by reversing at the points");
        Assert.AreEqual(new LineProblem(LineProblemCode.NoTrackPath, a.Id), result.Problems.Single());
    }

    [TestMethod]
    public void AStretchKeepsTheMeasuredDistanceBesideTheEnteredOne()
    {
        var layout = Layout();
        var a = Straight(layout, Station("Aby"), 2000, SingleTrack);
        var p = Straight(layout, Plain("p"), 1000, SingleTrack);
        var b = Straight(layout, Station("Bby"), 2000, SingleTrack);
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b)] });
        layout.UpdateTrackStretchesFromLines();
        var stretch = layout.Layout.TrackStretches.Single();
        stretch.Distance = 12;
        stretch.IsDistanceEntered = true;

        layout.UpdateTrackStretchesFromLines();

        Assert.AreEqual(12, stretch.Distance, Precision);
        Assert.AreEqual(3.0, stretch.MeasuredDistance!.Value, Precision);
    }

    private static DrawnLayout Layout()
    {
        var layout = new DrawnLayout();
        layout.Include(SingleTrack);
        layout.Include(DoubleTrack);
        return layout;
    }

    private static Module Station(string name)
    {
        var location = new Station(0, name, name[..3]);
        return new Module { Id = Guid.NewGuid(), Name = name, OperationLocation = location };
    }

    private static Module Plain(string name) => new() { Id = Guid.NewGuid(), Name = name };

    private static Module Limited(Module module, int speedLimit)
    {
        module.SpeedLimit = speedLimit;
        return module;
    }

    private static LineEntry Entry(PlacedModule module) => new() { ModuleId = module.Id };

    private static EndProfile Profile(string designation, params double[] offsets) => new()
    {
        Id = Guid.NewGuid(),
        Designation = designation,
        Scale = "H0",
        Tracks = [.. offsets.Select(o => new EndProfileTrack { Offset = o, Gauges = [16.5] })],
    };

    /// <summary>
    /// A straight module of <paramref name="length"/> along X with an exit at each end and a straight track for each
    /// track of <paramref name="profile"/>: at the right exit, offset <i>y</i> is at +<i>y</i>.
    /// </summary>
    private static PlacedModule Straight(DrawnLayout layout, Module module, double length, EndProfile profile, double? trackLength = null)
    {
        var path = new PathBuilder(Pose.Origin).Straight(length).Build();
        Exits(module, path, profile);
        foreach (var track in profile.Tracks)
            module.Drawing.TrackElements.Add(new StraightTrack { Start = new(new(0, track.Offset), Angle.Zero), Length = trackLength ?? length });
        return Placed(layout, module);
    }

    /// <summary>A double-track module curving left: the main path at <paramref name="radius"/>, the tracks <see cref="Gap"/> inside and outside.</summary>
    private static PlacedModule Curved(DrawnLayout layout, Module module, double radius, double angle)
    {
        var path = new PathBuilder(Pose.Origin).Curve(radius, angle).Build();
        Exits(module, path, DoubleTrack);
        foreach (var offset in new[] { Gap, -Gap })
            module.Drawing.TrackElements.Add(new CurvedTrack { Start = new(new(0, offset), Angle.Zero), Radius = radius - offset, Angle = angle });
        return Placed(layout, module);
    }

    /// <summary>
    /// A single-track module of 1000 mm whose track turns back on itself twice: from the left exit to the middle, back
    /// up to the left, and across to the right exit. Every way through it reverses at a track end.
    /// </summary>
    private const int TurnoutId = 3;
    private const double TurnoutLength = 300;

    /// <summary>
    /// A single-track module of 1000 mm with a left turnout whose points are at x = 800, its routes running back towards
    /// the left exit: the left exit's track meets the end of the straight route. With <paramref name="throughThePoints"/>,
    /// a track runs on from the points to the right exit. Without it, the only way to the right exit is to reverse at the
    /// points onto the diverging route, which curves round and leads there.
    /// </summary>
    private static PlacedModule WithTurnout(DrawnLayout layout, Module module, bool throughThePoints)
    {
        const double length = 1000;
        const double points = 800;
        var turnout = new Turnout { Id = TurnoutId, Start = new(new(points, 0), Angle.Half), Kind = TurnoutKind.Left, Length = TurnoutLength, Radius = 600, Angle = 12 };
        var elements = new List<TrackElement>
        {
            new StraightTrack { Id = 1, Start = Pose.Origin, Length = 200 },
            new StraightTrack { Id = 2, Start = new(new(200, 0), Angle.Zero), Length = points - TurnoutLength - 200 },
            turnout,
        };
        Pose rightExit;
        if (throughThePoints)
        {
            elements.Add(new StraightTrack { Id = 4, Start = new(new(points, 0), Angle.Zero), Length = length - points });
            rightExit = new(new(length, 0), Angle.Zero);
        }
        else
        {
            var round = new CurvedTrack { Id = 4, Start = turnout.Routes()[1].EndPose, Radius = 150, Angle = 168 };
            var turned = round.Routes()[0].EndPose;
            elements.Add(round);
            elements.Add(new StraightTrack { Id = 5, Start = turned, Length = length - turned.Position.X });
            rightExit = new(new(length, turned.Position.Y), Angle.Zero);
        }
        var reference = new CatalogueReference(SingleTrack.Id, SingleTrack.Designation);
        module.Drawing.MainPath = PathDrawing.From(new PathBuilder(Pose.Origin).Straight(length).Build());
        module.Drawing.Exits.Add(new() { Label = "L", Direction = ExitSide.Left, EndProfile = reference, Pose = new(Point.Origin, Angle.Half) });
        module.Drawing.Exits.Add(new() { Label = "R", Direction = ExitSide.Right, EndProfile = reference, Pose = rightExit });
        module.Drawing.TrackElements.AddRange(elements);
        return layout.Place(module);
    }

    private static PlacedModule Kinked(DrawnLayout layout, Module module)
    {
        const double length = 1000;
        var path = new PathBuilder(Pose.Origin).Straight(length).Build();
        Exits(module, path, SingleTrack);
        var middle = new Point(length / 2, 0);
        var back = new Pose(middle, new Angle(150)).Advanced(400);
        var across = new Point(length, 0) - back.Position;
        module.Drawing.TrackElements.Add(new StraightTrack { Start = Pose.Origin, Length = length / 2 });
        module.Drawing.TrackElements.Add(new StraightTrack { Start = new(middle, new Angle(150)), Length = 400 });
        module.Drawing.TrackElements.Add(new StraightTrack { Start = new(back.Position, Angle.FromRadians(Math.Atan2(across.Y, across.X))), Length = across.Length });
        return Placed(layout, module);
    }

    private static void Exits(Module module, SegmentPath mainPath, EndProfile profile)
    {
        var reference = new CatalogueReference(profile.Id, profile.Designation);
        module.Drawing.MainPath = PathDrawing.From(mainPath);
        module.Drawing.Exits.Add(new() { Label = "L", Direction = ExitSide.Left, EndProfile = reference, Pose = mainPath.StartPose.Reversed() });
        module.Drawing.Exits.Add(new() { Label = "R", Direction = ExitSide.Right, EndProfile = reference, Pose = mainPath.EndPose });
    }

    private static PlacedModule Placed(DrawnLayout layout, Module module)
    {
        module.Drawing.NumberTrackElements();
        return layout.Place(module);
    }
}
