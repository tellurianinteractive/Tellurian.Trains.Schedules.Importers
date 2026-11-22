using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>
/// A track crossing an end profile. Its rail top is at Y = 0 by definition (RÖK); <see cref="Offset"/> is the lateral
/// position of the track centre, positive to the right; millimetres.
/// </summary>
public sealed class EndProfileTrack : LayoutPart
{
    /// <summary>The lateral position of the track centre, positive to the right.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Offset { get; set; }

    /// <summary>
    /// The gauges of the track, the distances between its rails: one, or more for a dual-gauge track sharing a rail
    /// (decision 13 of <c>CrossAppReview.md</c>). The standard names them (<see cref="ModuleStandard.Gauges"/>).
    /// </summary>
    public required List<double> Gauges { get; set; }

    /// <summary>The widest gauge, which sets how wide the track is drawn; 0 when the track has none.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public double WidestGauge => Gauges.Count == 0 ? 0 : Gauges.Max();

    /// <summary>Whether the track has a gauge within <paramref name="tolerance"/> of one of <paramref name="other"/>'s, so trains of that gauge can pass a joint.</summary>
    public bool SharesAGaugeWith(EndProfileTrack other, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Gauges.Any(g => other.Gauges.Any(o => Math.Abs(g - o) <= tolerance));
    }

    /// <summary>How the track is powered.</summary>
    public RailSystem RailSystem { get; set; }

    /// <summary>A label for the reader, e.g. "1".</summary>
    public string? Label { get; set; }

    /// <inheritdoc/>
    public override EndProfileTrack Copy()
    {
        var copy = (EndProfileTrack)base.Copy();
        copy.Gauges = [.. Gauges];
        return copy;
    }

    /// <summary>The same track on the other side of the centre line.</summary>
    public EndProfileTrack MirroredX() => this.With(t => t.Offset = -Offset);
}
