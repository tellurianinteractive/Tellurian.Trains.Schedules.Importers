
namespace Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

/// <summary>
/// Overrides the rule result (spec §3.2) for a specific pair of end profiles, for known exceptions.
/// The pair is unordered and applies to all orientations.
/// </summary>
public sealed record EndProfileCompatibilityOverride : ExtensibleRecord
{
    /// <summary>The id of one end profile of the pair.</summary>
    public required Guid FirstEndProfileId { get; init; }
    /// <summary>The id of the other end profile of the pair.</summary>
    public required Guid SecondEndProfileId { get; init; }
    /// <summary>The fit that replaces the rule result.</summary>
    public required JointFit Fit { get; init; }
    /// <summary>Why the override exists, if noted.</summary>
    public string? Note { get; init; }

    /// <summary>True if the override is for the two end profiles, in either order.</summary>
    public bool Applies(Guid oneEndProfileId, Guid otherEndProfileId) =>
        (FirstEndProfileId == oneEndProfileId && SecondEndProfileId == otherEndProfileId) ||
        (FirstEndProfileId == otherEndProfileId && SecondEndProfileId == oneEndProfileId);
}
