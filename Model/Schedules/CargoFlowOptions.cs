using NoteResources = Tellurian.Trains.Schedules.Model.Resources.Notes;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// A reusable description of where a cargo flow's wagons are routed: the destinations they are brought
/// to and, optionally, the origin locations whose wagons are forwarded. Held in the timetable catalogue
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
    /// Optional UIC wagon-class letters that limit which wagons the cargo flow brings, e.g. "U,Z" to
    /// bring only those classes. Empty means any wagon class. Used both to filter and to describe the
    /// flow (it leads the routing summary, see <see cref="DestinationsSummary"/> consumers).
    /// </summary>
    public string OnlyWagonClasses { get; set; } = string.Empty;

    /// <summary>
    /// The ultimate origin of the wagons. Wagons gathered at these origin locations are forwarded by the
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

    /// <summary>
    /// The comma-separated destination location names, for compact display in lists and drop-downs.
    /// Empty when <see cref="ToAllDestinations"/> is set (use <see cref="DestinationsSummary"/> for that).
    /// </summary>
    public string DestinationLocationNames => string.Join(", ", Destinations.Select(d => d.Location.Name));

    /// <summary>
    /// The comma-separated origin location names, for compact display in lists.
    /// </summary>
    public string OriginLocationNames => string.Join(", ", Origins.Select(o => o.Location.Name));

    /// <summary>
    /// A short summary of where this cargo flow's wagons go: the localised "all destinations" text when
    /// <see cref="ToAllDestinations"/> is set, otherwise the <see cref="DestinationLocationNames"/>.
    /// </summary>
    public string DestinationsSummary => ToAllDestinations ? NoteResources.AllDestinations : DestinationLocationNames;

    /// <inheritdoc/>
    public override string ToString() => OnlyWagonClasses;
}
