using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A block post, junction or crossing place worked by signals. A train never stops here, whatever its call says, and
/// nothing is exchanged.
/// </summary>
public sealed class SignalControlledLocation : OperationLocation
{
    /// <summary>Creates a signal controlled location.</summary>
    public SignalControlledLocation(int id, string name, string signature) : base(id, name, signature) { }

    /// <summary>Creates one with no name or signature yet, to be given with an object initializer.</summary>
    [JsonConstructor]
    public SignalControlledLocation() : base() { }

    /// <summary>Trains can cross here: it has a passing loop.</summary>
    public bool TrainsCanCross { get; set; }

    /// <summary>Always false: nothing is exchanged where trains never stop.</summary>
    public override bool HasPassengerExchange { get => false; set { } }

    /// <summary>Always false: nothing is exchanged where trains never stop.</summary>
    public override bool HasCargoExchange { get => false; set { } }
}
