using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// A manned <see cref="OperationLocation"/>
/// </summary>
public class Station : OperationLocation
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="signature"></param>
    public Station(int id, string name, string signature) : base(id, name, signature) { }
    /// <summary>
    /// Gets or sets a value indicating whether this is a special type of yard (shadow yard) normally placed in the end of a line.
    /// </summary>
    public bool IsShadow { get; set; }

    /// <summary>
    /// Gets or sets the regions and countries represented by this station. Meaningful for
    /// shadow stations (<see cref="IsShadow"/>), which stand in for the outside world and
    /// are used for cargo flow routing. Empty for ordinary stations.
    /// </summary>
    public IList<Region> Regions { get; set; } = [];

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected Station() : base() { }
}

