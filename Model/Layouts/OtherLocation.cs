using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// An unmanned and not signal controlled <see cref="OperationLocation"/>, e.g. an unstaffed halt.
/// </summary>
/// <remarks>
/// A train may stop here when the call says so (<see cref="Timetables.StationCall.IsStop"/>), unlike a
/// <see cref="SignalControlledLocation"/> where it never stops. Passenger exchange stays configurable
/// (a halt can board and alight passengers) but there is no cargo exchange and no train direction change.
/// </remarks>
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

    /// <summary>
    /// Always false: an other location lacks the infrastructure to exchange cargo wagons.
    /// Passenger exchange (<see cref="OperationLocation.HasPassengerExchange"/>) stays configurable,
    /// e.g. for an unstaffed halt.
    /// </summary>
    public override bool HasCargoExchange
    {
        get => false;
        set { } // Ignore attempts to set
    }
}
