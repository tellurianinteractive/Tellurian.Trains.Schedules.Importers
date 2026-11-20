using Microsoft.Extensions.Logging.Abstractions;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.Components.Services;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// A layout made elsewhere, taken into the planner by the import service: a new plan, or a newer revision of the open
/// plan's own source.
/// </summary>
[TestClass]
public class LayoutImportTests
{
    public TestContext TestContext { get; set; } = default!;

    private static readonly Guid SourceId = Guid.Parse("50000000-0000-0000-0000-000000000005");

    private static Layout Layout(Guid sourceId, int revision, double distance = 10)
    {
        var layout = new Layout { Name = "Club meeting", SourceId = sourceId, SourceRevision = revision };
        var tellby = new Station(1, "Tellby", "Tby") { Layout = layout };
        tellby.Tracks.Add(new StationTrack(11, "1") { Station = tellby, PlatformLength = 100 });
        var ytterby = new Station(2, "Ytterby", "Yb") { Layout = layout };
        ytterby.Tracks.Add(new StationTrack(21, "1") { Station = ytterby, PlatformLength = 100 });
        layout.OperationLocations.Add(tellby);
        layout.OperationLocations.Add(ytterby);
        layout.TrackStretches.Add(new TrackStretch(1, tellby, ytterby, distance) { Layout = layout });
        return layout;
    }

    private static ScheduleImportService Service(ILayoutImporter importer) =>
        new(null!, null!, NullLoggerFactory.Instance, [importer]);

    private static readonly FakeImporter Importer = new(() => new Layout());

    [TestMethod]
    public void ALayoutBecomesANewPlan()
    {
        var import = ScheduleImportService.NewPlan(Importer, Layout(SourceId, 1), "en", "New layout");

        Assert.IsFalse(import.IsUpdate);
        Assert.AreEqual("Club meeting", import.Plan.Name);
        Assert.HasCount(2, import.Plan.Layout.OperationLocations);
    }

    [TestMethod]
    public void ARevisionOfThePlansOwnSourceUpdatesIt()
    {
        var current = PlanFactory.CreatePlan(Layout(SourceId, 1), "en");
        var newer = Layout(SourceId, 2, distance: 20);
        Assert.IsTrue(current.IsMadeFrom(newer));

        var import = ScheduleImportService.Update(Importer, newer, current);

        Assert.IsTrue(import.IsUpdate);
        Assert.AreSame(current, import.Plan);
        Assert.AreEqual(2, current.Layout.SourceRevision);
        Assert.AreEqual(20, current.Layout.TrackStretches.Single().Distance, 0.0005);
        Assert.HasCount(1, import.Update!.Applied);
    }

    [TestMethod]
    public void ARevisionCanBeMadeANewPlanInstead()
    {
        var current = PlanFactory.CreatePlan(Layout(SourceId, 1), "en");

        var import = ScheduleImportService.NewPlan(Importer, Layout(SourceId, 2, distance: 20), "en", "New layout");

        Assert.AreNotSame(current, import.Plan);
        Assert.AreEqual(10, current.Layout.TrackStretches.Single().Distance, 0.0005, "the open plan is left as it was");
        Assert.AreEqual(2, import.Plan.Layout.SourceRevision);
    }

    [TestMethod]
    public void ALayoutFromAnotherSourceIsNotARevision()
    {
        var current = PlanFactory.CreatePlan(Layout(SourceId, 1), "en");

        Assert.IsFalse(current.IsMadeFrom(Layout(Guid.NewGuid(), 1)));
    }

    [TestMethod]
    public async Task ALayoutIsReadWithoutTakingItIntoAPlan()
    {
        var importer = new FakeImporter(() => Layout(SourceId, 3));

        var read = await Service(importer).ReadLayoutAsync(importer, Stream.Null, "meeting.json", TestContext.CancellationToken);

        Assert.IsTrue(read.IsSuccess);
        Assert.AreEqual(3, read.Item.SourceRevision);
    }

    [TestMethod]
    public async Task WhatAnImporterThrowsIsReported()
    {
        var importer = new FakeImporter(() => throw new InvalidOperationException("Broken"));

        var read = await Service(importer).ReadLayoutAsync(importer, Stream.Null, "meeting.json", TestContext.CancellationToken);

        Assert.IsTrue(read.IsFailure);
        Assert.AreEqual("Broken", read.Messages.Single().Text);
    }

    private sealed class FakeImporter(Func<Layout> layout) : ILayoutImporter
    {
        public string ResourceKey => "Fake";
        public IReadOnlyList<string> FileExtensions => [".json"];
        public bool CanImport(string fileName, ReadOnlySpan<byte> content) => true;
        public Task<ImportResult<Layout>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default) =>
            Task.FromResult(ImportResult<Layout>.Success(layout()));
    }
}
