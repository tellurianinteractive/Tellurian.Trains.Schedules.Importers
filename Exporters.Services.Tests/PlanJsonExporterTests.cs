using System.Text;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.PlanFiles;
using Tellurian.Trains.Schedules.Model.Schedules;

namespace Tellurian.Trains.Schedules.Exporters.Services.Tests;

[TestClass]
public sealed class PlanJsonExporterTests
{
    private static Plan TestPlan() => PlanFactory.CreatePlan("Test plan", "en");

    [TestMethod]
    public async Task WritesAPlanFileThatReadsBack()
    {
        var plan = TestPlan();
        using var stream = new MemoryStream();

        var result = await new PlanJsonExporter().ExportAsync(plan, stream, ExportOptions.Default, TestContext.CancellationToken);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreSame(plan, result.Item);
        var json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.AreEqual(PlanFile.Write(plan, writeIndented: true), json);
        Assert.AreEqual("Test plan", PlanFile.Read(json).Name);
    }

    [TestMethod]
    public async Task WritesCompactJsonWhenAsked()
    {
        using var stream = new MemoryStream();
        await new PlanJsonExporter().ExportAsync(TestPlan(), stream, ExportOptions.Default with { WriteIndented = false }, TestContext.CancellationToken);
        Assert.DoesNotContain("\n", Encoding.UTF8.GetString(stream.ToArray()));
    }

    [TestMethod]
    public async Task AStreamThatCantBeWrittenGivesAFailureWithACode()
    {
        using var readOnly = new MemoryStream([], writable: false);

        var result = await new PlanJsonExporter().ExportAsync(TestPlan(), readOnly, ExportOptions.Default, TestContext.CancellationToken);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(PlanJsonExporter.WriteFailed, result.Messages.Single().Code);
        Assert.AreEqual(Severity.Error, result.Messages.Single().Severity);
    }

    [TestMethod]
    public async Task TheExportServiceWritesTheFileAndCreatesItsFolder()
    {
        var folder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"plan-export-{Guid.NewGuid():N}"));
        var file = new FileInfo(Path.Combine(folder.FullName, "plan.json"));
        try
        {
            var result = await new JsonExportService(file).ExportScheduleAsync(TestPlan());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Test plan", PlanFile.Read(await File.ReadAllTextAsync(file.FullName, TestContext.CancellationToken)).Name);
        }
        finally
        {
            if (folder.Exists) folder.Delete(recursive: true);
        }
    }

    public TestContext TestContext { get; set; } = null!;
}
