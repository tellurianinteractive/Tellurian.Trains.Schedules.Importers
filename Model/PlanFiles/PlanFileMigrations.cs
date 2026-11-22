using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.Files;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// The chain of migrations that brings the content of a plan file from the schema version it was written in to the
/// current one, one step per version. The steps are the plan's; the chain is the suite's (<see cref="FileEnvelope"/>).
/// </summary>
public static class PlanFileMigrations
{
    /// <summary>Every step, in order: from version 0 to 1, from 1 to 2, and so on.</summary>
    public static IReadOnlyList<FileMigration> All { get; } = [SchemaVersion1.Migration, SchemaVersion2.Migration, SchemaVersion3.Migration, SchemaVersion4.Migration];

    /// <summary>Brings <paramref name="content"/> from <paramref name="fromVersion"/> to <paramref name="toVersion"/> with <paramref name="migrations"/>.</summary>
    /// <exception cref="PlanFileException">A step is missing.</exception>
    public static JsonNode Upgrade(JsonNode content, int fromVersion, int toVersion, IReadOnlyList<FileMigration> migrations)
    {
        ArgumentNullException.ThrowIfNull(migrations);
        try
        {
            return FileEnvelope.Upgrade(PlanFile.Kind with { Migrations = migrations }, content, fromVersion, toVersion);
        }
        catch (FileFormatException ex)
        {
            throw PlanFile.ToPlanFileException(ex);
        }
    }
}
