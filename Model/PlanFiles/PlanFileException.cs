namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>A plan file that can't be read, and why.</summary>
public sealed class PlanFileException : Exception
{
    /// <summary>Creates the exception.</summary>
    public PlanFileException(PlanFileError error, string message, Exception? innerException = null) : base(message, innerException) => Error = error;

    /// <summary>Why the plan can't be read.</summary>
    public PlanFileError Error { get; }
}
