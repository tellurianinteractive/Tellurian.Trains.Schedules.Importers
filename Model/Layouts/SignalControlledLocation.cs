using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// An <see cref="OperationLocation"/> that is unmanned and signal controlled, such as a block post or junction.
/// </summary>
/// <remarks>
/// A train never stops here: it always passes through regardless of a call's
/// <see cref="Timetables.StationCall.IsStop"/> flags. The location exists for dispatch/block boundaries
/// and timing only, which is why it has no passenger or cargo exchange. Reports and the graph therefore
/// render any call here as a pass-through (a pipe), never as arrival/departure times.
/// </remarks>
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
    /// Always false: a signal controlled location is unmanned and has no passenger exchange.
    /// </summary>
    public override bool HasPassengerExchange { get => false; set { } }

    /// <summary>
    /// Always false: a signal controlled location is unmanned and has no cargo exchange.
    /// </summary>
    public override bool HasCargoExchange { get => false; set { } }

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected SignalControlledLocation() : base() { }
}
