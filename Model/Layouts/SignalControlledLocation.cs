using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// An <see cref="OperationLocation"/> that is unmanned
/// </summary>
public class SignalControlledLocation : OperationLocation
{
    /// <summary>
    /// The <see cref="Station"/> that controls this location.
    /// </summary>
    public Station? ControlledBy { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="signature"></param>
    public SignalControlledLocation(int id, string name, string signature) : base(id, name, signature) { }

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected SignalControlledLocation() : base() { }
}
