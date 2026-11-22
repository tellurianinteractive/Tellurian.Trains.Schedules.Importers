namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>An area of a hall where nothing may be placed, e.g. in front of a fire hose cabinet.</summary>
public sealed class NoGoZone : LayoutPart
{
    /// <summary>The identity of the zone, unique among the hall's no-go zones; given by <see cref="Hall.Add(NoGoZone)"/>.</summary>
    public int Id { get; set; }

    /// <summary>A closed path in hall coordinates, mm.</summary>
    public required PathDrawing Area { get; set; }

    /// <summary>Why nothing may be placed there.</summary>
    public required string Reason { get; set; }

    /// <inheritdoc/>
    public override NoGoZone Copy() => (NoGoZone)base.Copy();

    /// <inheritdoc/>
    public override string ToString() => Reason;
}
