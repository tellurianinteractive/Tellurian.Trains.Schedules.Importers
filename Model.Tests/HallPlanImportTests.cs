using System.Text.Json;
using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Model.Tests;

// A hall plan Tellby – Ytterby – Stenungsund, in Norway, as Module Draw writes its layout block: a new plan is made
// from it, and a newer revision of it is loaded into that plan.
[TestClass]
public class HallPlanImportTests
{
    private static readonly Guid HallPlanId = Guid.Parse("30000000-0000-0000-0000-000000000003");

    private static Layout HallPlanLayout(int revision = 1, double secondDistance = 15, bool withYtterbyTrack2 = true)
    {
        var layout = new Layout
        {
            Name = "Club meeting",
            SourceId = HallPlanId,
            SourceRevision = revision,
            PreferredStandard = new ModuleStandard { ShortName = "FREMO H0", Scale = new Scale { Id = 4, ShortName = "H0" } },
        };
        var tellby = new Station(1, "Tellby", "Tby") { CountryCode = "NO", HasPassengerExchange = true, Layout = layout };
        tellby.Tracks.Add(new StationTrack(11, "1") { Station = tellby, IsMain = true });
        tellby.Tracks.Add(new StationTrack(12, "2") { Station = tellby });
        var ytterby = new Station(2, "Ytterby", "Yb") { CountryCode = "NO", Layout = layout };
        ytterby.Tracks.Add(new StationTrack(21, "1") { Station = ytterby, IsMain = true });
        if (withYtterbyTrack2) ytterby.Tracks.Add(new StationTrack(22, "2") { Station = ytterby });
        var stenungsund = new Station(3, "Stenungsund", "Snu") { CountryCode = "SE", Layout = layout };
        stenungsund.Tracks.Add(new StationTrack(31, "1") { Station = stenungsund, IsMain = true });
        layout.OperationLocations.Add(tellby);
        layout.OperationLocations.Add(ytterby);
        layout.OperationLocations.Add(stenungsund);
        layout.TrackStretches.Add(new TrackStretch(1, tellby, ytterby, 10) { Speed = 80, Layout = layout });
        layout.TrackStretches.Add(new TrackStretch(2, ytterby, stenungsund, secondDistance) { Speed = 80, Layout = layout });
        layout.Modules.Add(new LayoutModule { Id = 1, Module = new Module { Name = "Tellby", OperationLocation = tellby, Tracks = tellby.Tracks } });
        layout.Modules.Add(new LayoutModule { Id = 2, Module = new Module { Name = "Curve", Tracks = { new StationTrack(41, "1") } } });
        layout.Lines.Add(new Line { Id = 1, Entries = [new() { ModuleId = 1 }, new() { ModuleId = 2, IsTurned = true }] });
        layout.Joints.Add(new Joint { First = new(1, ExitSide.Right), Second = new(2, ExitSide.Right), NumberOfClamps = 4 });
        layout.FitTolerances = new() { Joint = 1 };
        return layout;
    }

    private static readonly Guid LinedHallPlanId = Guid.Parse("30000000-0000-0000-0000-000000000004");

    /// <summary>
    /// A hall plan whose stretch is worked out from its line: Tellby, a plain-line module and Ytterby, the stations
    /// 2000 mm long, no stretches of its own.
    /// </summary>
    private static Layout LinedHallPlan(int revision = 1, int speedLimit = 70, double plainLength = 1000)
    {
        var layout = new Layout { Name = "Lined", SourceId = LinedHallPlanId, SourceRevision = revision };
        var tellby = layout.Place(Drawn(new Module { Name = "Tellby", OperationLocation = new Station(0, "Tellby", "Tby") }, 2000));
        var plain = layout.Place(Drawn(new Module { Name = "Plain", SpeedLimit = speedLimit }, plainLength));
        var ytterby = layout.Place(Drawn(new Module { Name = "Ytterby", OperationLocation = new Station(0, "Ytterby", "Yb") }, 2000));
        layout.Add(new Line { Entries = [new() { ModuleId = tellby.Id }, new() { ModuleId = plain.Id }, new() { ModuleId = ytterby.Id }] });
        return layout;
    }

    private static Module Drawn(Module module, double length)
    {
        module.Drawing.MainPath = Model.Layouts.Drawings.PathDrawing.Polyline([new(0, 0), new(length, 0)]);
        return module;
    }

    /// <summary>The layout as it comes out of a hall plan file: written as Module Draw does and read back.</summary>
    private static Layout ReadFromFile(Layout layout)
    {
        var file = new JsonObject
        {
            ["format"] = HallPlanFile.Format,
            ["kind"] = HallPlanFile.Kind, ["schemaVersion"] = 2,
            ["content"] = new JsonObject { ["layout"] = JsonNode.Parse(JsonSerializer.Serialize(layout, Layout.JsonSerializerOptions)) },
        };
        return HallPlanFile.ReadLayout(file.ToJsonString());
    }

    [TestMethod]
    public void ANewPlanTakesTheLocationsStretchesAndModulesOfTheHallPlan()
    {
        var plan = PlanFactory.CreatePlan(ReadFromFile(HallPlanLayout()), "en");

        Assert.AreEqual("Club meeting", plan.Name);
        var layout = plan.Layout;
        CollectionAssert.AreEqual(new[] { "Tby", "Yb", "Snu" }, layout.OperationLocations.Select(l => l.Signature).ToArray());
        Assert.IsTrue(layout.OperationLocations.All(l => ReferenceEquals(l.Layout, layout)));
        Assert.IsTrue(layout.OperationLocations.SelectMany(l => l.Tracks.Select(t => (l, t))).All(p => ReferenceEquals(p.t.Station, p.l)));
        Assert.HasCount(2, layout.TrackStretches);
        Assert.AreSame(layout.OperationLocations.First(), layout.TrackStretches.First().Start);
        Assert.HasCount(2, layout.Modules);
        Assert.AreSame(layout.OperationLocations.First().Tracks, layout.Modules.First().Module.Tracks);
        Assert.AreEqual(HallPlanId, layout.SourceId);
        Assert.AreEqual(1, layout.SourceRevision);
        Assert.AreEqual(4, layout.Settings.Identity.ScaleId);
    }

    [TestMethod]
    public void EachStretchGetsARunningTimeFromItsDistanceAndSpeed()
    {
        var plan = PlanFactory.CreatePlan(HallPlanLayout(), "en");

        foreach (var stretch in plan.Layout.TrackStretches)
        {
            Assert.IsFalse(stretch.IsTimeEntered);
            Assert.AreEqual(TrackStretchTiming.RunningMinutes(stretch, plan.Layout.Settings.TimeAndSpeed), stretch.Time);
        }
        Assert.IsGreaterThan(plan.Layout.TrackStretches.First().Time, plan.Layout.TrackStretches.Last().Time, "the longer stretch takes longer");
    }

    [TestMethod]
    public void ANewPlanWorksOutItsTrackStretchesFromTheLines()
    {
        var plan = PlanFactory.CreatePlan(ReadFromFile(LinedHallPlan()), "en");

        var stretch = plan.Layout.TrackStretches.Single();
        Assert.AreEqual("Tellby–Ytterby", stretch.ToString());
        Assert.AreEqual(3.0, stretch.Distance, 0.0005, "1 m from Tellby's middle, 1 m of plain line, 1 m to Ytterby's middle");
        Assert.AreEqual(70, stretch.Speed, "the only module with a speed limit");
        Assert.AreEqual(TrackStretchTiming.RunningMinutes(stretch, plan.Layout.Settings.TimeAndSpeed), stretch.Time);
        Assert.HasCount(1, stretch.ModuleIds);
        Assert.IsFalse(stretch.IsDistanceEntered || stretch.IsSpeedEntered || stretch.IsTimeEntered);
    }

    [TestMethod]
    public void AReloadTakesTheNewDistanceAndKeepsAnEnteredSpeed()
    {
        var plan = PlanFactory.CreatePlan(ReadFromFile(LinedHallPlan()), "en");
        var stretch = plan.Layout.TrackStretches.Single();
        stretch.Speed = 50;
        stretch.IsSpeedEntered = true;

        plan.UpdateFromHallPlan(ReadFromFile(LinedHallPlan(revision: 2, speedLimit: 90, plainLength: 2000)));

        Assert.AreEqual(4.0, stretch.Distance, 0.0005);
        Assert.AreEqual(50, stretch.Speed, "the planner's speed");
        Assert.AreEqual(TrackStretchTiming.RunningMinutes(stretch, plan.Layout.Settings.TimeAndSpeed), stretch.Time);
    }

    [TestMethod]
    public void ANewPlanKeepsWhatWasMeasuredAndTheOrderAlongTheLineThroughThePlanFile()
    {
        var plan = PlanFactory.CreatePlan(ReadFromFile(LinedHallPlan()), "en");

        var read = PlanFile.Read(PlanFile.Write(plan));

        var stretch = read.Layout.TrackStretches.Single();
        Assert.AreEqual(3.0, stretch.MeasuredDistance!.Value, 0.0005);
        Assert.AreEqual(70, stretch.MeasuredSpeed);
        var leg = read.Layout.StretchLines.Single().Legs.Single();
        Assert.IsTrue(leg.Joins(stretch.StartId, stretch.EndId));
        Assert.IsTrue(read.Layout.CanSuggestTimetableStretches);
    }

    [TestMethod]
    public void AReloadTakesWhatWasMeasuredAlsoWhenTheValuesInUseWereEntered()
    {
        var plan = PlanFactory.CreatePlan(ReadFromFile(LinedHallPlan()), "en");
        var stretch = plan.Layout.TrackStretches.Single();
        stretch.Distance = 12;
        stretch.IsDistanceEntered = true;
        stretch.Speed = 50;
        stretch.IsSpeedEntered = true;

        plan.UpdateFromHallPlan(ReadFromFile(LinedHallPlan(revision: 2, speedLimit: 90, plainLength: 2000)));

        Assert.AreEqual(12, stretch.Distance, 0.0005, "the planner's distance");
        Assert.AreEqual(4.0, stretch.MeasuredDistance!.Value, 0.0005);
        Assert.AreEqual(90, stretch.MeasuredSpeed);
        Assert.IsTrue(plan.Layout.StretchLines.Single().Legs.Single().Joins(stretch.StartId, stretch.EndId));
    }

    [TestMethod]
    public void AReloadTakesTheModulesSpeedWhenNoneWasEntered()
    {
        var plan = PlanFactory.CreatePlan(ReadFromFile(LinedHallPlan()), "en");

        plan.UpdateFromHallPlan(ReadFromFile(LinedHallPlan(revision: 2, speedLimit: 90)));

        Assert.AreEqual(90, plan.Layout.TrackStretches.Single().Speed);
    }

    [TestMethod]
    public void TheDefaultCountryIsTheOneMostLocationsAreIn()
    {
        var plan = PlanFactory.CreatePlan(HallPlanLayout(), "sv");

        var norway = Country.ByCountryCode("NO")!;
        var sweden = Country.ByCountryCode("SE")!;
        Assert.AreEqual(norway.Id, plan.Layout.Settings.Identity.DefaultCountryId);
        Assert.IsTrue(plan.Layout.Countries.Any(c => c.Id == norway.Id));
        Assert.IsTrue(plan.Layout.Countries.Any(c => c.Id == sweden.Id), "every country a location is in is in the catalogue");
    }

    [TestMethod]
    public void WithoutCountriesTheLanguageGivesTheDefaultCountryAndTheNameIsGiven()
    {
        var hallPlan = HallPlanLayout();
        hallPlan.Name = string.Empty;
        foreach (var location in hallPlan.OperationLocations) location.CountryCode = null;

        var plan = PlanFactory.CreatePlan(hallPlan, "da", "Ny plan");

        Assert.AreEqual(Country.ByLanguage("da").Id, plan.Layout.Settings.Identity.DefaultCountryId);
        Assert.AreEqual("Ny plan", plan.Name);
    }

    [TestMethod]
    public void ANewPlanFromAHallPlanSurvivesThePlanFile()
    {
        var plan = PlanFactory.CreatePlan(ReadFromFile(HallPlanLayout()), "en");

        var read = PlanFile.Read(PlanFile.Write(plan));

        PlanFingerprint.AssertSame(PlanFingerprint.Of(plan), PlanFingerprint.Of(read));
        Assert.AreEqual(HallPlanId, read.Layout.SourceId);
        Assert.HasCount(2, read.Layout.Modules);
        Assert.AreSame(read.Layout.OperationLocations.First().Tracks, read.Layout.Modules.First().Module.Tracks);
    }

    [TestMethod]
    public void ANewPlanKeepsTheLinesAndJointsOfTheHallPlanThroughThePlanFile()
    {
        var plan = PlanFactory.CreatePlan(ReadFromFile(HallPlanLayout()), "en");

        var read = PlanFile.Read(PlanFile.Write(plan)).Layout;

        Assert.IsTrue(read.Lines.Single().Entries[1].IsTurned);
        Assert.AreEqual(4, read.Joints.Single().NumberOfClamps);
        Assert.AreEqual(1, read.EffectiveFitTolerances.Joint);
    }

    [TestMethod]
    public void ANewerRevisionTakesTheLinesAndJoints()
    {
        var plan = PlanFactory.CreatePlan(HallPlanLayout(), "en");
        var newer = HallPlanLayout(revision: 2);
        newer.Lines.Single().Entries[1].IsTurned = false;
        newer.Joints.Clear();

        plan.UpdateFromHallPlan(ReadFromFile(newer));

        Assert.IsFalse(plan.Layout.Lines.Single().Entries[1].IsTurned);
        Assert.IsEmpty(plan.Layout.Joints);
    }

    [TestMethod]
    public void AnUnchangedHallPlanChangesNothing()
    {
        var plan = PlanFactory.CreatePlan(HallPlanLayout(), "en");

        var update = plan.UpdateFromHallPlan(HallPlanLayout());

        Assert.IsTrue(update.IsUnchanged, string.Join("; ", update.Comparison.Changes.Select(c => $"{c.Kind} {c.Location?.Signature} {c.Track?.Designation} [{string.Join(',', c.Properties)}]")));
        Assert.IsEmpty(update.Applied);
    }

    [TestMethod]
    public void ANewerRevisionTakesTheDistancesAndLeavesTheRestToConfirm()
    {
        var plan = PlanFactory.CreatePlan(HallPlanLayout(), "en");
        var stretch = plan.Layout.TrackStretches.Last();
        var timeBefore = stretch.Time;

        var update = plan.UpdateFromHallPlan(ReadFromFile(HallPlanLayout(revision: 2, secondDistance: 30, withYtterbyTrack2: false)));

        Assert.AreEqual(30, stretch.Distance, 0.0005);
        Assert.IsGreaterThan(timeBefore, stretch.Time);
        Assert.AreEqual(2, plan.Layout.SourceRevision);
        Assert.IsTrue(update.Applied.Any(c => c.Kind == LayoutChangeKind.StretchChanged));
        Assert.AreEqual(LayoutChangeKind.TrackRemoved, update.ToConfirm.Single().Kind);
        Assert.IsEmpty(update.Manual);
        Assert.HasCount(2, plan.Layout.OperationLocations.Single(l => l.Signature == "Yb").Tracks, "a removed track waits for the planner");
    }

    [TestMethod]
    public void AfterAConfirmedChangeComparingAgainLeavesOnlyWhatIsLeft()
    {
        var plan = PlanFactory.CreatePlan(HallPlanLayout(), "en");
        var newer = HallPlanLayout(revision: 2, withYtterbyTrack2: false);
        var update = plan.UpdateFromHallPlan(newer);

        Assert.IsTrue(plan.ApplyChange(update.ToConfirm.Single()).IsAllowed);
        var again = plan.UpdateFromHallPlan(newer);

        Assert.IsTrue(again.IsUnchanged, "the newer layout is only read, so it can be compared again");
        Assert.HasCount(1, newer.OperationLocations.Single(l => l.Signature == "Yb").Tracks, "the newer layout is left as it was");
    }
}
