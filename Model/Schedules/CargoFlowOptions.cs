namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Options that applies to freight cards directed by waybills.
/// </summary>
public sealed class CargoFlowOptions : TrainPartOptions
{
    /// <summary>
    /// Gets or sets the unique identifier for this cargo flow description within its
    /// timetable catalogue (see <see cref="Tellurian.Trains.Schedules.Model.Timetables.Timetable.CargoFlowOptions"/>).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the catalogue display name shown when picking a cargo flow description
    /// (for example "Coal to the harbour"). Several cargo flows may reference the same description;
    /// editing it here updates them all.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// If true, should instruct drivers/stations that the traindriver also should perform the shunting of arrived wagons.
    /// </summary>
    public bool AlsoShuntAfterArrival { get; set; }

    /// <summary>
    /// if true, hould instruct drivers/stations that the traindriver also should perform the shunting of departing wagons before train departure time.
    /// </summary>
    public bool AlsoShuntBeforeDeparture { get; set; }

    /// <summary>
    /// The ultimate origin of the wagons. Should in some cases override the train part's from-station.
    /// </summary>
    public ICollection<Origin> Origins { get; set; } = [];

    /// <summary>
    /// The destinations of freight wagons.
    /// </summary>
    public ICollection<Destination> Destinations { get; set; } = [];

    /// <summary>
    /// If true, overrides the <see cref="Destinations"/>
    /// </summary>
    public bool ToAllDestinations { get; set; }

    /// <summary>
    /// If true, no wagons should be brought from the train-part's from-station.
    /// </summary>
    /// <remarks>
    /// If true, the <see cref="AlsoShuntBeforeDeparture"/> should be ignored.
    /// Still, the train can bring wagons from the <see cref="Origins"/>
    /// </remarks>
    public bool BringsNoWagonsFromHere { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
