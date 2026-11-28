using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;
using Tellurian.Trains.Schedules.Model.Drawings.Modules;
using Tellurian.Trains.Schedules.Model.Drawings.Tracks;

namespace Tellurian.Trains.Schedules.Model.Drawings.Tests;

/// <summary>The physical layout: modules placed with ids of the layout's, lines, groups and joints (<c>LayoutsLayerSurvey.md</c> 2.7, 6.1, 6.3).</summary>
[TestClass]
public sealed class LayoutLinesTests
{
    /// <summary>A station module as a module file has it: location id 0, tracks numbered from 1, track elements referring to them.</summary>
    internal static Module Station(string name)
    {
        var location = new Station(0, name, name[..3]);
        location.Tracks.Add(new StationTrack(1, "1"));
        location.Tracks.Add(new StationTrack(2, "2"));
        var module = new Module { Id = Guid.NewGuid(), Name = name, OperationLocation = location };
        module.Drawing.TrackElements.Add(new StraightTrack { Id = 1, Start = Pose.Origin, Length = 1000, StationTrackId = 1 });
        module.Drawing.TrackElements.Add(new StraightTrack { Id = 2, Start = Pose.Origin, Length = 1000, StationTrackId = 2 });
        return module;
    }

    /// <summary>A plain-line module with one track of its own.</summary>
    internal static Module Plain(string name)
    {
        var module = new Module { Id = Guid.NewGuid(), Name = name };
        module.Tracks.Add(new StationTrack(1, "1"));
        module.Drawing.TrackElements.Add(new StraightTrack { Id = 1, Start = Pose.Origin, Length = 500, StationTrackId = 1 });
        return module;
    }

    [TestMethod]
    public void APlacedModuleGetsIdsOfTheLayoutsAndKeepsItsOwnTrackIdsAsSource()
    {
        var layout = new DrawnLayout();
        var tellby = layout.Place(Station("Tellby"));
        var line = layout.Place(Plain("Line"));
        var ytterby = layout.Place(Station("Ytterby"));

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, layout.Modules.Select(m => m.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2 }, layout.Layout.OperationLocations.Select(l => l.Id).ToArray());
        Assert.AreSame(layout.Layout, ytterby.Module.OperationLocation!.Layout);
        Assert.AreEqual(2, ytterby.Module.OperationLocationId);

        var tracks = layout.Modules.SelectMany(m => m.Module.Tracks).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, tracks.Select(t => t.Id).ToArray(), "track ids are unique in the layout");
        CollectionAssert.AreEqual(new int?[] { 1, 2, 1, 1, 2 }, tracks.Select(t => t.SourceId).ToArray());
        CollectionAssert.AreEqual(new int?[] { 4, 5 }, ytterby.Module.Drawing.TrackElements.Select(e => e.StationTrackId).ToArray(), "track elements follow their tracks");
        Assert.AreEqual(3, line.Module.Drawing.TrackElements[0].StationTrackId);
        Assert.AreSame(ytterby.Module.OperationLocation, ytterby.Module.Tracks[0].Station);
        Assert.AreEqual(1, tellby.Module.Tracks[0].Id);
    }

    [TestMethod]
    public void PlacingLeavesTheModuleAsItWas()
    {
        var module = Station("Tellby");
        var layout = new DrawnLayout();
        layout.Place(Station("Kode"));

        var placed = layout.Place(module);

        Assert.AreNotSame(module, placed.Module);
        Assert.AreEqual(0, module.OperationLocation!.Id);
        CollectionAssert.AreEqual(new[] { 1, 2 }, module.Tracks.Select(t => t.Id).ToArray());
        Assert.IsNull(module.Tracks[0].SourceId);
        Assert.AreEqual(1, module.Drawing.TrackElements[0].StationTrackId);
        Assert.AreEqual(3, placed.Module.Drawing.TrackElements[0].StationTrackId);
    }

    [TestMethod]
    public void AModuleTakenFromAnotherLayoutKeepsTheIdOfItsDrawingAsSource()
    {
        var first = new DrawnLayout();
        first.Place(Station("Kode"));
        var moved = first.Place(Station("Tellby"));
        Assert.AreEqual(3, moved.Module.Tracks[0].Id);

        var placed = new DrawnLayout().Place(moved.Module);

        Assert.AreEqual(1, placed.Module.Tracks[0].Id);
        Assert.AreEqual(1, placed.Module.Tracks[0].SourceId);
    }

    [TestMethod]
    public void AGroupIsAMainLineWithTheBranchesThatStartFromIt()
    {
        var layout = new DrawnLayout();
        var ids = Enumerable.Range(0, 6).Select(i => layout.Place(Plain($"M{i}")).Id).ToArray();
        var main = layout.Add(Line(ids[0], ids[1]));
        var other = layout.Add(Line(ids[4]));
        var branchOfBranch = layout.Add(Line(ids[3]));
        branchOfBranch.StartAt = new(ids[2], ExitSide.Right);
        var branch = layout.Add(Line(ids[2]));
        branch.StartAt = new(ids[1], ExitSide.Right, 2);

        CollectionAssert.AreEqual(new[] { main, other }, layout.MainLines.ToArray());
        CollectionAssert.AreEqual(new[] { main, branch, branchOfBranch }, layout.GroupOf(main).ToArray());
        CollectionAssert.AreEqual(new[] { other }, layout.GroupOf(other).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, layout.Lines.Select(l => l.Id).ToArray());
        Assert.AreSame(branch, layout.LineOf(ids[2]));
        Assert.AreEqual(ids[2], layout.FindEntry(ids[2])!.ModuleId);
        Assert.IsEmpty(layout.ModulesPlacedMoreThanOnce());
    }

    [TestMethod]
    public void AModuleInTwoLinesIsFound()
    {
        var layout = new DrawnLayout();
        var id = layout.Place(Plain("M")).Id;
        layout.Add(Line(id));
        layout.Add(Line(id));
        CollectionAssert.AreEqual(new[] { id }, layout.ModulesPlacedMoreThanOnce().ToArray());
    }

    [TestMethod]
    public void TheLeftExitJoinsThePreviousModuleUnlessTurned()
    {
        var entry = new LineEntry { ModuleId = 1 };
        Assert.AreEqual((ExitPlace.Left, ExitPlace.Right), (entry.Entry, entry.Exit));
        var flipped = entry.Flipped();
        Assert.AreEqual((ExitPlace.Right, ExitPlace.Left), (flipped.Entry, flipped.Exit));
        Assert.IsFalse(entry.IsTurned, "flipping makes a copy");
        Assert.AreEqual(new ExitPlace(ExitSide.Right, 2), new LineEntry { ModuleId = 1, ContinuesFrom = new(ExitSide.Right, 2) }.Exit);
    }

    [TestMethod]
    public void JointsAreFoundInEitherOrder()
    {
        var layout = new DrawnLayout();
        var joint = new Joint { First = new(1, ExitSide.Right), Second = new(2, ExitSide.Left), NumberOfClamps = 3 };
        layout.Joints.Add(joint);

        Assert.AreSame(joint, layout.FindJoint(new(1, ExitSide.Right), new(2, ExitSide.Left)));
        Assert.AreSame(joint, layout.FindJoint(new(2, ExitSide.Left), new(1, ExitSide.Right)));
        Assert.IsNull(layout.FindJoint(new(1, ExitSide.Right), new(1, ExitSide.Right)));
        Assert.IsTrue(joint.Has(new(2, ExitSide.Left)));
    }

    [TestMethod]
    public void EndProfilesAndStandardsAreIncludedOnce()
    {
        var layout = new DrawnLayout();
        var standard = new ModuleStandard { Id = Guid.NewGuid(), ShortName = "FREMO/H0-RE" };

        Assert.AreSame(standard, layout.Include(standard));
        Assert.AreSame(standard, layout.Include(new ModuleStandard { Id = standard.Id, ShortName = "Same id" }));
        layout.Include(new ModuleStandard { ShortName = "No id" });

        Assert.AreSame(standard, layout.Standards.Single());
        Assert.AreSame(standard, layout.FindStandard(standard.Id!.Value));
        Assert.AreSame(FitTolerances.Default, layout.EffectiveFitTolerances);
    }

    internal static Line Line(params int[] moduleIds) => new() { Entries = [.. moduleIds.Select(id => new LineEntry { ModuleId = id })] };
}
