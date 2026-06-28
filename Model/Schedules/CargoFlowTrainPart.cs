using System.Text.Json.Serialization;
using NoteResources = Tellurian.Trains.Schedules.Model.Resources.Notes;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// A waybill-directed cargo flow over a segment of a train: wagons connected at the part's from-call and
/// disconnected at its to-call. Unlike <see cref="ScheduledTrainPart"/> it is not assigned to a vehicle
/// schedule or driver duty — it belongs directly to its <see cref="TrainPart.Train"/> (see
/// <c>Train.CargoFlows</c>). The routing (where wagons go, which origins are forwarded) is the reusable
/// <see cref="CargoFlowOptions"/> it references; the per-occurrence operational behaviour lives here.
/// </summary>
public sealed class CargoFlowTrainPart : TrainPart
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private CargoFlowTrainPart() : base() { }

    /// <summary>
    /// Initializes a new instance of <see cref="CargoFlowTrainPart"/> with the specified station calls.
    /// </summary>
    /// <param name="from">The departure call where wagons are connected.</param>
    /// <param name="to">The arrival call where wagons are disconnected.</param>
    public CargoFlowTrainPart(StationCall from, StationCall to) : base(from, to) { }

    /// <summary>
    /// Gets or sets the position of this cargo flow within the train (1 = front). Several cargo flows on
    /// the same train may share a position.
    /// </summary>
    public int PositionInTrain { get; set; } = 1;

    /// <summary>
    /// If true, instructs that the train driver also performs the shunting of arrived wagons.
    /// </summary>
    public bool AlsoShuntAfterArrival { get; set; }

    /// <summary>
    /// If true, instructs that the train driver also performs the shunting of departing wagons before
    /// the train's departure time.
    /// </summary>
    public bool AlsoShuntBeforeDeparture { get; set; }

    /// <summary>
    /// If true, no wagons are brought from the from-call's station; the flow still forwards wagons from
    /// its <see cref="CargoFlowOptions"/> origins.
    /// </summary>
    public bool BringsNoWagonsFromHere { get; set; }

    /// <summary>
    /// If true, a note tells staff to couple the wagons to the train at the from-call.
    /// </summary>
    public bool HasCoupleNote { get; set; } = true;

    /// <summary>
    /// If true, a note tells staff to uncouple the wagons from the train at the to-call.
    /// </summary>
    public bool HasUncoupleNote { get; set; } = true;

    /// <summary>
    /// Gets or sets the foreign key to the referenced cargo flow description in the timetable catalogue.
    /// </summary>
    public int CargoFlowOptionsId { get; set; }

    /// <summary>
    /// Gets or sets the reusable cargo flow description this flow uses (a catalogue entry on the
    /// timetable). Editing the description affects every cargo flow that references it.
    /// </summary>
    public CargoFlowOptions CargoFlowOptions { get; set; } = default!;
}

/// <summary>
/// Provides extension methods for <see cref="CargoFlowTrainPart"/>.
/// </summary>
public static class CargoFlowTrainPartExtensions
{
    extension(CargoFlowTrainPart trainPart)
    {
        /// <summary>
        /// Creates the <see cref="ICallNote">notes</see> shown at the from-call: a destination note
        /// listing where the cargo flow brings wagons.
        /// </summary>
        public IEnumerable<ICallNote> DepartureNotes
        {
            get
            {
                List<ICallNote> result = [];
                if (trainPart.HasCoupleNote && trainPart.CargoFlowOptions is not null)
                    result.Add(new CargoFlowDestinationNote(trainPart));
                return result;
            }
        }

        internal string ToPlainText
        {
            get
            {
                var options = trainPart.CargoFlowOptions;
                return options.ToAllDestinations ? NoteResources.AllDestinations :
                        string.Join(", ", options.Destinations.Select(d => d.ToString()));
            }
        }

        internal string ToHtml
        {
            get
            {
                var options = trainPart.CargoFlowOptions;
                return options.ToAllDestinations ? NoteResources.AllDestinations : string.Join(", ", options.Destinations.Select(d => d.ToHtmlMarkup.Value));
            }
        }
    }
}
