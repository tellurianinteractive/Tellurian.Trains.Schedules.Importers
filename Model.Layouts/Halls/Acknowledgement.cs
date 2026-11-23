using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>
/// A planner's decision that a specific warning is OK (HP-15). It holds only while the modules involved are placed as
/// they were, which the fingerprint of their state (pose, drawing, leg adjustment and module revision) records.
/// </summary>
public sealed class Acknowledgement : LayoutPart
{
    /// <summary>The identity of the acknowledgement, unique in the hall plan; given by <see cref="HallPlan.Add(Acknowledgement)"/>.</summary>
    public int Id { get; set; }

    /// <summary>The code of the kind of finding acknowledged (VA-11, VA-12, VA-13, VA-14).</summary>
    public required string FindingCode { get; set; }

    /// <summary>The <see cref="LayoutModule.Id"/> of each module involved.</summary>
    public List<int> ModuleIds { get; set; } = [];

    /// <summary>The state of the modules involved when it was acknowledged.</summary>
    public required string Fingerprint { get; set; }

    /// <summary>Why it is OK.</summary>
    public string? Note { get; set; }

    /// <summary>When it was acknowledged.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required DateTimeOffset Time { get; set; }

    /// <summary>A copy with a copy of its list of modules.</summary>
    public override Acknowledgement Copy()
    {
        var copy = (Acknowledgement)base.Copy();
        copy.ModuleIds = [.. ModuleIds];
        return copy;
    }

    /// <inheritdoc/>
    public override string ToString() => FindingCode;
}
