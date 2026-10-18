using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// What kind of vehicle an <see cref="OtherVehicle"/> is.
/// </summary>
public enum OtherVehicleType
{
    /// <summary>A locomotive.</summary>
    Locomotive,
    /// <summary>A trainset.</summary>
    Trainset,
    /// <summary>A wagonset.</summary>
    Wagonset,
    /// <summary>Any other vehicle that has to be registered.</summary>
    Other,
}

/// <summary>
/// A vehicle a participant brings to the meeting that has to be registered but takes no part in the operation:
/// it is not scheduled and is not a shunter. Known by its <see cref="Identity"/> alone, without a day, a station
/// or a time. Kept in <see cref="Plan.OtherVehicles"/>.
/// </summary>
public sealed class OtherVehicle
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private OtherVehicle()
    {
        Identity = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="OtherVehicle"/>.
    /// </summary>
    /// <param name="id">The identifier, unique within the plan's other vehicles.</param>
    /// <param name="type">What kind of vehicle it is.</param>
    /// <param name="identity">Free text saying which vehicle it is; surrounding white space is removed.</param>
    /// <param name="participantId">The <see cref="Participant.Id"/> of the participant bringing it.</param>
    /// <param name="dccAddress">Its DCC address, or <c>null</c> for none.</param>
    public OtherVehicle(int id, OtherVehicleType type, string identity, int participantId, int? dccAddress = null)
    {
        Id = id;
        Type = type;
        Identity = identity.Trim();
        ParticipantId = participantId;
        DccAddress = dccAddress;
    }

    /// <summary>
    /// Gets or sets the identifier, unique within the plan's other vehicles.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets what kind of vehicle it is.
    /// </summary>
    public OtherVehicleType Type { get; set; }

    /// <summary>
    /// Gets or sets free text saying which vehicle it is, e.g. "Rc 1328 in red livery". Stored trimmed.
    /// </summary>
    public string Identity
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the <see cref="Participant.Id"/> of the participant bringing the vehicle.
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's number. <c>null</c> when none is given; stored trimmed, blank is none.
    /// </summary>
    public string? VehicleNumber
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Gets or sets the vehicle's DCC address; <see cref="DccAddresses.ToBeProvided"/> (zero) says the owner is
    /// still to provide it. <c>null</c> when it has none.
    /// </summary>
    public int? DccAddress { get; set; }
}

/// <summary>
/// Provides extension members for <see cref="OtherVehicle"/> and <see cref="OtherVehicleType"/>.
/// </summary>
public static class OtherVehicleExtensions
{
    extension(OtherVehicleType type)
    {
        /// <summary>
        /// Whether a vehicle of this type is driven on the layout and so must have a DCC address: a
        /// locomotive or a trainset.
        /// </summary>
        public bool NeedsDccAddress => type is OtherVehicleType.Locomotive or OtherVehicleType.Trainset;

        /// <summary>
        /// Whether <paramref name="address"/> may be stored for a vehicle of this type.
        /// </summary>
        /// <param name="address">The address, or <c>null</c> for none.</param>
        public bool AcceptsDccAddress(int? address) => type switch
        {
            OtherVehicleType.Wagonset => address is null,
            OtherVehicleType.Other => address is null || DccAddresses.IsValid(address.Value),
            _ => address is { } value && DccAddresses.IsValid(value),
        };
    }

    extension(OtherVehicle vehicle)
    {
        /// <summary>
        /// Whether <paramref name="address"/> may be stored for the vehicle: a valid address (see
        /// <see cref="DccAddresses.IsValid"/>) for a locomotive or a trainset, a valid one or none for any
        /// other vehicle, and none at all for a wagonset.
        /// </summary>
        /// <param name="address">The address, or <c>null</c> for none.</param>
        public bool AcceptsDccAddress(int? address) =>
            vehicle.Type.AcceptsDccAddress(address);
    }
}
