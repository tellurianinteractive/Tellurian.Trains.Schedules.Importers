using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// What the planner has decided for the joint between two exits of placed modules (HP-11). The fit and the join method
/// by the rules are worked out from the end profiles, not stored; only the planner's choices are, so a joint is listed
/// only when something differs from the defaults.
/// </summary>
public sealed class Joint : LayoutPart
{
    /// <summary>The number of clamps of a clamped joint, unless the planner says otherwise.</summary>
    public const int DefaultNumberOfClamps = 2;

    /// <summary>One of the exits.</summary>
    public required ExitReference First { get; set; }

    /// <summary>The other exit.</summary>
    public required ExitReference Second { get; set; }

    /// <summary>The join method chosen against the rules, e.g. clamps when the screw-hole data is missing.</summary>
    public JoinMethod? JoinMethodOverride { get; set; }

    /// <summary>The number of clamps of a clamped joint. Always written, since it is not 0 unless said otherwise.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int NumberOfClamps { get; set; } = DefaultNumberOfClamps;

    /// <summary>A note for the people setting up.</summary>
    public string? Note { get; set; }

    /// <summary>Whether this joint connects <paramref name="a"/> and <paramref name="b"/>, in either order.</summary>
    public bool Connects(ExitReference a, ExitReference b) =>
        (First == a && Second == b) || (First == b && Second == a);

    /// <summary>Whether one of the exits of this joint is <paramref name="exit"/>.</summary>
    public bool Has(ExitReference exit) => First == exit || Second == exit;

    /// <inheritdoc/>
    public override Joint Copy() => (Joint)base.Copy();

    /// <inheritdoc/>
    public override string ToString() => $"{First} – {Second}";
}
