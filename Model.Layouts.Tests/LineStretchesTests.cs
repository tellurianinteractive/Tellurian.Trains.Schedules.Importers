using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;
using static Tellurian.Trains.Schedules.Model.Layouts.Tests.LayoutLinesTests;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// Stretches worked out from the lines (<c>LayoutsLayerSurvey.md</c> 3.3 step 1 and 3.6, <c>ModelHarmonisation.md</c>
/// decision 15): which modules, sides, tracks and main-path distance; the update of the track stretches; the
/// suggested timetable stretches.
/// </summary>
[TestClass]
public sealed class LineStretchesTests
{
    private static readonly EndProfile SingleTrack = Profile("Single", 0);
    private static readonly EndProfile DoubleTrack = Profile("Double", -23, 23);

    [TestMethod]
    public void AMainLineGivesAStretchBetweenEachPairOfNeighbouringLocations()
    {
        var layout = new Layout();
        var (a, p1, p2, b, q, c) = (Place(layout, Station("Aby")), Place(layout, Plain("p1")), Place(layout, Plain("p2")), Place(layout, Station("Bby")), Place(layout, Plain("q")), Place(layout, Station("Cby")));
        layout.Add(new Line { Entries = [Entry(a), Entry(p1), Entry(p2), Entry(b), Entry(q), Entry(c)] });

        var result = layout.StretchesFromLines();

        Assert.IsEmpty(result.Problems);
        Assert.HasCount(2, result.Stretches);
        var (first, second) = (result.Stretches[0], result.Stretches[1]);
        Assert.AreEqual("Aby–Bby", first.ToString());
        CollectionAssert.AreEqual(new[] { p1.Id, p2.Id }, first.ModuleIds.ToArray());
        Assert.AreEqual(ExitSide.Right, first.StartSide);
        Assert.AreEqual(ExitSide.Left, first.EndSide);
        Assert.AreEqual("Bby–Cby", second.ToString());
        CollectionAssert.AreEqual(new[] { q.Id }, second.ModuleIds.ToArray());
        Assert.IsNull(first.TracksCount, "no end profiles without a drawing");
        Assert.IsNull(first.MainPathDistance, "no main paths without a drawing");
    }

    [TestMethod]
    public void ATurnedLocationIsLeftFromItsOwnSide()
    {
        var layout = new Layout();
        var (a, p, b) = (Place(layout, Station("Aby")), Place(layout, Plain("p")), Place(layout, Station("Bby")));
        layout.Add(new Line { Entries = [Entry(a, isTurned: true), Entry(p, isTurned: true), Entry(b, isTurned: true)] });

        var stretch = layout.StretchesFromLines().Stretches.Single();

        Assert.AreEqual(ExitSide.Left, stretch.StartSide);
        Assert.AreEqual(ExitSide.Right, stretch.EndSide);
        CollectionAssert.AreEqual(new[] { p.Id }, stretch.ModuleIds.ToArray());
    }

    [TestMethod]
    public void PlainLineToAFreeEndIsReportedAndGivesNoStretch()
    {
        var layout = new Layout();
        var (a, p1, p2) = (Place(layout, Station("Aby")), Place(layout, Plain("p1")), Place(layout, Plain("p2")));
        layout.Add(new Line { Entries = [Entry(a), Entry(p1), Entry(p2)] });

        var result = layout.StretchesFromLines();

        Assert.IsEmpty(result.Stretches);
        Assert.AreEqual(new LineProblem(LineProblemCode.LineEndsWithoutLocation, p2.Id), result.Problems.Single());
    }

    [TestMethod]
    public void TheTracksAreTheFewestAtAnyJointAndTheDistanceRunsAlongTheMainPathsFromCentreToCentre()
    {
        var layout = new Layout();
        layout.Include(SingleTrack);
        layout.Include(DoubleTrack);
        var a = Place(layout, Drawn(Station("Aby"), 2000, DoubleTrack));
        var p = Place(layout, Drawn(Plain("p"), 1000, SingleTrack));
        var b = Place(layout, Drawn(Station("Bby"), 3000, DoubleTrack));
        b.Module.OperationLocation!.Centre = 1.0;
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b)] });

        var stretch = layout.StretchesFromLines().Stretches.Single();

        Assert.AreEqual(1, stretch.TracksCount);
        Assert.AreEqual(3.0, stretch.MainPathDistance!.Value, Precision, "1 m from Aby's middle, 1 m of plain line, 1 m to Bby's centre");
    }

    [TestMethod]
    [DataRow(80, 120, 60, 87, DisplayName = "each module weighted by its length along the stretch: (80 + 120 + 60) / 3, rounded")]
    [DataRow(80, 120, null, 100, DisplayName = "a module without a limit doesn't count")]
    [DataRow(null, null, null, null, DisplayName = "no limits, no speed")]
    public void TheSpeedIsTheModulesLimitsWeightedByTheirLengthsAlongTheStretch(int? atA, int? onP, int? atB, int? expected)
    {
        var layout = new Layout();
        layout.Include(SingleTrack);
        var a = Place(layout, Limited(Drawn(Station("Aby"), 2000, SingleTrack), atA));
        var p = Place(layout, Limited(Drawn(Plain("p"), 1000, SingleTrack), onP));
        var b = Place(layout, Limited(Drawn(Station("Bby"), 2000, SingleTrack), atB));
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b)] });

        Assert.AreEqual(expected, layout.StretchesFromLines().Stretches.Single().Speed, "1 m of each module: half of each station, all of the plain line");
    }

    [TestMethod]
    public void WhereLengthsArentKnownTheLimitsCountEqually()
    {
        var layout = new Layout();
        var a = Place(layout, Station("Aby"));
        var p = Place(layout, Limited(Plain("p"), 80));
        var q = Place(layout, Limited(Plain("q"), 100));
        var b = Place(layout, Station("Bby"));
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(q), Entry(b)] });

        Assert.AreEqual(90, layout.StretchesFromLines().Stretches.Single().Speed);
    }

    [TestMethod]
    public void AnAddedTrackStretchGetsTheSpeedOfItsModules()
    {
        var layout = new Layout();
        var a = Place(layout, Station("Aby"));
        var p = Place(layout, Limited(Plain("p"), 70));
        var b = Place(layout, Station("Bby"));
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b)] });

        layout.UpdateTrackStretchesFromLines();

        Assert.AreEqual(70, layout.TrackStretches.Single().Speed);
    }

    [TestMethod]
    public void ABranchStartsAtTheLocationItLeavesFrom()
    {
        var layout = new Layout();
        var a = Place(layout, Station("Aby"));
        var p = Place(layout, Plain("p"));
        var j = Place(layout, Drawn(Station("Jby"), 1000, SingleTrack, ExitSide.Up));
        var q = Place(layout, Plain("q"));
        var b = Place(layout, Station("Bby"));
        var r = Place(layout, Plain("r"));
        var c = Place(layout, Station("Cby"));
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(j), Entry(q), Entry(b)] });
        var branch = layout.Add(new Line { StartAt = new(j.Id, ExitSide.Up), Entries = [Entry(r), Entry(c)] });

        var stretches = layout.StretchesFromLines().Stretches;

        CollectionAssert.AreEqual(new[] { "Aby–Jby", "Jby–Bby", "Jby–Cby" }, stretches.Select(s => s.ToString()).ToArray());
        var toC = stretches[2];
        Assert.AreEqual(ExitSide.Up, toC.StartSide);
        Assert.AreEqual(branch.Id, toC.LineId);
        CollectionAssert.AreEqual(new[] { r.Id }, toC.ModuleIds.ToArray());
    }

    [TestMethod]
    public void AStretchFoundAgainstItsLineRunsWithTheLine()
    {
        var layout = new Layout();
        var a = Place(layout, Drawn(Station("Aby"), 1000, SingleTrack, ExitSide.Up));
        var x = Place(layout, Plain("x"));
        var b = Place(layout, Station("Bby"));
        var y = Place(layout, Plain("y"));
        var c = Place(layout, Drawn(Station("Cby"), 1000, SingleTrack, ExitSide.Up));
        var r = Place(layout, Plain("r"));
        layout.Add(new Line { Entries = [Entry(a), Entry(x), Entry(b), Entry(y), Entry(c)] });
        var loop = layout.Add(new Line { StartAt = new(c.Id, ExitSide.Up), Entries = [Entry(r)], EndAt = new(a.Id, ExitSide.Up) });

        var stretches = layout.StretchesFromLines().Stretches;

        CollectionAssert.AreEqual(new[] { "Aby–Bby", "Cby–Aby", "Bby–Cby" }, stretches.Select(s => s.ToString()).ToArray(), "Cby–Aby is found from Aby, against the loop");
        var back = stretches[1];
        Assert.AreEqual(loop.Id, back.LineId);
        Assert.AreEqual(ExitSide.Up, back.StartSide);
        Assert.AreEqual(ExitSide.Up, back.EndSide);
        CollectionAssert.AreEqual(new[] { r.Id }, back.ModuleIds.ToArray());
    }

    [TestMethod]
    public void TwoWaysBetweenTheSameLocationsGiveOneStretchAndAProblem()
    {
        var layout = new Layout();
        layout.Include(SingleTrack);
        var a = Place(layout, Drawn(Station("Aby"), 1000, SingleTrack, ExitSide.Up));
        var x = Place(layout, Plain("x"));
        var b = Place(layout, Drawn(Station("Bby"), 1000, SingleTrack, ExitSide.Up));
        var r = Place(layout, Plain("r"));
        layout.Add(new Line { Entries = [Entry(a), Entry(x), Entry(b)] });
        layout.Add(new Line { StartAt = new(a.Id, ExitSide.Up), Entries = [Entry(r)], EndAt = new(b.Id, ExitSide.Up) });

        var result = layout.StretchesFromLines();

        CollectionAssert.AreEqual(new[] { x.Id }, result.Stretches.Single().ModuleIds.ToArray());
        CollectionAssert.Contains(result.Problems.Select(p => p.Code).ToList(), LineProblemCode.StretchesInParallel);
    }

    [TestMethod]
    public void MissingModulesAndExitsAreReported()
    {
        var layout = new Layout();
        var a = Place(layout, Station("Aby"));
        var b = Place(layout, Station("Bby"));
        layout.Add(new Line { Entries = [Entry(a), new LineEntry { ModuleId = 99 }] });
        layout.Add(new Line { Entries = [new LineEntry { ModuleId = b.Id, ContinuesFrom = new(ExitSide.Right, 2) }, Entry(a, isTurned: true)] });

        var problems = layout.StretchesFromLines().Problems;

        CollectionAssert.Contains(problems.ToList(), new LineProblem(LineProblemCode.ModuleMissing, 99));
        CollectionAssert.Contains(problems.ToList(), new LineProblem(LineProblemCode.ExitMissing, b.Id, new(ExitSide.Right, 2)));
    }

    [TestMethod]
    public void TheTrackStretchesAreUpdatedKeepingWhatThePlannerEntered()
    {
        var layout = new Layout();
        layout.Include(SingleTrack);
        var a = Place(layout, Drawn(Station("Aby"), 2000, SingleTrack));
        var p = Place(layout, Drawn(Plain("p"), 1000, SingleTrack));
        var b = Place(layout, Drawn(Station("Bby"), 2000, SingleTrack));
        var q = Place(layout, Drawn(Plain("q"), 1000, SingleTrack));
        var c = Place(layout, Drawn(Station("Cby"), 2000, SingleTrack));
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b), Entry(q), Entry(c)] });
        var entered = new TrackStretch(5, Location(c), Location(b), 12, 2) { IsDistanceEntered = true, IsTimeEntered = true, Time = 7, Speed = 80, IsSpeedEntered = true };
        layout.TrackStretches.Add(entered);

        layout.UpdateTrackStretchesFromLines();

        Assert.HasCount(2, layout.TrackStretches);
        Assert.AreSame(Location(c), entered.Start, "keeps its direction");
        Assert.AreEqual(ExitSide.Left, entered.StartSide);
        Assert.AreEqual(ExitSide.Right, entered.EndSide);
        CollectionAssert.AreEqual(new[] { q.Id }, entered.ModuleIds.ToArray());
        Assert.AreEqual(1, entered.TracksCount);
        Assert.AreEqual(12, entered.Distance, Precision);
        Assert.AreEqual(7, entered.Time);
        var added = layout.TrackStretches.Single(s => s != entered);
        Assert.AreEqual(6, added.Id);
        Assert.IsFalse(added.HasSpeed, "no module has a speed limit");
        Assert.AreEqual(TrackStretchFacts.AssumedSpeed, added.LimitingSpeed);
        Assert.AreEqual(80, entered.Speed, "an entered speed is kept");
        Assert.AreSame(Location(a), added.Start);
        Assert.AreEqual(3.0, added.Distance, Precision);
        CollectionAssert.AreEqual(new[] { p.Id }, added.ModuleIds.ToArray());
    }

    [TestMethod]
    public void ASpeedNotEnteredFollowsTheModules()
    {
        var layout = new Layout();
        var a = Place(layout, Station("Aby"));
        var p = Place(layout, Limited(Plain("p"), 60));
        var b = Place(layout, Station("Bby"));
        layout.Add(new Line { Entries = [Entry(a), Entry(p), Entry(b)] });
        var stretch = new TrackStretch(1, Location(a), Location(b), 10) { Speed = 80 };
        layout.TrackStretches.Add(stretch);

        layout.UpdateTrackStretchesFromLines();

        Assert.AreEqual(60, stretch.Speed);
    }

    [TestMethod]
    public void TimetableStretchesAreSuggestedPerLineAndNotAdded()
    {
        var layout = new Layout();
        var a = Place(layout, Station("Aby"));
        var p = Place(layout, Plain("p"));
        var b = Place(layout, Drawn(Station("Bby"), 1000, SingleTrack, ExitSide.Down));
        var q = Place(layout, Plain("q"));
        var c = Place(layout, Station("Cby"));
        var r = Place(layout, Plain("r"));
        var d = Place(layout, Station("Dby"));
        layout.Add(new Line { Name = "Main", Entries = [Entry(a), Entry(p), Entry(b), Entry(q), Entry(c)] });
        layout.Add(new Line { Name = "Branch", StartAt = new(b.Id, ExitSide.Down), Entries = [Entry(r), Entry(d)] });
        layout.TimetableStretches.Add(new TimetableStretch(4, "3"));
        layout.UpdateTrackStretchesFromLines();

        var suggestions = layout.SuggestedTimetableStretches();

        Assert.HasCount(1, layout.TimetableStretches, "suggestions are not added");
        Assert.HasCount(2, suggestions);
        Assert.AreEqual(5, suggestions[0].Id);
        Assert.AreEqual("4", suggestions[0].Number);
        Assert.AreEqual("Main", suggestions[0].Description);
        CollectionAssert.AreEqual(new[] { "Aby", "Bby", "Cby" }, suggestions[0].Stations.Select(s => s.Name).ToArray());
        Assert.AreEqual("5", suggestions[1].Number);
        CollectionAssert.AreEqual(new[] { "Bby", "Dby" }, suggestions[1].Stations.Select(s => s.Name).ToArray());
    }

    private static LayoutModule Place(Layout layout, Module module) => layout.Place(module);

    private static Module Limited(Module module, int? speedLimit)
    {
        module.SpeedLimit = speedLimit;
        return module;
    }

    private static OperationLocation Location(LayoutModule module) => module.Module.OperationLocation!;

    private static LineEntry Entry(LayoutModule module, bool isTurned = false) => new() { ModuleId = module.Id, IsTurned = isTurned };

    private static EndProfile Profile(string designation, params double[] offsets) => new()
    {
        Id = Guid.NewGuid(),
        Designation = designation,
        Scale = "H0",
        Tracks = [.. offsets.Select(o => new EndProfileTrack { Offset = o, Gauges = [16.5] })],
    };

    /// <summary>
    /// <paramref name="module"/> with a straight main path of <paramref name="length"/> mm, an exit at each end and one
    /// at the middle for each of <paramref name="across"/>, all with <paramref name="profile"/>.
    /// </summary>
    private static Module Drawn(Module module, double length, EndProfile profile, params ExitSide[] across)
    {
        var reference = new CatalogueReference(profile.Id, profile.Designation);
        module.Drawing.MainPath = PathDrawing.Polyline([new(0, 0), new(length, 0)]);
        module.Drawing.Exits.Add(new() { Label = "L", Direction = ExitSide.Left, EndProfile = reference, Pose = new(new(0, 0), Angle.Half) });
        module.Drawing.Exits.Add(new() { Label = "R", Direction = ExitSide.Right, EndProfile = reference, Pose = new(new(length, 0), Angle.Zero) });
        foreach (var side in across)
            module.Drawing.Exits.Add(new() { Label = side.ToString(), Direction = side, EndProfile = reference, Pose = new(new(length / 2, 100), Angle.Quarter) });
        return module;
    }
}
