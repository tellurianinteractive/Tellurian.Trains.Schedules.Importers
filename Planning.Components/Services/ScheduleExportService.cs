using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

/// <summary>
/// Serialises the loaded <see cref="Plan"/> for download. JSON today; SQLite (.db) to follow,
/// which will need the EF Core SQLite provider running in WebAssembly.
/// </summary>
public sealed class ScheduleExportService
{
    /// <summary>
    /// Writes the plan as a plan file, exactly as browser storage writes it, so the result round-trips
    /// back in via "Open schedule". Indented, because this is the copy a person may open and read.
    /// </summary>
    public string ToJson(Plan plan) => PlanFile.Write(plan, writeIndented: true);
}
