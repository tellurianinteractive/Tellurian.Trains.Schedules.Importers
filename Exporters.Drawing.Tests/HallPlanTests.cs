using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;
using Tellurian.Trains.Schedules.Model.Layouts.Halls;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;

namespace Tellurian.Trains.Schedules.Exporters.Drawing.Tests;

/// <summary>
/// Module Draw's hall plan files: a layout and where it stands in a hall (<c>LayoutsLayerSurvey.md</c> 2.8), with the
/// layout written as <c>content.layout</c> in its own form, which the planner reads.
/// </summary>
[TestClass]
public sealed class HallPlanTests
{
    private static readonly HallPlanExporter Exporter = new();

    private static string TestFile(string name) => File.ReadAllText(Path.Combine("TestData", name)).ReplaceLineEndings("\n");

    private static HallPlan Read(string json)
    {
        var result = new HallPlanImporter().Import(json);
        Assert.IsTrue(result.IsSuccess, string.Join(", ", result.Messages.Select(m => m.Text)));
        return result.Item;
    }

    /// <summary>Two copies of a station module and a plain one between them in a main line, and a branch from the second station.</summary>
    internal static HallPlan Plan()
    {
        var hall = new HallImporter().Import(TestFile("club-hall.v1.hall.json")).Item;
        var content = new ModuleImporter().Import(TestFile("station-3-sections.module.json")).Item;
        var layout = new Layout { PreferredStandard = new ModuleStandard { Id = Guid.Parse("5b6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4b01"), ShortName = "FREMO/H0-RE" } };
        layout.Include(layout.PreferredStandard);
        foreach (var profile in content.EndProfiles) layout.Include(profile);
        var tellby = layout.Place(content.Module);
        var plain = layout.Place(new Module { Id = Guid.NewGuid(), Name = "Line", Drawing = content.Module.Drawing.Copy() });
        var ytterby = layout.Place(content.Module.With(m => m.Name = "Ytterby"));
        var branchEnd = layout.Place(new Module { Id = Guid.NewGuid(), Name = "Terminus" });
        var main = layout.Add(new Line { Name = "Main", Entries = [new() { ModuleId = tellby.Id }, new() { ModuleId = plain.Id, IsTurned = true, LegAdjustment = 5 }, new() { ModuleId = ytterby.Id }] });
        layout.Add(new Line { Entries = [new() { ModuleId = branchEnd.Id, Note = "Behind the curtain" }], StartAt = new(ytterby.Id, ExitSide.Right) });
        layout.Joints.Add(new Joint { First = new(tellby.Id, ExitSide.Right), Second = new(plain.Id, ExitSide.Right), JoinMethodOverride = JoinMethod.Clamped, NumberOfClamps = 3 });
        layout.FitTolerances = new() { Joint = 1 };
        var plan = new HallPlan
        {
            Id = Guid.Parse("7a6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4c01"),
            Name = "Club meeting",
            Hall = hall,
            Layout = layout,
            Anchors =
            [
                new()
                {
                    LineId = main.Id,
                    ModuleId = tellby.Id,
                    Pose = new(new Point(1000, 2000), Angle.Quarter),
                    IsLocked = true,
                    Dimensions = new() { ReferencePoint = AnchorReferencePoint.ExitCentre(ExitPlace.Left), First = HallElementReference.Wall(0), Second = HallElementReference.Obstacle(2), AngleWall = 0 },
                },
            ],
            Settings = new() { JointGap = 3, IsClearanceChecked = false },
        };
        plan.Add(new PublicArea { Area = PathDrawing.Rectangle(2000, 1000, Transform.Identity), Label = "Visitors" });
        plan.Add(new WalkingPath { CentreLine = PathDrawing.Polyline([new(0, 0), new(3000, 0)]), Width = 800 });
        plan.Add(new Acknowledgement { FindingCode = "VA-12", ModuleIds = [tellby.Id, plain.Id], Fingerprint = "abc", Time = new DateTimeOffset(2026, 11, 22, 10, 0, 0, TimeSpan.Zero) });
        return plan;
    }

    [TestMethod]
    public void AHallPlanReadsBackAndWritesTheSame()
    {
        var plan = Plan();
        var written = Exporter.Write(plan, ExportOptions.Default);

        var read = Read(written);

        Assert.AreEqual(written, Exporter.Write(read, ExportOptions.Default));
        Assert.AreEqual(plan.Anchors[0].Pose, read.Anchors[0].Pose);
        Assert.AreEqual(2, read.Anchors[0].Dimensions!.Second.ObstacleId);
        Assert.AreEqual(plan.Acknowledgements[0].Time, read.Acknowledgements[0].Time);
        CollectionAssert.AreEqual(new[] { 1, 2 }, read.Acknowledgements[0].ModuleIds);
        Assert.AreEqual(3, read.Settings.JointGap);
        Assert.IsFalse(read.Settings.IsClearanceChecked);
        Assert.AreEqual(1, read.PublicAreas[0].Id);
    }

    [TestMethod]
    public void TheLayoutIsReadWithItsReferencesRestored()
    {
        var read = Read(Exporter.Write(Plan(), ExportOptions.Default));
        var layout = read.Layout;

        Assert.HasCount(4, layout.Modules);
        Assert.HasCount(2, layout.OperationLocations);
        var ytterby = layout.FindModule(3)!.Module;
        Assert.AreSame(layout.OperationLocations.Single(l => l.Id == 2), ytterby.OperationLocation);
        Assert.AreSame(layout, ytterby.OperationLocation!.Layout);
        Assert.AreEqual("Ytterby", ytterby.Name);
        Assert.IsTrue(ytterby.HasDrawing);
        Assert.HasCount(2, layout.Lines);
        Assert.IsTrue(layout.Lines.ElementAt(1).IsBranch);
        Assert.IsTrue(layout.FindEntry(2)!.IsTurned);
        Assert.AreEqual(5, layout.FindEntry(2)!.LegAdjustment);
        Assert.AreEqual(3, layout.Joints.Single().NumberOfClamps);
        Assert.AreEqual(JoinMethod.Clamped, layout.Joints.Single().JoinMethodOverride);
        Assert.AreEqual(1, layout.EffectiveFitTolerances.Joint);
        Assert.IsNotNull(layout.FindEndProfile(Guid.Parse("5b6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4d01")));
        Assert.AreEqual("FREMO/H0-RE", read.PrimaryStandard!.ShortName);
        Assert.IsTrue(read.IsAccepted(read.PrimaryStandard.Id!.Value));
    }

    [TestMethod]
    public void ThePlannerReadsTheLayoutAsWritten()
    {
        var plan = Plan();
        plan.Revision = 4;
        var written = Exporter.Write(plan, ExportOptions.Default);

        var layout = HallPlanFile.ReadLayout(written);

        Assert.AreEqual(plan.Id, layout.SourceId);
        Assert.AreEqual(4, layout.SourceRevision);
        Assert.AreEqual("Club meeting", layout.Name, "a layout without a name of its own has the hall plan's");
        Assert.HasCount(2, layout.Lines);
        Assert.AreSame(layout.OperationLocations.Single(l => l.Id == 2), layout.FindModule(3)!.Module.OperationLocation);
        var module = JsonNode.Parse(written)!["content"]!["layout"]!["modules"]![2]!["module"]!;
        Assert.AreEqual(2, (int)module["operationLocationId"]!, "in a layout a module refers to its location by id");
        Assert.IsNull(module["operationLocation"]);
        Assert.IsNull(plan.Layout.SourceId, "the hall plan given is not changed");
    }

    [TestMethod]
    public void ARevisionCountsUpAndTheLayoutFollows()
    {
        var plan = Plan();

        var written = Read(Exporter.Write(plan, new ExportOptions(IsRevision: true, RevisedBy: "Stefan", Date: new DateOnly(2026, 11, 22))));

        Assert.AreEqual(1, written.Revision);
        Assert.AreEqual(1, written.Layout.SourceRevision);
        Assert.AreEqual("Stefan", written.RevisionLog[0].Name);
        Assert.AreEqual(0, plan.Revision);
        Assert.IsEmpty(plan.RevisionLog);
    }

    [TestMethod]
    public void HallPlanFilesBeforeVersion2AreRefused()
    {
        const string json = """{ "format": "tellurian-drawing", "kind": "HallPlan", "schemaVersion": 1, "content": { "id": "7a6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4c01", "name": "Old", "layouts": [] } }""";

        var result = new HallPlanImporter().Import(json);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FileErrorCode.MigrationMissing, result.Messages.Single().Code);
    }

    [TestMethod]
    public void HallsAndHallPlansAreNotTakenForEachOther()
    {
        var plan = Exporter.Write(Plan(), ExportOptions.Default);
        var hall = new HallExporter().Write(Plan().Hall, ExportOptions.Default);

        Assert.AreEqual(FileErrorCode.WrongKind, new HallImporter().Import(plan).Messages.Single().Code);
        Assert.AreEqual(FileErrorCode.WrongKind, new HallPlanImporter().Import(hall).Messages.Single().Code);
        Assert.IsTrue(new HallPlanImporter().CanImport("x.json", System.Text.Encoding.UTF8.GetBytes(plan)));
        Assert.IsFalse(new HallPlanImporter().CanImport("x.json", System.Text.Encoding.UTF8.GetBytes(hall)));
    }

    [TestMethod]
    public void ACopyOwnsItsLayoutAndParts()
    {
        var plan = Plan();

        var copy = plan.Copy();
        copy.Layout.Lines.First().Entries[0].IsTurned = true;
        copy.Layout.FindModule(1)!.Module.Name = "Changed";
        copy.Anchors[0].IsLocked = false;
        copy.Hall.Name = "Other hall";
        copy.Acknowledgements[0].ModuleIds.Add(3);

        Assert.IsFalse(plan.Layout.Lines.First().Entries[0].IsTurned);
        Assert.AreNotEqual("Changed", plan.Layout.FindModule(1)!.Module.Name);
        Assert.IsTrue(plan.Anchors[0].IsLocked);
        Assert.AreNotEqual("Other hall", plan.Hall.Name);
        Assert.HasCount(2, plan.Acknowledgements[0].ModuleIds);
        Assert.AreSame(copy.Layout, copy.Layout.FindModule(3)!.Module.OperationLocation!.Layout);
    }

    [TestMethod]
    public void TheAddedAreasPathsAndAcknowledgementsGetIdsOfTheirOwn()
    {
        var plan = Plan();
        var second = plan.Add(new PublicArea { Area = PathDrawing.Rectangle(10, 10, Transform.Identity), Id = 1 });
        Assert.AreEqual(2, second.Id, "an id in use is replaced");
        Assert.AreEqual(1, plan.WalkingPaths[0].Id);
        Assert.AreEqual(800, plan.WalkingPaths[0].EffectiveWidth(plan.Settings));
        Assert.AreEqual(HallPlanSettings.DefaultWalkingPathWidth, new WalkingPath { CentreLine = plan.WalkingPaths[0].CentreLine }.EffectiveWidth(plan.Settings));
    }
}
