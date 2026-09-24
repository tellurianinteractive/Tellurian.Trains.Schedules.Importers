namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note telling the station's dispatcher to take the cargo flow's freight wagons off the train here,
/// naming every destination the flow serves.
/// </summary>
/// <remarks>
/// <para>
/// The arrival counterpart of <see cref="CargoFlowDestinationNote"/>, and derived the same way: from
/// <see cref="CargoFlowTrainPart.HasUncoupleNote"/> at the call the flow's wagons are disconnected at.
/// The destinations are what makes it actionable — the wagons coming off are sorted by where they go on
/// to, and a destination beyond this station is what says which train they are put over to.
/// </para>
/// <para>
/// For the dispatcher only. The loco driver's booklet states the flow in its cargo block, with the load
/// limits and wagon classes that belong to planning it; repeating it as a note would say the same thing
/// twice on the page it is already on.
/// </para>
/// </remarks>
/// <param name="Part">The cargo flow whose wagons come off here.</param>
public sealed record CargoFlowUncoupleNote(CargoFlowTrainPart Part) : GeneratedNote;
