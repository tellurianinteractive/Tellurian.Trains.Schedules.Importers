using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

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
    /// If false, indicate a station where train drivers has to operate it themselves, e.g. a freight terminal och a small shadow station.
    /// </summary>
    public bool IsManned { get; set; }

    /// <summary>
    /// Gets or sets the regions and countries represented by this station. Mostly meaningful for
    /// shadow stations (<see cref="IsShadow"/>), which stand in for the outside world and
    /// are used for cargo flow routing. Seldom used for for ordinary stations.
    /// </summary>
    public IList<Region> Regions { get; set; } = [];

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected Station() : base() { }
}

/// <summary>
/// 
/// </summary>
public static class StationExtensions
{
    extension(Station station)
    {
        /// <summary>
        /// Adds a <see cref="Region"/> if not already present.
        /// </summary>
        /// <param name="region"></param>
        public void Add(Region region)
        {
            if (station.Regions.Contains(region)) return;
            station.Regions.Add(region);
        }
    }
}

