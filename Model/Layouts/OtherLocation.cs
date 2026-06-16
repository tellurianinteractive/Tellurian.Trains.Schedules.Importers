using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// An unmanned and not signal controlled <see cref="OperationLocation"/>.
/// </summary>
public class OtherLocation : OperationLocation
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="signature"></param>
    public OtherLocation(int id, string name, string signature) : base(id, name, signature) { }

    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected OtherLocation() : base() { }

    /// <summary>
    /// Always false for other locations, as they lack the infrastructure for loco runaround.
    /// </summary>
    public override bool IsChangingTrainDirectionPossible
    {
        get => false;
        set { } // Ignore attempts to set
    }
}
