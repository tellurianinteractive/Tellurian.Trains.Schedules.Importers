using Microsoft.AspNetCore.Components;
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
        internal NoteTemplate TemplateOf => note switch
        {
            UseNote(var so) => new(NoteResources.Use, so),
            CoupleNote(var so, 0) => new(NoteResources.CoupleToTrain, so),
            CoupleNote(var so, var position) => new(NoteResources.CoupleToTrainInPosition, so, NoteArg.Plain(position)),
            UncoupleNote(var so) => new(NoteResources.UncoupleFromTrain, so),
            FromParkingNote(var so) => new(NoteResources.MoveTractionUnitFromParkingToDepartureTrack, so),
            ToParkingNote(var so) => new(NoteResources.MoveTractionUnitToParking, so),
            CirculateNote => new(NoteResources.CirculateTractionUnit),
            TurnNote => new(NoteResources.TurnTractionUnit),
            TurnAndCirculateNote => new(NoteResources.TurnAndCirculateTractionUnit),
            ReinforcementNote(_, var part) => new(NoteResources.ReinforcesBetweenAnd, part.Train, part.From.OperationLocation, part.To.OperationLocation),
            TractionUnitExchangeNote(_, var from, var to) => new(NoteResources.TractionUnitExchange, from, to),
            CargoFlowDestinationNote(var part) when part.CargoFlowOptions is not null =>
                new(NoteResources.BringsWagonsTo, NoteArg.Markup(part.ToText, part.ToHtml)),
            // The key's name is emphasised beside the location, because at a station holding several it
            // is what the driver has to ask for by name.
            PickUpLockKeyNote(var location, var name) => name.HasValue
                ? new(NoteResources.PickUpKeyForUnlocking, name, location)
                : new(NoteResources.PickUpUnnamedKeyForUnlocking, location),
            LeaveLockKeyNote(var location, var name) => name.HasValue
                ? new(NoteResources.LeaveKeyFrom, name, location)
                : new(NoteResources.LeaveUnnamedKeyFrom, location),
            NoStopNote => new(NoteResources.NoStop),
            NoExchangeNote => new(NoteResources.NoExchange),
            // Reuses the wording the sessions value itself uses for the marker, so a reader meets the
            // same phrase whether it is stated in a column or as a note.
            OnDemandNote => new(DaysExtensions.DayResource("OnDemandOnly")),
            CrossingNote(var meets, var settings) => new(NoteResources.Crosses, MeetList(meets, settings)),
            OvertakesNote(var meets, var settings) => new(NoteResources.Overtakes, MeetList(meets, settings)),
            IsOvertakenNote(var meets, var settings) => new(NoteResources.IsOvertakenBy, MeetList(meets, settings)),
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
    /// When a meet happens: the window both trains are present, or a single time when the other train
    /// only passes through and there is no interval to state.
    /// </summary>
    private static string When(Meet meet) =>
        meet.From == meet.To ? meet.From.HHMM() : $"{meet.From.HHMM()}-{meet.To.HHMM()}";
}
