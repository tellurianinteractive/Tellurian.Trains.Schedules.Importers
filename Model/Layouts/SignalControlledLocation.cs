using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// An <see cref="OperationLocation"/> that is unmanned and signal controlled, worked from a manned station: a
/// block post, a junction, a place where trains can cross, or a junction where they can also cross.
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
    /// Gets or sets whether trains in opposite directions can cross here, one waiting while the other
    /// passes. Stated rather than derived: a junction needs two tracks to know which way a train goes,
    /// whether or not trains can cross, so the tracks do not tell. A location that is neither a
    /// junction nor a crossing place is a block post (<c>IsBlockPost</c>).
    /// </summary>
    public bool TrainsCanCross { get; set; }

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
