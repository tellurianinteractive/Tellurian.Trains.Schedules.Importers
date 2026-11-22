using Microsoft.Extensions.Logging;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Xpln;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

public sealed class ScheduleImportService(
    ICompaniesService companiesService,
    ITrainCategoriesService categoriesService,
    ILoggerFactory loggerFactory,
    IEnumerable<ILayoutImporter> layoutImporters)
{
    private static readonly string[] PlanFileExtensions = [".json", ".ods", ".xlsx"];

    /// <summary>Every file extension that can be opened: plan files, XPLN, and what the registered layout importers read.</summary>
    public IReadOnlyList<string> FileExtensions =>
        [.. PlanFileExtensions.Concat(layoutImporters.FileExtensions).Distinct(StringComparer.OrdinalIgnoreCase)];

    /// <summary>The registered layout importer that reads the file, if any; otherwise the file is a plan or XPLN.</summary>
    public ILayoutImporter? LayoutImporterFor(string fileName, ReadOnlySpan<byte> content) => layoutImporters.For(fileName, content);

    /// <summary>
    /// Reads a layout made elsewhere with <paramref name="importer"/>, without taking it into any plan: that is
    /// <see cref="NewPlan"/> or <see cref="Update"/>, once it is known which. Whatever the importer throws is reported
    /// as a failure, since an importer may come from elsewhere.
    /// </summary>
    public async Task<ImportResult<Layout>> ReadLayoutAsync(ILayoutImporter importer, Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(importer);
        try
        {
            var read = await importer.ImportAsync(stream, fileName, cancellationToken);
            return read.IsSuccess && !read.Items.Any() ? ImportResult<Layout>.Failure([.. read.Messages]) : read;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ImportResult<Layout>.Failure(Message.System(ex.Message));
        }
    }

    /// <summary>A new plan made from <paramref name="layout"/>, which it takes over (<see cref="PlanFactory.CreatePlan(Layout, string, string?)"/>).</summary>
    /// <param name="language">The two-letter language the new plan's catalogues are made in.</param>
    /// <param name="newPlanName">The name of the new plan when the layout has none.</param>
    public static LayoutImport NewPlan(ILayoutImporter importer, Layout layout, string language, string newPlanName) =>
        new(importer, layout, PlanFactory.CreatePlan(layout, language, newPlanName), null);

    /// <summary>
    /// <paramref name="current"/> updated with <paramref name="layout"/>, a revision of the layout it was made from
    /// (<c>plan.UpdateFromHallPlan</c>): what changes only distances and times is applied, the rest is left to confirm.
    /// </summary>
    public static LayoutImport Update(ILayoutImporter importer, Layout layout, Plan current)
    {
        ArgumentNullException.ThrowIfNull(current);
        return new(importer, layout, current, current.UpdateFromHallPlan(layout));
    }

    public async Task<ImportResult<Plan>> ImportFromJsonAsync(Stream stream, string name)
    {
        try
        {
            return ImportResult<Plan>.Success(await PlanFile.ReadAsync(stream));
        }
        catch (PlanFileException ex)
        {
            return ImportResult<Plan>.Failure(Message.System(ex.Message));
        }
    }

    public async Task<ImportResult<Plan>> ImportFromXplnAsync(Stream stream, string fileName)
    {
        try
        {
            IDataSetProvider provider = fileName.EndsWith(".ods", StringComparison.OrdinalIgnoreCase)
                ? new OdsDataSetProvider(loggerFactory.CreateLogger<OdsDataSetProvider>())
                : new XlsxDataSetProvider(loggerFactory.CreateLogger<XlsxDataSetProvider>());

            var logger = loggerFactory.CreateLogger<XplnDataImporter>();
            // The language and country come from a culture in the file name (e.g. "Plan.de-DE.ods"),
            // falling back to the current UI culture when the name has no culture segment.
            var options = XplnImportOptions.FromFileName(fileName);
            using var importer = new XplnDataImporter(stream, provider, companiesService, categoriesService, logger, options);
            var name = XplnImportOptions.ScheduleNameFrom(fileName);
            var result = await importer.ImportScheduleAsync(name);
            // XPLN has no platforms: every track of a location that exchanges passengers is given one.
            if (result.IsSuccess) result.Item.Layout.EnsurePlatforms();
            return result;
        }
        catch (Exception ex)
        {
            return ImportResult<Plan>.Failure(Message.System($"XPLN import error: {ex.Message}"));
        }
    }
}

/// <summary>
/// What taking a layout into the planner did: the plan it went into, either a new plan or the open plan updated
/// (<see cref="Update"/>, with what was applied and what is left).
/// </summary>
public sealed record LayoutImport(ILayoutImporter Importer, Layout Source, Plan Plan, HallPlanUpdate? Update)
{
    /// <summary>The open plan was updated with a revision of its own source, rather than a new plan made.</summary>
    public bool IsUpdate => Update is not null;
}
