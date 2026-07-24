using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// An <see cref="OperationLocation"/> that serves cargo only.
/// </summary>
public class IndustrialArea : OperationLocation
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="signature"></param>
    public IndustrialArea(int id, string name, string signature) : base(id, name, signature) { }

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected IndustrialArea() : base() { }

    /// <summary>
    /// Always false for industrial areas, as they lack the infrastructure for loco runaround.
    /// </summary>
    public override bool IsChangingTrainDirectionPossible
    {
        get => false;
        set { } // Ignore attempts to set
    }

    /// <summary>
    /// Always false for an industrial area.
    /// </summary>
    public override bool HasPassengerExchange
    {
        get => false;
        set { } // Ignore attempts to set
    }

    /// <summary>
    /// Always true: an industrial area always have facilities for haldning cargo wagons.
    /// </summary>
    public override bool HasCargoExchange
    {
        get => true;
        set { } // Ignore attempts to set
    }
}
