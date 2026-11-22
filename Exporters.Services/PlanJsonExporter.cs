using System.Text;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Exporters.Services;

/// <summary>
/// Writes a plan as a plan file (<see cref="PlanFile"/>): the JSON that the planner opens and that Tellurian Dispatch
/// reads. A plan has no revision log, so <see cref="ExportOptions.IsRevision"/> changes nothing, and the envelope
/// records the version of the model library.
/// </summary>
public sealed class PlanJsonExporter : IExporter<Plan>
{
    /// <summary>The <see cref="Message.Code"/> of a failure to write to the destination.</summary>
    public const string WriteFailed = nameof(WriteFailed);

    /// <inheritdoc/>
    public string ResourceKey => "TellurianPlanJson";

    /// <inheritdoc/>
    public string FileExtension => ".json";

    /// <inheritdoc/>
    public string MediaType => "application/json";

    /// <inheritdoc/>
    public async Task<ExportResult<Plan>> ExportAsync(Plan item, Stream destination, ExportOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(options);
        try
        {
            var bytes = Encoding.UTF8.GetBytes(PlanFile.Write(item, options.WriteIndented));
            await destination.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
            await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            return ExportResult<Plan>.Success(item);
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or ObjectDisposedException or System.Text.Json.JsonException or InvalidOperationException)
        {
            return ExportResult<Plan>.Failure(Message.Error($"{ex.GetType().Name}: {ex.Message}") with { Code = WriteFailed });
        }
    }
}
