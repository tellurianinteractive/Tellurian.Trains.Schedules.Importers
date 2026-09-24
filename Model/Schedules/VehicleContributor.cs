using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// One participant bringing a rolling stock item to the meeting. The first contributor of a
/// <see cref="VehicleContribution"/> is its primary one, who brings the unit and sets it up on the layout;
/// every later one brings a spare.
/// </summary>
public sealed class VehicleContributor
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private VehicleContributor()
    {
        Note = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="VehicleContributor"/>.
    /// </summary>
    /// <param name="participantId">The <see cref="Participant.Id"/> of the participant bringing the unit.</param>
    /// <param name="dccAddress">The unit's DCC address, or <c>null</c> for a unit that has none.</param>
    /// <param name="note">An optional note about what this participant brings.</param>
    public VehicleContributor(int participantId, int? dccAddress = null, string? note = null)
    {
        ParticipantId = participantId;
        DccAddress = dccAddress;
        Note = note?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the <see cref="Participant.Id"/> of the participant bringing the unit, resolved through
    /// the plan's participant catalogue.
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Gets or sets the DCC address of the unit this participant brings. Every traction unit must have one,
    /// the spares included, since each of them is driven on the layout; <see cref="DccAddresses.ToBeProvided"/>
    /// (zero) says the owner is still to provide it. <c>null</c> for a wagonset, which is not driven, and for
    /// a traction unit whose address has not been entered.
    /// </summary>
    public int? DccAddress { get; set; }

    /// <summary>
    /// Gets or sets a note about what this participant brings, e.g. that a spare arrives on the second day.
    /// Empty when there is none.
    /// </summary>
    public string Note { get; set; }
}
