using Microsoft.AspNetCore.Components;
using NoteResources = Tellurian.Trains.Schedules.Model.Resources.Notes;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Base type for the transient call notes generated on demand from a <see cref="TrainPart"/>'s options.
/// Unlike the persisted <see cref="CallNote"/> family, generated notes are never stored; they exist only
/// to be rendered. Each note is a thin data carrier — the text and markup are produced by the switch
/// expressions in <see cref="GeneratedNoteExtensions"/>.
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
    public string Text => this.TextOf;

    /// <inheritdoc/>
    public MarkupString Html => this.HtmlOf;
}

/// <summary>
///
/// </summary>
public static class GeneratedNoteExtensions
{
    extension(GeneratedNote note)
    {
        /// <summary>
        /// Plain-text rendering of the note.
        /// </summary>
        internal string TextOf => note switch
        {
            UseNote(var so) => NoteText.Format(NoteResources.Use, so),
            CoupleNote(var so, 0) => NoteText.Format(NoteResources.CoupleToTrain, so),
            CoupleNote(var so, var position) => NoteText.Format(NoteResources.CoupleToTrainInPosition, so, position),
            UncoupleNote(var so) => NoteText.Format(NoteResources.UncoupleFromTrain, so),
            FromParkingNote(var so) => NoteText.Format(NoteResources.MoveTractionUnitFromParkingToDepartureTrack, so),
            ToParkingNote(var so) => NoteText.Format(NoteResources.MoveTractionUnitToParking, so),
            ReinforcementNote(_, var part) => NoteText.Format(NoteResources.ReinforcesBetweenAnd, part.Train, part.From.Station, part.To.Station),
            CargoFlowDestinationNote(var part) when part.CargoFlowOptions is not null =>
                NoteText.Format(NoteResources.BringsWagonsTo, part.CargoFlowDestinationsText),
            _ => string.Empty,
        };

        /// <summary>
        /// HTML/CSS markup rendering of the note. Most notes simply wrap their text in a
        /// <c>callnote</c> span; notes that contain coloured elements such as regions render a specialised variant.
        /// </summary>
        internal MarkupString HtmlOf => note switch
        {
            // Specialised variant: destination regions are rendered as coloured chips (see Region.Display).
            CargoFlowDestinationNote(var part) when part.CargoFlowOptions is not null =>
                new($"""<span class="callnote">{NoteText.Format(NoteResources.BringsWagonsTo, part.CargoFlowDestinationsHtml)}</span>"""),
            // Default: what the base note rendering does — wrap the plain text in a callnote span.
            _ => new($"""<span class="callnote">{note.TextOf}</span>"""),
        };
    }
}
