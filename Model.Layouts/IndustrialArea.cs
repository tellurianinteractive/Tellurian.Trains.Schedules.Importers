using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>A freight-only area: cargo is always exchanged, passengers never, and trains can't change direction.</summary>
public sealed class IndustrialArea : OperationLocation
{
    /// <summary>Creates an industrial area.</summary>
    public IndustrialArea(int id, string name, string signature) : base(id, name, signature) { }

    /// <summary>Creates one with no name or signature yet, to be given with an object initializer.</summary>
    [JsonConstructor]
    public IndustrialArea() : base() { }

    /// <summary>Always false.</summary>
    public override bool IsChangingTrainDirectionPossible { get => false; set { } }

    /// <summary>Always false: an industrial area takes no passengers.</summary>
    public override bool HasPassengerExchange { get => false; set { } }

    /// <summary>Always true: that is what an industrial area is for.</summary>
    public override bool HasCargoExchange { get => true; set { } }

    /// <summary>
    /// The industrial area has signals. Entered by the user until a drawing can tell it
    /// (<c>ModelSurfaceAnalysis.md</c> 4.2.1).
    /// </summary>
    public bool HasSignals { get; set; }
}
