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

    /// <summary>
    /// The track the vehicles working this part stand on before it departs, where that is another track
    /// than the one the train departs from; null where they are on the train's own track. They are then
    /// fetched from it before departure.
    /// </summary>
    /// <remarks>
    /// Only a track of the location the part departs from means anything; see <c>OtherFromTrack</c>,
    /// which is what reads it.
    /// </remarks>
    public StationTrack? FromTrack { get; set; }

    /// <summary>
    /// The track the vehicles working this part are put on after it arrives, where that is another track
    /// than the one the train arrives at; null where they stay on the train's own track.
    /// </summary>
    /// <remarks>
    /// Only a track of the location the part arrives at means anything; see <c>OtherToTrack</c>, which
    /// is what reads it.
    /// </remarks>
    public StationTrack? ToTrack { get; set; }
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
                trainPart.AddFromOtherTrackNotes(result);
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
                trainPart.AddWagonSetArrivalNotes(result);
                trainPart.AddToOtherTrackNotes(result);
                return result;
            }
        }

        /// <summary>
        /// The track the vehicles working this part are fetched from before it departs, or null where they
        /// stand on the train's own track. <see cref="ScheduledTrainPart.FromTrack"/> counts only where it
        /// is another track of the location the part departs from.
        /// </summary>
        /// <remarks>
        /// A track somewhere else says nothing about this part. It is what is left behind when the call
        /// the part departs from is moved to another location, and ignoring it here is what keeps it from
        /// sending a vehicle to a track at a station the part never calls at.
        /// </remarks>
        public StationTrack? OtherFromTrack => OtherTrackAt(trainPart.FromTrack, trainPart.From);

        /// <summary>
        /// The track the vehicles working this part are put on after it arrives, or null where they stay
        /// on the train's own track. <see cref="ScheduledTrainPart.ToTrack"/> counts only where it is
        /// another track of the location the part arrives at; see <c>OtherFromTrack</c>.
        /// </summary>
        public StationTrack? OtherToTrack => OtherTrackAt(trainPart.ToTrack, trainPart.To);

        /// <summary>
        /// Forgets the other tracks that are no longer at the location the part departs from or arrives
        /// at, so that a part moved to another station and back does not bring back a track the planner
        /// last saw it lose.
        /// </summary>
        internal void ForgetOtherTracksElsewhere()
        {
            if (trainPart.FromTrack is { } from && !IsTrackAt(from, trainPart.From)) trainPart.FromTrack = null;
            if (trainPart.ToTrack is { } to && !IsTrackAt(to, trainPart.To)) trainPart.ToTrack = null;
        }

        private void AddTractionUnitDepartureNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.TractionOptions;
            if (options is null) return;
            // Fetching the units from their other track is what brings them to the train, so that note
            // says all these would: there is nothing left to use, to couple, to bring from stabling or to lift on.
            if (trainPart.OtherFromTrack is null) trainPart.AddTractionUnitJoiningNotes(options, callNotes);
            if (options.IsReinforcement)
            {
                var settings = trainPart.PlanSessionsSettings;
                callNotes.AddRange(trainPart.Working(trainPart.TractionUnits)
                    .Select(u => new ReinforcementNote(u.Vehicle, trainPart)
                    { DisplayOrder = 800, IsForDeparture = true, Sessions = u.Sessions, Settings = settings }));
            }
        }

        private void AddTractionUnitJoiningNotes(TractionOptions options, List<ICallNote> callNotes)
        {
            var settings = trainPart.PlanSessionsSettings;
            var units = trainPart.Working(trainPart.TractionUnits);
            if (options.FromLayover == TractionLayover.Stabling)
            {
                callNotes.AddRange(units
                    .Select(u => new FromParkingNote(u.Vehicle) { IsForDeparture = true, Sessions = u.Sessions, Settings = settings }));
            }
            else if (options.FromLayover == TractionLayover.LiftedOff)
            {
                callNotes.AddRange(units
                    .Select(u => new LiftOnNote(u.Vehicle) { IsForDeparture = true, Sessions = u.Sessions, Settings = settings }));
            }
            else if (options.HasCoupleNote)
            {
                callNotes.AddRange(units
                    .Select(u => new CoupleNote(u.Vehicle) { IsForDeparture = true, Sessions = u.Sessions, Settings = settings }));
            }
            else if (options.DisplayUseNote)
            {
                callNotes.AddRange(units
                    .Select(u => new UseNote(u.Vehicle) { IsForDeparture = true, Sessions = u.Sessions, Settings = settings }));
            }
        }

        private void AddTractionUnitArrivalNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.TractionOptions;
            if (options is null) return;
            // Putting the units on their other track takes them off the train, so that note says all these
            // would: there is nothing left to uncouple, to drive to stabling or to lift off.
            if (trainPart.OtherToTrack is null) trainPart.AddTractionUnitLeavingNotes(options, callNotes);
            // Not part of the chain above: uncoupling is what a circulating loco does first, so a part
            // asking for both wants both notes, in that order. One note whatever the consist — the whole
            // of it turns or circulates together.
            if (trainPart.TurningNote(options) is not { } turning) return;
            var units = trainPart.Working(trainPart.TractionUnits);
            callNotes.Add(turning with
            {
                // It names no traction unit, so it is qualified by the sessions any of them is turned or
                // run round on. A part no vehicle works yet still gets the note — that is what keeps what
                // the planner asked for standing — and there is then nothing to qualify it by.
                Sessions = units.Count == 0 ? null : units.Select(u => u.Sessions).SharedPartOf(trainPart.Train.Sessions),
                Settings = trainPart.PlanSessionsSettings,
            });
        }

        private void AddTractionUnitLeavingNotes(TractionOptions options, List<ICallNote> callNotes)
        {
            var settings = trainPart.PlanSessionsSettings;
            var units = trainPart.Working(trainPart.TractionUnits);
            if (options.ToLayover == TractionLayover.Stabling)
            {
                callNotes.AddRange(units
                    .Select(u => new ToParkingNote(u.Vehicle) { IsForArrival = true, Sessions = u.Sessions, Settings = settings }));
            }
            else if (options.ToLayover == TractionLayover.LiftedOff)
            {
                callNotes.AddRange(units
                    .Select(u => new LiftOffNote(u.Vehicle) { IsForArrival = true, Sessions = u.Sessions, Settings = settings }));
            }
            else if (options.HasUncoupleNote)
            {
                callNotes.AddRange(units
                    .Select(u => new UncoupleNote(u.Vehicle) { IsForArrival = true, Sessions = u.Sessions, Settings = settings }));
            }
        }

        // One note per vehicle of this part's own schedule, not per vehicle working the part: a part is
        // equal to any part over the same two calls, so the locomotive hauling a wagonset would otherwise
        // be sent to the track the wagonset's schedule names.
        // Wagonsets are the exception for the loco driver. The wagonset block of the duty booklet already
        // gives each one's track, so the driver is told once to shunt the wagons, and only the dispatcher
        // reads each wagonset and its track.
        private void AddFromOtherTrackNotes(List<ICallNote> callNotes)
        {
            if (trainPart.OtherFromTrack is not { } track) return;
            var settings = trainPart.PlanSessionsSettings;
            var own = trainPart.OwnVehicleSessions.ToList();
            callNotes.AddRange(own.Select(o => new FromTrackNote(o.Vehicle, track)
            {
                IsForDeparture = true,
                IsDriverNote = !o.Vehicle.IsWagonSet,
                Sessions = o.Sessions,
                Settings = settings,
            }));
            if (WagonsetSessions(own) is { Count: > 0 } wagonsets)
                callNotes.Add(new ShuntWagonsToDepartureTrackNote
                {
                    IsForDeparture = true,
                    IsStationNote = false,
                    Sessions = wagonsets.SharedPartOf(trainPart.Train.Sessions),
                    Settings = settings,
                });
        }

        private void AddToOtherTrackNotes(List<ICallNote> callNotes)
        {
            if (trainPart.OtherToTrack is not { } track) return;
            var settings = trainPart.PlanSessionsSettings;
            var own = trainPart.OwnVehicleSessions.ToList();
            callNotes.AddRange(own.Select(o => new ToTrackNote(o.Vehicle, track)
            {
                IsForArrival = true,
                IsDriverNote = !o.Vehicle.IsWagonSet,
                Sessions = o.Sessions,
                Settings = settings,
            }));
            if (WagonsetSessions(own) is { Count: > 0 } wagonsets)
                callNotes.Add(new ShuntWagonsToArrivalTrackNote
                {
                    IsForArrival = true,
                    IsStationNote = false,
                    Sessions = wagonsets.SharedPartOf(trainPart.Train.Sessions),
                    Settings = settings,
                });
        }

        // Each vehicle of the schedule this part belongs to (see AddFromOtherTrackNotes), with the sessions
        // or days it works the part's train on: those of the train's own that the vehicle is assigned to
        // the schedule for, or null where that is all of them and saying so would repeat the train. A
        // vehicle assigned for none of the train's sessions never works it, and gets no note.
        private IEnumerable<(ScheduledObject Vehicle, Sessions? Sessions)> OwnVehicleSessions
        {
            get
            {
                if (trainPart.Schedule is not { } schedule) yield break;
                var runs = trainPart.Train.Sessions;
                foreach (var vehicle in schedule.Vehicles)
                {
                    var shared = SessionsAssigned(vehicle, schedule.Equals, runs);
                    if (Never(shared)) continue;
                    yield return (vehicle, shared);
                }
            }
        }

        /// <summary>
        /// The vehicles among <paramref name="vehicles"/> that work this part on at least one of the
        /// sessions its train runs, each with the sessions or days it works it on — or null where that is
        /// every one of them, and saying so would only repeat the train.
        /// </summary>
        /// <remarks>
        /// The counterpart of <c>OwnVehicleSessions</c> for the vehicles resolved through the plan rather
        /// than through the part's own schedule (see <c>ScheduledObjects</c>), which is what the notes
        /// naming a vehicle are built from: a part is equal to any part over the same two calls, so a
        /// locomotive and the wagonset it hauls are both found there, each through its own schedule. The
        /// sessions come from the assignments to <em>whichever</em> schedules hold this part, so a vehicle
        /// assigned to two of them is counted once over both.
        /// <para>
        /// A vehicle assigned for none of the train's sessions never works it, and is left out rather than
        /// given a note qualified by no session at all.
        /// </para>
        /// </remarks>
        private List<(ScheduledObject Vehicle, Sessions? Sessions)> Working(IEnumerable<ScheduledObject> vehicles)
        {
            var runs = trainPart.Train.Sessions;
            return
            [
                .. vehicles
                    .Select(vehicle => (Vehicle: vehicle, Sessions: SessionsAssigned(vehicle, HoldsThisPart, runs)))
                    .Where(worked => !Never(worked.Sessions))
            ];

            bool HoldsThisPart(Schedule schedule) => schedule.Parts.Contains(trainPart);
        }

        // How sessions and days are written in this plan, for the notes that name some of them.
        private SessionsSettings? PlanSessionsSettings =>
            trainPart.Schedule?.Plan?.Layout.Settings.General.SessionSettings();

        /// <summary>
        /// The one note for what has to be done with the traction after arrival so the train can leave the
        /// other way: run it round to the other end of the train, turn it, or both. Null when neither is
        /// asked for, or when what is asked for is not needed.
        /// </summary>
        /// <remarks>
        /// Both flags together give a single note, not two. The two moves are one errand — the loco leaves
        /// the train, goes to the turntable and comes back on the other end — and stating them separately
        /// reads as two independent movements.
        /// </remarks>
        private GeneratedNote? TurningNote(TractionOptions options) =>
            (options.TurnLoco, options.RunaroundLoco && trainPart.NeedsRunaround) switch
            {
                (true, true) => new TurnAndCirculateNote { IsForArrival = true },
                (true, false) => new TurnNote { IsForArrival = true },
                (false, true) => new CirculateNote { IsForArrival = true },
                _ => null,
            };

        private void AddWagonSetDepartureNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.WagonSetOptions;
            if (options is null) return;
            // As for traction: fetching the wagonset from its other track is what brings it to the train.
            if (options.HasCoupleNote && trainPart.OtherFromTrack is null)
            {
                var settings = trainPart.PlanSessionsSettings;
                callNotes.AddRange(trainPart.Working(trainPart.WagonSets)
                    .Select(u => new CoupleNote(u.Vehicle) { IsForDeparture = true, Sessions = u.Sessions, Settings = settings }));
            }
        }

        /// <remarks>
        /// The mirror of <c>AddWagonSetDepartureNotes</c>: putting the wagonset on its other track takes
        /// it off the train, so where that is asked for the <see cref="ToTrackNote"/> says all this would.
        /// </remarks>
        private void AddWagonSetArrivalNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.WagonSetOptions;
            if (options is null) return;
            if (options.HasUncoupleNote && trainPart.OtherToTrack is null)
            {
                var settings = trainPart.PlanSessionsSettings;
                callNotes.AddRange(trainPart.Working(trainPart.WagonSets)
                    .Select(u => new UncoupleNote(u.Vehicle) { IsForArrival = true, Sessions = u.Sessions, Settings = settings }));
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
        /// Whether leaving in the direction the train arrived from costs this part's traction a runaround
        /// — the locomotive being run round to the other end of the train. It does not where every
        /// traction unit working the part reverses as it stands: a trainset, or a locomotive working a
        /// reversible train (see <c>ScheduledObject.ReversesWithoutRunaround</c>). A part whose traction
        /// cannot be resolved — one not yet worked by any vehicle, above all — is taken to need it, so
        /// what the planner asked for stands until the vehicles say otherwise.
        /// </summary>
        /// <remarks>
        /// The part-scope counterpart of <c>Plan.NeedsLocoRunaround</c>, which answers the same question
        /// for a whole train and is what the timings allow the standing time from.
        /// </remarks>
        public bool NeedsRunaround
        {
            get
            {
                var units = trainPart.TractionUnits.ToList();
                return units.Count == 0 || units.Any(unit => !unit.ReversesWithoutRunaround);
            }
        }

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

    // The sessions or days a vehicle is assigned to the matching schedules for, of those its train runs;
    // null where that is every one of them. The one aggregation behind OwnVehicleSessions and Working,
    // which differ only in which schedules they count.
    private static Sessions? SessionsAssigned(ScheduledObject vehicle, Func<Schedule, bool> schedules, Sessions trainRuns) =>
        vehicle.ScheduleAssignments
            .Where(assignment => schedules(assignment.Schedule))
            .Aggregate(Sessions.FromBitPattern(0), (all, assignment) => all.Or(assignment.Sessions))
            .SharedPartOf(trainRuns);

    // Whether a qualifier names no session at all: the vehicle is assigned, but never when the train runs.
    private static bool Never(Sessions? sessions) => sessions is { } some && some.Numbers.Length == 0;

    // The sessions or days each wagonset among a part's own vehicles works its train on (see
    // OwnVehicleSessions), for the one note telling the loco driver to shunt them all.
    private static List<Sessions?> WagonsetSessions(IEnumerable<(ScheduledObject Vehicle, Sessions? Sessions)> own) =>
        [.. own.Where(o => o.Vehicle.IsWagonSet).Select(o => o.Sessions)];

    // The track, where it is one of the call's location's tracks other than the one the train uses there.
    private static StationTrack? OtherTrackAt(StationTrack? track, StationCall call) =>
        track is not null && !track.Equals(call.Track) && IsTrackAt(track, call) ? track : null;

    private static bool IsTrackAt(StationTrack track, StationCall call) =>
        call.OperationLocation.Tracks.Contains(track);
}
