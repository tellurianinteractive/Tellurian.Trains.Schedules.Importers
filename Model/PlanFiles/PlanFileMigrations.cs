using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// The chain of migrations of the plan file (<c>Documentation/ModelHarmonisation.md</c>, section 5). A plan of an
/// older schema version goes through every step from its own version up to <see cref="PlanFile.CurrentSchemaVersion"/>.
/// </summary>
public static class PlanFileMigrations
{
    /// <summary>Every step, one per schema version it reads.</summary>
    public static IReadOnlyList<PlanFileMigration> All { get; } = [SchemaVersion1.Migration, SchemaVersion2.Migration, SchemaVersion3.Migration, SchemaVersion4.Migration];

    /// <summary>
    /// <paramref name="content"/> brought from <paramref name="fromVersion"/> to <paramref name="toVersion"/> by
    /// <paramref name="migrations"/>, one step at a time.
    /// </summary>
    /// <exception cref="PlanFileException">A step of the chain is missing.</exception>
    public static JsonNode Upgrade(JsonNode content, int fromVersion, int toVersion, IReadOnlyList<PlanFileMigration> migrations)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(migrations);
        for (var version = fromVersion; version < toVersion; version++)
        {
            var step = migrations.FirstOrDefault(m => m.FromVersion == version)
                ?? throw new PlanFileException(PlanFileError.MigrationMissing, $"There is no migration of a plan from schema version {version}.");
            content = step.Migrate(content);
        }
        return content;
    }
}
