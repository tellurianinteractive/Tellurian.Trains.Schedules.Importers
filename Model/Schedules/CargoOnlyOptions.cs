namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Options for cargo only that runs in a fixed schedule.
/// </summary>
public sealed class CargoOnlyOptions : TrainPartOptions
{
    /// <summary>
    /// Optional name of type of cargo.
    /// </summary>
    public string? CargoName { get; set; }
    /// <summary>
    /// If true, the cargo should be loaded at the train part's from-station.
    /// </summary>
    public bool Load => base.HasCoupleNote;

    /// <summary>
    /// If true, the cargo should be unloaded at the train part's to-station.
    /// </summary>
    public bool Unload => base.HasUncoupleNote;
}
