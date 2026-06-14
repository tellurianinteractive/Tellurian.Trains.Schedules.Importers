namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Options tha applies to no-traction rolling stock.
/// </summary>
public sealed class NonTractionOptions : TrainPartOptions
{
    /// <summary>
    /// Optional specification of wagons in train part.
    /// </summary>
    public ICollection<Wagon> WagonGroup { get; set; } = [];
    /// <summary>
    /// The wagon groups overall order in the train.
    /// </summary>
    public int OrderInTrain { get; set; }
}

/// <summary>
/// Detailed info about specific wagons in a train.
/// </summary>
/// <param name="OrderInTrain">The position in train in forward direction. Order should be reversed in backwards direction.</param>
/// <param name="Class">Type of wagon according standard clas definitions.</param>
public record Wagon(int OrderInTrain, string Class)
{
    /// <summary>
    /// Optional indivdual number of wagon.
    /// </summary>
    public string? Number { get; set; }
}

/// <summary>
/// Options that applies to freight cards directed by waybills.
/// </summary>
public sealed class CargoFlowOptions : TrainPartOptions
{
    /// <summary>
    /// If true, should instruct drivers/stations that the traindriver also should perform the shunting of arrived wagons.
    /// </summary>
    public bool AlsoShuntAfterArrival { get; set; }
    /// <summary>
    /// if true, hould instruct drivers/stations that the traindriver also should perform the shunting of departing wagons before train departure time.
    /// </summary>
    public bool AksoShuntBeforeDeparture { get; set; }

    /// <summary>
    /// The ultimate origin of the wagons. Should in some cases override the train part's from-station.
    /// </summary>
    public Station? TransferOrigin { get; set; }

    /// <summary>
    /// The ultimate destination of the wagons. Should in some cases override the train part's to-station.
    /// </summary>
    public Station? TransferDestination { get; set; }

    /// <summary>
    /// If true, the destination note should contain the regions at train part's to-station, or <see cref="TransferDestination"/> if given.
    /// </summary>
    public bool AndRegions { get; set; }
    /// <summary>
    /// If true, the destination note should contain 'and beyond', meaning all operation locations in the layout beyond the train part's to-station, or <see cref="TransferDestination"/> if given.
    /// </summary>
    public bool AndBeyond { get; set; }

    /// <summary>
    /// If true, the destination note should contain 'and local destinations', meaning all operation locations served by freight trains from the train part's to-station, or <see cref="TransferDestination"/> if given.
    /// </summary>
    public bool AndLocalDestinations { get; set; }

    /// <summary>
    /// If true, overrides <see cref="AndRegions"/>, <see cref="AndBeyond"/> and <see cref="AndLocalDestinations"/>, just noting 'to all destinations'.
    /// </summary>
    public bool ToAllDestinations { get; set; }
}

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
