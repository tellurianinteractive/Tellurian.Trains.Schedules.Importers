namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note on a shunting task telling the driver to take the wagons that arrived at this station out to its
/// cargo customers, naming where those wagons came from (the flow's origins).
/// </summary>
/// <remarks>
/// Produced only where the flow's wagons are bound for the very station the task is worked at; the other
/// case is <see cref="FetchDepartingWagonsNote"/>. See <c>CargoFlowTrainPartExtensions.ShuntingWork</c>.
/// </remarks>
public sealed record ShuntArrivingWagonsNote(CargoFlowTrainPart Part) : GeneratedNote;

/// <summary>
/// Note on a shunting task telling the driver to fetch the wagons bound elsewhere in from this station's
/// cargo customers, naming where those wagons are going (the flow's destinations).
/// </summary>
/// <remarks>
/// The counterpart of <see cref="ShuntArrivingWagonsNote"/>: the wagons leave the station rather than
/// arrive at it, so the flow's destinations are what the driver is told.
/// </remarks>
public sealed record FetchDepartingWagonsNote(CargoFlowTrainPart Part) : GeneratedNote;
