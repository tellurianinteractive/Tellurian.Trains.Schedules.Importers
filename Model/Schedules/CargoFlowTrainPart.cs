using System.Net;
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

    /// <summary>
    /// A cargo flow is an entity keyed by <see cref="TrainPart.Id"/>. Several cargo flows on the same
    /// train may legitimately share the same from/to span (different cargo, position, or operations),
    /// so identity — not the from/to geometry used by <see cref="TrainPart.Equals(TrainPart)"/> —
    /// distinguishes them. This keeps adding and deleting flows with identical endpoints correct.
    /// </summary>
    public override bool Equals(object? obj) => obj is CargoFlowTrainPart other && other.Id == Id;

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();
}

/// <summary>
/// Provides extension methods for <see cref="CargoFlowTrainPart"/>.
/// </summary>
public static class CargoFlowTrainPartExtensions
{
    extension(CargoFlowTrainPart trainPart)
    {
        /// <summary>
        /// The time the flow's wagons are connected: the train's departure from the from-call — or, on a
        /// shunting task, the arrival at which the working starts. Not <c>TrainPart.Departure</c>, which
        /// knows nothing of the shunting task's inverted ends.
        /// </summary>
        public Time ConnectTime => trainPart.Train.CargoFlowConnectTime(trainPart.From);

        /// <summary>
        /// The time the flow's wagons are disconnected: the train's arrival at the to-call — or, on a
        /// shunting task, the departure at which the working ends.
        /// </summary>
        public Time DisconnectTime => trainPart.Train.CargoFlowDisconnectTime(trainPart.To);

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
                    result.Add(new CargoFlowDestinationNote(trainPart) { IsForDeparture = true });
                return result;
            }
        }

        /// <summary>
        /// Whether this flow's wagons are worked into the station's sidings or out of them, on the
        /// shunting task the flow belongs to. Meaningless on a travelling train, which shunts nothing.
        /// </summary>
        /// <remarks>
        /// Derived, not stored. The flow's <see cref="CargoFlowOptions"/> already says where the wagons go,
        /// and that answers the question: a flow whose destination is the very station being worked is
        /// carrying wagons that have <em>arrived</em> here, so they go out to the cargo customers; a flow
        /// bound anywhere else is gathering wagons <em>from</em> those customers for a departing train.
        /// A flow to all destinations, or to several of which this station is only one, is bound elsewhere
        /// too, and so gathers.
        /// </remarks>
        public ShuntingWork ShuntingWork =>
            trainPart.CargoFlowOptions is { ToAllDestinations: false, Destinations.Count: > 0 } options &&
            options.Destinations.All(d => d.Location.Equals(trainPart.To.OperationLocation))
                ? ShuntingWork.ToCargoCustomers
                : ShuntingWork.FromCargoCustomers;

        /// <summary>
        /// Where this flow's wagons came from, as plain text: the forwarded origin locations joined with
        /// commas, or empty when the flow names none.
        /// </summary>
        /// <remarks>
        /// The counterpart of <c>ToText</c>. There is no "all origins" wording to fall back on the way
        /// there is for destinations: a flow that names no origin says nothing about where its wagons came
        /// from, and the note it appears in leaves the clause out rather than inventing one.
        /// </remarks>
        public string FromText =>
            string.Join(", ", (trainPart.CargoFlowOptions?.Origins ?? []).Select(o => o.Location.Name));

        /// <summary>
        /// Where this flow's wagons came from, as markup. The location names are planner-entered text
        /// embedded in note markup, so they are encoded; an origin carries nothing that renders as a chip.
        /// </summary>
        public string FromHtml =>
            string.Join(", ", (trainPart.CargoFlowOptions?.Origins ?? [])
                .Select(o => WebUtility.HtmlEncode(o.Location.Name)));

        /// <summary>
        /// The instruction this flow gives the person working the shunting task it belongs to: take the
        /// wagons that arrived here out to the cargo customers, or fetch the wagons bound elsewhere in
        /// from them. Empty for a flow on a travelling train, and for one with no routing to state.
        /// </summary>
        /// <remarks>
        /// Attached to the <em>arrival</em> half of the task's single call, which shows the time the work
        /// starts — the driver reads the instruction when they begin, not when they are finished.
        /// </remarks>
        public IEnumerable<ICallNote> ShuntingNotes
        {
            get
            {
                if (!trainPart.Train.IsShuntingTask || trainPart.CargoFlowOptions is null) return [];
                return
                [
                    // Sorted early: on a shunting task the instruction is not a remark about the working,
                    // it is the working, so nothing else on the call should be read before it.
                    trainPart.ShuntingWork == ShuntingWork.ToCargoCustomers
                        ? new ShuntArrivingWagonsNote(trainPart) { IsForArrival = true, IsShuntingNote = true, DisplayOrder = 200 }
                        : new FetchDepartingWagonsNote(trainPart) { IsForArrival = true, IsShuntingNote = true, DisplayOrder = 200 },
                ];
            }
        }

        /// <summary>
        /// Where this flow's wagons go, as plain text: the destinations joined with commas, or the
        /// "all destinations" text when the flow is unrestricted.
        /// </summary>
        /// <remarks>
        /// The places only, without each destination's load limit. A limit belongs to the planner's view
        /// of the flow, not to the instruction read on the spot: the duty booklet's cargo block gives it a
        /// column of its own, and a note stating it beside the place would only lengthen a sentence the
        /// driver reads while working.
        /// </remarks>
        public string ToText
        {
            get
            {
                var options = trainPart.CargoFlowOptions;
                return options.ToAllDestinations ? NoteResources.AllDestinations :
                        string.Join(", ", options.Destinations.Select(d => d.PlaceText));
            }
        }

        /// <summary>
        /// Where this flow's wagons go, as markup, with destination regions rendered as coloured chips.
        /// This is the "Wagons to" statement the cargo notes carry. Limits are left off, as in
        /// <c>ToText</c>.
        /// </summary>
        public string ToHtml
        {
            get
            {
                var options = trainPart.CargoFlowOptions;
                return options.ToAllDestinations ? NoteResources.AllDestinations : string.Join(", ", options.Destinations.Select(d => d.PlaceHtml.Value));
            }
        }
    }
}
