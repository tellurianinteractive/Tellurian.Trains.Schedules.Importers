using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.App.Services;

/// <summary>
/// Serialises the loaded <see cref="Plan"/> for download. JSON today; SQLite (.db) to follow,
/// which will need the EF Core SQLite provider running in WebAssembly.
/// </summary>
public sealed class ScheduleExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    /// <summary>
    /// Serialises the plan to JSON using the same reference-preserving format the importer reads,
    /// so the result round-trips back in via "Open schedule".
    /// </summary>
    public string ToJson(Plan plan) => JsonSerializer.Serialize(plan, JsonOptions);
}
