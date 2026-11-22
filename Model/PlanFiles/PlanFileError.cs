namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>Why a plan file can't be read.</summary>
public enum PlanFileError
{
    /// <summary>The text is not JSON, or not a plan.</summary>
    NotAPlan,

    /// <summary>The plan was written by a newer version, with a schema version this one doesn't know.</summary>
    NewerVersion,

    /// <summary>A step of the migration chain is missing.</summary>
    MigrationMissing,
}
