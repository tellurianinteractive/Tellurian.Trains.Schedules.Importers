namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Options tha applies to no-traction rolling stock.
/// </summary>
/// <remarks>
/// The individual wagons of a wagonset live on <see cref="ScheduledObject.Units"/> (the rake shared across
/// the whole schedule), not on the per-train-part options.
/// </remarks>
public sealed class WagonSetOptions : TrainPartOptions
{
    /// <summary>
    /// The wagon groups overall order in the train.
    /// </summary>
    public int OrderInTrain { get; set; }

    /// <summary>
    /// If true, the wagons are loaded where this part ends. Only meaningful for a wagonset of kind
    /// <see cref="WagonSetKind.Cargo"/>. Generates an arrival note.
    /// </summary>
    public bool LoadWagons { get; set; }

    /// <summary>
    /// If true, the wagons are unloaded where this part ends and sent back to where their schedule last
    /// loaded them. Only meaningful for a wagonset of kind <see cref="WagonSetKind.Cargo"/>. Generates an
    /// arrival note.
    /// </summary>
    public bool UnloadWagons { get; set; }

    /// <summary>
    /// If true, the cargo is containers. Only changes the wording of the note said where the wagons are
    /// both unloaded and loaded again.
    /// </summary>
    public bool IsContainers { get; set; }

    /// <summary>
    /// Where the wagonset comes from before departure: from stabling, lifted on to the track, or
    /// (<see cref="TractionLayover.None"/>) from a track of the station. Anything but none replaces the
    /// couple note with an instruction to bring the wagonset to the departure track.
    /// </summary>
    /// <remarks>Must match the <see cref="ToLayover"/> of the part the wagonset worked before (rule S6).</remarks>
    public TractionLayover FromLayover { get; set; }

    /// <summary>
    /// Where the wagonset goes after arrival: to stabling, lifted off the track, or
    /// (<see cref="TractionLayover.None"/>) it stays on a track of the station.
    /// </summary>
    /// <remarks>Must match the <see cref="FromLayover"/> of the part the wagonset works next (rule S6).</remarks>
    public TractionLayover ToLayover { get; set; }
}
