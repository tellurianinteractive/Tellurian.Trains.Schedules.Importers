namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>
/// A path for train drivers to walk along while following their trains (HP-18). It may pass under duck-under areas, but
/// not through other modules (VA-13).
/// </summary>
public sealed class WalkingPath : LayoutPart
{
    /// <summary>The identity of the path, unique among the hall plan's walking paths; given by <see cref="HallPlan.Add(WalkingPath)"/>.</summary>
    public int Id { get; set; }

    /// <summary>An open path in hall coordinates, mm.</summary>
    public required PathDrawing CentreLine { get; set; }

    /// <summary>The width, mm; null uses <see cref="HallPlanSettings.WalkingPathWidth"/>.</summary>
    public double? Width { get; set; }

    /// <summary>A label shown in the drawing.</summary>
    public string? Label { get; set; }

    /// <summary>The width of the path, its own or that of <paramref name="settings"/>, mm.</summary>
    public double EffectiveWidth(HallPlanSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return Width ?? settings.WalkingPathWidth;
    }

    /// <inheritdoc/>
    public override WalkingPath Copy() => (WalkingPath)base.Copy();

    /// <inheritdoc/>
    public override string ToString() => Label ?? Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
