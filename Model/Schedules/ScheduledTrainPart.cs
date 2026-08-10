using System.Globalization;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// A <see cref="ScheduledTrainPart"/> that is assigned to a vehicle <see cref="Schedule"/> and/or a
/// <see cref="DriverDuty"/>. It carries the per-part options describing how the assigned traction,
/// wagons or fixed-schedule cargo are handled over the segment. A part may carry several option kinds
/// at once; each slot is null when not applicable.
/// </summary>
public sealed class
    ScheduledTrainPart : TrainPart
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private ScheduledTrainPart() : base() { }

    /// <summary>
    /// Initializes a new instance of <see cref="ScheduledTrainPart"/> with the specified station calls.
    /// </summary>
    /// <param name="from">The departure station call.</param>
    /// <param name="to">The arrival station call.</param>
    /// <exception cref="ArgumentException">Thrown when the station calls are from different trains.</exception>
    public ScheduledTrainPart(StationCall from, StationCall to) : base(from, to) { }

    /// <summary>
    /// Gets or sets the foreign key to the vehicle schedule. Optional.
    /// </summary>
    public int? ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle schedule this train part is assigned to.
    /// </summary>
    public Schedule? Schedule { get; set; }

    /// <summary>
    /// Options applying when this part is operated by a traction unit (locomotive or trainset).
    /// Null when not applicable. A part may carry several option kinds at once.
    /// </summary>
    public TractionOptions? TractionOptions { get; set; }

    /// <summary>
    /// Options applying when this part carries non-traction rolling stock (wagons).
    /// Null when not applicable.
    /// </summary>
    public WagonSetOptions? WagonSetOptions { get; set; }

    /// <summary>
    /// Options applying when this part is a fixed-schedule cargo-only working.
    /// Null when not applicable.
    /// </summary>
    public CargoOnlyOptions? CargoOnlyOptions { get; set; }
}

/// <summary>
/// Provides extension methods for <see cref="ScheduledTrainPart"/>.
/// </summary>
public static class ScheduledTrainPartExtensions
{
    extension(ScheduledTrainPart trainPart)
    {
        /// <summary>
        /// Determines whether this train part overlaps in time with any of the specified train parts.
        /// Overlap only applies to scheduled parts (vehicle circulations and driver duties); a cargo
        /// flow is not subject to it.
        /// </summary>
        /// <remarks>
        /// The parts are compared over their <c>WorkingSpan</c>, so the preparation time at a train's
        /// origin and the finishing-up time at its destination count as occupied: a vehicle or driver
        /// still making one train ready cannot be working another.
        /// </remarks>
        /// <param name="otherTrainParts">The collection of train parts to check against.</param>
        /// <returns><c>true</c> if there is any overlap; otherwise, <c>false</c>.</returns>
        public bool IsOverlapping(IEnumerable<ScheduledTrainPart> otherTrainParts)
        {
            var span = trainPart.WorkingSpan;
            return otherTrainParts.Any(o => span.OverlapsInTime(o.WorkingSpan));
        }

        /// <summary>
        /// Creates <see cref="ICallNote">notes</see> for the departure station call at the train part's start station.
        /// </summary>
        public IEnumerable<ICallNote> DepartureNotes =>
            [.. trainPart.GeneratedDepartureNotes, .. trainPart.From.Notes];

        /// <summary>
        /// Only the notes this part <em>generates</em> for its departure — without the call's own
        /// persisted ones.
        /// </summary>
        /// <remarks>
        /// The split exists because a call assembles its notes from both families itself (see
        /// <c>DriverNotes</c> and <c>StationNotes</c>), and reading the persisted ones from here as well
        /// would print every one of them twice.
        /// </remarks>
        public IEnumerable<ICallNote> GeneratedDepartureNotes
        {
            get
            {
                List<ICallNote> result = [];
                trainPart.AddTractionUnitDepartureNotes(result);
                trainPart.AddWagonSetDepartureNotes(result);
                return result;
            }
        }

        /// <summary>
        /// Creates <see cref="ICallNote">notes</see> for the departure station call for traction units.
        /// </summary>
        public IEnumerable<ICallNote> TractionDepartureNotes
        {
            get
            {
                List<ICallNote> result = [];
                trainPart.AddTractionUnitDepartureNotes(result);
                result.AddRange(trainPart.From.Notes);
                return result;

            }
        }

        /// <summary>
        /// Creates <see cref="ICallNote">notes</see> for the arrival station call at the train part's end station.
        /// </summary>
        public IEnumerable<ICallNote> ArrivalNotes =>
            [.. trainPart.GeneratedArrivalNotes, .. trainPart.To.Notes];

        /// <summary>
        /// Only the notes this part <em>generates</em> for its arrival — without the call's own persisted
        /// ones. See <c>GeneratedDepartureNotes</c> for why the two are separable.
        /// </summary>
        public IEnumerable<ICallNote> GeneratedArrivalNotes
        {
            get
            {
                List<ICallNote> result = [];
                trainPart.AddTractionUnitArrivalNotes(result);
                return result;
            }
        }

        private void AddTractionUnitDepartureNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.TractionOptions;
            if (options is null) return;
            if (options.FromParking)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new FromParkingNote(so) { IsForDeparture = true }));
            }
            else if (options.HasCoupleNote)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new CoupleNote(so) { IsForDeparture = true }));
            }
            else if (options.DisplayUseNote)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new UseNote(so) { IsForDeparture = true }));
            }
            if (options.IsReinforcement)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new ReinforcementNote(so, trainPart) { DisplayOrder = 800, IsForDeparture = true }));

            }
        }

        private void AddTractionUnitArrivalNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.TractionOptions;
            if (options is null) return;
            if (options.ToParking)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new ToParkingNote(so) { IsForArrival = true }));
            }
            else if (options.HasUncoupleNote)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new UncoupleNote(so) { IsForArrival = true }));
            }
            // Not part of the chain above: uncoupling is what a circulating loco does first, so a part
            // asking for both wants both notes, in that order. One note whatever the consist — the whole
            // of it turns or circulates together.
            if (TurningNote(options) is { } turning) callNotes.Add(turning);
        }

        private void AddWagonSetDepartureNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.WagonSetOptions;
            if (options is null) return;
            if (options.HasCoupleNote)
            {
                callNotes.AddRange(trainPart.WagonSets
                    .Select(so => new CoupleNote(so) { IsForDeparture = true }));
            }
        }

        /// <summary>The vehicles working this part, resolved through the plan that owns its schedule.</summary>
        public IEnumerable<ScheduledObject> ScheduledObjects =>
            trainPart.Schedule?.Plan.ScheduledObjectsFor(trainPart) ?? [];

        /// <summary>The locomotives and trainsets hauling this part.</summary>
        public IEnumerable<ScheduledObject> TractionUnits =>
            trainPart.ScheduledObjects.Where(so => so.IsTraction);

        /// <summary>The wagonsets this part carries.</summary>
        public IEnumerable<ScheduledObject> WagonSets =>
            trainPart.ScheduledObjects.Where(so => so.IsWagonSet);

        /// <summary>
        /// The part written as the traction units working it followed by the times of its
        /// <c>WorkingSpan</c> — "MZ 5 Fullerup 14:51-&gt;Skovborg 15:05". Double-headed traction is joined
        /// with a plus sign, as it is written elsewhere.
        /// </summary>
        /// <remarks>
        /// For a message about a train hauled twice over. There the train is named already, so what
        /// <c>WorkingSpanText</c> leads with says nothing, while the one thing
        /// that tells the two parts apart — which locomotive works each — is missing. Two parts of the
        /// same train can run over the same stretch at the same times, and then the locomotive is all
        /// there is to tell them apart. A part whose traction cannot be resolved (it is detached from its
        /// plan) shows the span alone rather than an empty name.
        /// <para>
        /// Where the train is <em>not</em> already named, <c>WorkingSpanText</c> is the one to use.
        /// </para>
        /// </remarks>
        public string TractionWorkingSpanText
        {
            get
            {
                var (from, to) = trainPart.WorkingSpan;
                var span = string.Format(CultureInfo.CurrentCulture, "{0} {1}->{2} {3}",
                    trainPart.From.OperationLocation, from.HHMM(), trainPart.To.OperationLocation, to.HHMM());
                // Through the schedule this part belongs to, not through TractionUnits: a part is equal to
                // any part over the same two calls, so two schedules covering one leg each resolve to both
                // locomotives — and telling those two apart is the whole point here.
                var traction = string.Join(" + ", (trainPart.Schedule?.Vehicles ?? [])
                    .Where(v => v.IsTraction)
                    .Select(v => v.Designation));
                return traction.Length == 0 ? span : $"{traction} {span}";
            }
        }
    }

    /// <summary>
    /// The one note for what has to be done with the traction after arrival so the train can leave the
    /// other way: circulate it to the other end of the train, turn it, or both. Null when neither is
    /// asked for.
    /// </summary>
    /// <remarks>
    /// Both flags together give a single note, not two. The two moves are one errand — the loco leaves
    /// the train, goes to the turntable and comes back on the other end — and stating them separately
    /// reads as two independent movements.
    /// </remarks>
    private static GeneratedNote? TurningNote(TractionOptions options) =>
        options switch
        {
            { TurnLoco: true, ReverseLoco: true } => new TurnAndCirculateNote { IsForArrival = true },
            { TurnLoco: true } => new TurnNote { IsForArrival = true },
            { ReverseLoco: true } => new CirculateNote { IsForArrival = true },
            _ => null,
        };
}
