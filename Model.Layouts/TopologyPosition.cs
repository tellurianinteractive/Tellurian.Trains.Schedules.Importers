namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>Where a location is drawn in the topology schematic.</summary>
public sealed class TopologyPosition
{
    /// <summary>The <see cref="OperationLocation.Id"/> of the location.</summary>
    public int LocationId { get; set; }

    /// <summary>The horizontal position.</summary>
    public double X { get; set; }

    /// <summary>The vertical position.</summary>
    public double Y { get; set; }
}
