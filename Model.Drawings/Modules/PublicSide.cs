namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>A side, or part of one, that the owner intends to face the public (MD-18).</summary>
public sealed record PublicSide : ExtensibleRecord
{
    /// <summary>The side, or part of it, that faces the public.</summary>
    public required SideAttachment Attachment { get; init; }
}
