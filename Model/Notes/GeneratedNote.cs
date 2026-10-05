using Microsoft.AspNetCore.Components;
using ClassNames = Tellurian.Trains.Schedules.Model.Resources.ClassNames;
using NoteResources = Tellurian.Trains.Schedules.Model.Resources.Notes;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Base type for the transient call notes generated on demand from a <see cref="ScheduledTrainPart"/>'s options.
/// Unlike the persisted <see cref="CallNote"/> family, generated notes are never stored; they exist only
/// to be rendered. Each note is a thin data carrier — what it says is produced by the switch
/// expression in <see cref="GeneratedNoteExtensions"/>.
/// </summary>
public abstract record GeneratedNote : ICallNote
{
    /// <summary>
    /// Controls the order in which the note is displayed; lower values sort first.
    /// </summary>
    public int DisplayOrder { get; init; } = 900;

    /// <inheritdoc/>
    public bool IsDriverNote { get; init; } = true;

    /// <inheritdoc/>
    public bool IsStationNote { get; init; } = true;

    /// <inheritdoc/>
    public bool IsShuntingNote { get; init; }

    /// <inheritdoc/>
    public bool IsForArrival { get; init; }

    /// <inheritdoc/>
    public bool IsForDeparture { get; init; }

    /// <summary>
    /// The sessions or days the note holds on, where those are fewer than the ones its train runs; null
    /// where it holds whenever the train runs, and saying so would only repeat the train.
    /// </summary>
    /// <remarks>
    /// A vehicle is assigned to its schedule for sessions of its own (see <c>ScheduleAssignment</c>), so
    /// an instruction about that vehicle is not necessarily given every time the train comes. Where it is
    /// not, the note leads with the sessions it is for — the reader sees before anything else whether it
    /// is for today. A note naming no vehicle takes the sessions of all the vehicles it speaks for
    /// together.
    /// </remarks>
    public Sessions? Sessions { get; init; }

    /// <summary>
    /// How <see cref="Sessions"/> is written — as session numbers or as weekday names. Null where the
    /// note has no sessions to write, when nothing is prefixed.
    /// </summary>
    public SessionsSettings? Settings { get; init; }

    /// <inheritdoc/>
    public string ToText => this.TextOf;

    /// <inheritdoc/>
    public MarkupString ToHtml => this.HtmlOf;
}

/// <summary>
/// What each generated note says, and the two forms it is read in.
/// </summary>
public static class GeneratedNoteExtensions
{
    extension(GeneratedNote note)
    {
        /// <summary>
        /// What the note says: its localised format string and the values substituted into it. One
        /// switch, not one per output form — see <see cref="NoteTemplate"/> for why.
        /// </summary>
        /// <remarks>
        /// Values are emphasised in the markup form unless the arm says otherwise. A position in the
        /// train is <see cref="NoteArg.Plain(object?)"/> because it stands beside the vehicle that
        /// carries the emphasis; destinations and meets are <see cref="NoteArg.Markup(string, string)"/>
        /// because they render themselves as coloured chips and session circles.
        /// </remarks>
        internal NoteTemplate TemplateOf => OnSessions(note.WordingOf, note.Sessions, note.Settings);

        /// <summary>
        /// The note without its session qualifier: the localised format string and the values substituted
        /// into it. <c>TemplateOf</c> is this led by the sessions, where the note holds on only some of
        /// them.
        /// </summary>
        private NoteTemplate WordingOf => note switch
        {
            UseNote(var so) => new(NoteResources.Use, so),
            CoupleNote(var so, 0) => new(NoteResources.CoupleToTrain, so),
            CoupleNote(var so, var position) => new(NoteResources.CoupleToTrainInPosition, so, NoteArg.Plain(position)),
            UncoupleNote(var so) => new(NoteResources.UncoupleFromTrain, so),
            FromParkingNote(var so) => new(NoteResources.MoveTractionUnitFromParkingToDepartureTrack, so),
            ToParkingNote(var so) => new(NoteResources.MoveTractionUnitToParking, so),
            // The kind of vehicle leads the instruction — a driver reading "fetch wagon group 21" knows
            // what to look for on the track before finding the number. It is plain: the number beside it
            // is what identifies the vehicle and carries the emphasis.
            FromTrackNote(var so, var track) => new(NoteResources.FetchFromTrack, NoteArg.Plain(ClassNames.InSentenceFor(so)), so, track),
            ToTrackNote(var so, var track) => new(NoteResources.PutOnTrack, NoteArg.Plain(ClassNames.InSentenceFor(so)), so, track),
            ShuntWagonsToDepartureTrackNote => new(NoteResources.ShuntWagonsToDepartureTrack),
            ShuntWagonsToArrivalTrackNote => new(NoteResources.ShuntWagonsToArrivalTrack),
            CirculateNote => new(NoteResources.CirculateTractionUnit),
            TurnNote => new(NoteResources.TurnTractionUnit),
            TurnAndCirculateNote => new(NoteResources.TurnAndCirculateTractionUnit),
            ReinforcementNote(_, var part) => new(NoteResources.ReinforcesBetweenAnd, part.Train, part.From.OperationLocation, part.To.OperationLocation),
            TractionUnitExchangeNote(_, var from, var to) => new(NoteResources.TractionUnitExchange, from, to),
            CargoFlowDestinationNote(var parts) when parts.HasCargoFlowOptions =>
                new(NoteResources.BringsWagonsTo, NoteArg.Markup(parts.DestinationsText, parts.DestinationsHtml)),
            // Every destination the flow serves, not only those at this station: the wagons taken off
            // here are sorted by where they go on to, and a destination beyond it is what says which
            // train they are put over to.
            CargoFlowUncoupleNote(var parts) when parts.HasCargoFlowOptions =>
                new(NoteResources.UncoupleWagonsFor, NoteArg.Markup(parts.DestinationsText, parts.DestinationsHtml)),
            // The origins are what the driver sorts the arrived wagons by, so they carry the emphasis.
            // A flow naming none still gives a usable instruction — take what arrived out to the
            // customers — so it gets the wording without the clause rather than an empty one.
            ShuntArrivingWagonsNote(var part) => part.FromText.HasValue
                ? new(NoteResources.ShuntArrivingWagonsFromToCargoCustomers, NoteArg.Markup(part.FromText, part.FromHtml))
                : new(NoteResources.ShuntArrivingWagonsToCargoCustomers),
            FetchDepartingWagonsNote(var part) =>
                new(NoteResources.FetchWagonsToFromCargoCustomers, NoteArg.Markup(part.ToText, part.ToHtml)),
            // The key's name is emphasised beside the location, because at a station holding several it
            // is what the driver has to ask for by name.
            PickUpLockKeyNote(var location, var name) => name.HasValue
                ? new(NoteResources.PickUpKeyForUnlocking, name, location)
                : new(NoteResources.PickUpUnnamedKeyForUnlocking, location),
            LeaveLockKeyNote(var location, var name) => name.HasValue
                ? new(NoteResources.LeaveKeyFrom, name, location)
                : new(NoteResources.LeaveUnnamedKeyFrom, location),
            NoStopNote => new(NoteResources.NoStop),
            ExchangeTransferringPassengersNote => new(NoteResources.ExchangeTransferringPassengers),
            NoExchangeNote => new(NoteResources.NoExchange),
            // Reuses the wording the sessions value itself uses for the marker, so a reader meets the
            // same phrase whether it is stated in a column or as a note.
            OnDemandNote => new(DaysExtensions.DayResource("OnDemandOnly")),
            ShuntingTaskNote(var starts, var ends) => new(NoteResources.ShuntingTaskFromTo, starts.HHMM(), ends.HHMM()),
            CrossingNote(var meets) => new(NoteResources.Crosses, MeetList(meets, note.Settings)),
            OvertakesNote(var meets) => new(NoteResources.Overtakes, MeetList(meets, note.Settings)),
            IsOvertakenNote(var meets) => new(NoteResources.IsOvertakenBy, MeetList(meets, note.Settings)),
            _ => new(string.Empty),
        };

        /// <summary>
        /// Plain-text rendering of the note.
        /// </summary>
        internal string TextOf => note.TemplateOf.ToText;

        /// <summary>
        /// HTML/CSS markup rendering of the note: the same content with the substituted values
        /// emphasised, wrapped in the <c>callnote</c> span every note is styled through.
        /// </summary>
        internal MarkupString HtmlOf => new($"""<span class="callnote">{note.TemplateOf.ToHtml}</span>""");
    }

    /// <summary>
    /// Every train met, each with the window in which both are present and a session qualifier when
    /// that meet does not happen on every session the reader's duty runs.
    /// </summary>
    /// <remarks>
    /// A composed argument rather than a note of its own: the list is built in both forms here and
    /// substituted into the crossing or overtaking note as one value. The train is named with
    /// <c>Train.ToString()</c>, which composes the company signature, category prefix, number and
    /// suffix and resolves the <em>effective</em> company — so a train inheriting its category's company
    /// is still named correctly — and is emphasised, since it is what the driver is looking for. The
    /// times are not: they sit beside the name and the reader already has them in the time column.
    /// <paramref name="meets"/> is already in display order (see <c>MeetNoteExtensions.MeetNotes</c>);
    /// this only renders it.
    /// </remarks>
    private static NoteArg MeetList(IReadOnlyList<Meet> meets, SessionsSettings? settings)
    {
        var items = meets.Select(meet =>
        {
            var item = new NoteTemplate(NoteResources.TrainMeetAtTime, meet.Other, NoteArg.Plain(When(meet)));
            if (meet.Sessions is not { } qualifier || settings is null) return NoteArg.Markup(item.ToText, item.ToHtml);
            var sessions = new NoteTemplate(NoteResources.InSessions, NoteArg.Markup(qualifier.ToText(settings), qualifier.ToHtml(settings)));
            return NoteArg.Markup($"{item.ToText} {sessions.ToText}", $"{item.ToHtml} {sessions.ToHtml}");
        }).ToList();
        return NoteArg.Markup(
            string.Join(", ", items.Select(item => item.Text)),
            string.Join(", ", items.Select(item => item.Html)));
    }

    /// <summary>
    /// A note that holds on only some of the sessions or days its train runs, led by those sessions or
    /// days; the note as it is where it holds on all of them.
    /// </summary>
    /// <remarks>
    /// Leading rather than trailing, so the reader sees before anything else whether the note is for
    /// today at all. The value renders itself — session circles, or day names — exactly as it does in the
    /// session columns, and so does the note it leads, with its own values emphasised.
    /// </remarks>
    private static NoteTemplate OnSessions(NoteTemplate note, Sessions? sessions, SessionsSettings? settings) =>
        sessions is { } qualifier && settings is not null
            ? new(NoteResources.OnSessions,
                NoteArg.Markup(qualifier.ToText(settings), qualifier.ToHtml(settings)),
                NoteArg.Markup(note.ToText, note.ToHtml))
            : note;

    /// <summary>
    /// When a meet happens: the window both trains are present, or a single time when the other train
    /// only passes through and there is no interval to state.
    /// </summary>
    private static string When(Meet meet) =>
        meet.From == meet.To ? meet.From.HHMM() : $"{meet.From.HHMM()}-{meet.To.HHMM()}";
}
