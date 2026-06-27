namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// A reusable description of where a cargo flow's wagons are routed: the destinations they are brought
/// to and, optionally, the origin stations whose wagons are forwarded. Held in the timetable catalogue
/// (<see cref="Tellurian.Trains.Schedules.Model.Timetables.Timetable.CargoFlowOptions"/>) and referenced
/// by one or more <see cref="CargoFlowTrainPart"/>s; editing a description updates every cargo flow that
/// uses it. Per-occurrence behaviour (where wagons are connected/disconnected, shunting, couple notes)
/// lives on <see cref="CargoFlowTrainPart"/>, not here.
/// </summary>
public sealed class CargoFlowOptions
{
    /// <summary>
    /// Gets or sets the unique identifier for this cargo flow description within its timetable catalogue.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the catalogue display name shown when picking a cargo flow description
    /// (for example "Coal to the harbour").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The ultimate origin of the wagons. Wagons gathered at these origin stations are forwarded by the
    /// cargo flow (in addition to, or instead of, wagons from the train part's from-station).
    /// </summary>
    public ICollection<Origin> Origins { get; set; } = [];

    /// <summary>
    /// The destinations of freight wagons.
    /// </summary>
    public ICollection<Destination> Destinations { get; set; } = [];

    /// <summary>
    /// If true, overrides the <see cref="Destinations"/> and brings wagons to all destinations.
    /// </summary>
    public bool ToAllDestinations { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
