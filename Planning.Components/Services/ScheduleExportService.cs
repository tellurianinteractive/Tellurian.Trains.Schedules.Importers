using System.Text.Json;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

/// <summary>
/// Serialises the loaded <see cref="Plan"/> for download. JSON today; SQLite (.db) to follow,
/// which will need the EF Core SQLite provider running in WebAssembly.
/// </summary>
public sealed class ScheduleExportService
{
    // The settings every other persistence path uses, so a downloaded plan is written exactly as
    // browser storage writes it: a country as its id, and a station call and a catalogue entry each
    // stored once. Indented, because this is the copy a person may open and read.
    private static readonly JsonSerializerOptions JsonOptions = PlanJson.CreateOptions(writeIndented: true);

    /// <summary>
    /// Serialises the plan to JSON using the same reference-preserving format the importer reads,
    /// so the result round-trips back in via "Open schedule".
    /// </summary>
    public string ToJson(Plan plan) => JsonSerializer.Serialize(plan, JsonOptions);
}
