using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;
using Tellurian.Trains.Schedules.Model.Drawings.Modules;
using Tellurian.Trains.Schedules.Model.Layouts;

namespace Tellurian.Trains.Schedules.Exporters.Drawing.Tests;

/// <summary>
/// What a module drawing shares with its row in the Module Registry (<see cref="ModuleRegistryData"/>,
/// <c>ModelSurfaceAnalysis.md</c> 4.2): read and derived from a module file, and written back one way only.
/// </summary>
[TestClass]
public sealed class ModuleRegistryDataTests
{
    private static ModuleFileContent Station()
    {
        var result = new ModuleImporter().Import(File.ReadAllText(Path.Combine("TestData", "station-3-sections.module.json")));
        Assert.IsTrue(result.IsSuccess, string.Join(", ", result.Messages.Select(m => m.Text)));
        // The file's station has no tracks of its own: one for the tests, with a length as if measured.
        var station = result.Item.Module.OperationLocation!;
        if (station.Tracks.Count == 0) station.Add(new StationTrack(9, "1") { DisplayOrder = 1, Length = 2.5, Usage = "Passenger" });
        return result.Item;
    }

    [TestMethod]
    public void TheSharedPropertiesAreReadAndWorkedOutFromTheFile()
    {
        var content = Station();

        var data = ModuleRegistryData.From(content);

        Assert.AreEqual(content.Module.Name, data.Name);
        Assert.HasCount(content.Module.Drawing.Exits.Count, data.Exits);
        Assert.IsNotNull(data.Measures, "a drawn module has measures");
        Assert.IsNotEmpty(data.Gauges, "the gauges come from the exits' end profiles");
        CollectionAssert.Contains(data.RailSystems.ToArray(), RailSystem.TwoRail);
        Assert.AreEqual(content.Module.Drawing.HasDuckUnder, data.IsDuckunder);
        Assert.AreEqual(content.Module.Drawing.IsEndOfLine, data.IsEndOfLine);
        Assert.IsNotNull(data.Location);
        Assert.AreEqual(content.Module.OperationLocation!.Signature, data.Location.Signature);
        Assert.HasCount(content.Module.OperationLocation.Tracks.Count, data.Location.Tracks);
    }

    [TestMethod]
    public void WhatTheRegistryOwnsIsWrittenBackAndTheDrawingIsLeftAlone()
    {
        var content = Station();
        var data = ModuleRegistryData.From(content);
        var track = data.Location!.Tracks[0];
        var changed = data with
        {
            Name = "Renamed",
            Prefix = "SF",
            Number = 12,
            ModuleOwners = "Module Club",
            FunctionalState = 3,
            IsDuckunder = !data.IsDuckunder,
            Location = data.Location with
            {
                Name = "Renamed station",
                Tracks = [track with { Usage = "Goods", Length = track.Length + 99 }, .. data.Location.Tracks.Skip(1)],
            },
        };

        var written = content.WithRegistryData(changed);

        Assert.AreEqual("Renamed", written.Module.Name);
        Assert.AreEqual("SF", written.Module.Prefix);
        Assert.AreEqual(12, written.Module.Number);
        Assert.AreEqual("Module Club", written.Module.ModuleOwners);
        Assert.AreEqual(3, written.Module.FunctionalState);
        Assert.AreEqual(content.Module.IsDuckunder, written.Module.IsDuckunder, "a feature worked out from the drawing is not written back");
        Assert.AreEqual("Renamed station", written.Module.OperationLocation!.Name);
        var writtenTrack = written.Module.OperationLocation.Tracks.Single(t => t.Designation == track.Designation);
        Assert.AreEqual("Goods", writtenTrack.Usage);
        Assert.AreEqual(content.Module.OperationLocation!.Tracks.Single(t => t.Designation == track.Designation).Length, writtenTrack.Length, "a length measured in the drawing is not written back");
        Assert.HasCount(content.Module.Drawing.Exits.Count, written.Module.Drawing.Exits);
        Assert.AreNotEqual("Renamed", content.Module.Name, "the content given is not changed");
    }
}
