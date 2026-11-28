using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>Another place where trains can stop, such as an unstaffed halt: no cargo, and trains can't change direction.</summary>
public sealed class OtherLocation : OperationLocation
{
    /// <summary>Creates another location.</summary>
    public OtherLocation(int id, string name, string signature) : base(id, name, signature) { }

    /// <summary>Creates one with no name or signature yet, to be given with an object initializer.</summary>
    [JsonConstructor]
    public OtherLocation() : base() { }

    /// <summary>Always false.</summary>
    public override bool IsChangingTrainDirectionPossible { get => false; set { } }

    /// <summary>Always false: a halt has no cargo customers.</summary>
    public override bool HasCargoExchange { get => false; set { } }
}
