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
        return layout;
    }

    /// <summary>The layout as it comes out of a hall plan file: written as Module Draw does and read back.</summary>
    private static Layout ReadFromFile(Layout layout)
    {
        var file = new JsonObject
        {
            ["format"] = HallPlanFile.Format,
            ["kind"] = HallPlanFile.Kind,
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
