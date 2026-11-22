namespace Tellurian.Trains.Schedules.Model.Layouts.Drawings;

/// <summary>A reference to a source document, such as a FREMO or MMM end profile PDF.</summary>
public sealed record DocumentReference : ExtensibleRecord
{
    /// <summary>The document's title.</summary>
    public string? Title { get; init; }
    /// <summary>Where the document can be found.</summary>
    public string? Url { get; init; }
    /// <summary>The document's revision, as the document gives it.</summary>
    public string? Revision { get; init; }
}
