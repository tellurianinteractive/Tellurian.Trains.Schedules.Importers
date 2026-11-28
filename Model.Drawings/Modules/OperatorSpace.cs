namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>
/// An area next to the module where operators stand (MD-17). Either attached to a side with a depth
/// (auto), or a polygon drawn by hand (custom, <see cref="Attachment"/> null). The area is always stored.
/// </summary>
public sealed record OperatorSpace : ExtensibleRecord
{
    /// <summary>The part of a side the space is attached to; null for a space drawn by hand.</summary>
    public SideAttachment? Attachment { get; init; }

    /// <summary>The depth from the side for an attached space, mm.</summary>
    public double? Depth { get; init; }

    /// <summary>A closed path in module coordinates, outside the baseboard.</summary>
    public required PathDrawing Area { get; init; }

    /// <summary>An optional label for the space.</summary>
    public string? Label { get; init; }
}
