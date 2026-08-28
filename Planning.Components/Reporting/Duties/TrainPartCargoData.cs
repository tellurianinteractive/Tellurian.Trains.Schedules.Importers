using System.Net;
using Microsoft.AspNetCore.Components;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

/// <summary>
/// The rows of the cargo-wagons block on a train part page.
/// </summary>
/// <remarks>
/// This block matters more than its size suggests: when a duty is worked by two people it is the
/// conductor's working document, and the one block a non-driving reader uses on its own.
/// </remarks>
public sealed class TrainPartCargoData
{
    /// <summary>The cargo flows, ordered by position in the rake and then by session.</summary>
    public IReadOnlyList<TrainPartCargoFlow> Flows { get; init; } = [];

    /// <summary>How the sessions column is rendered.</summary>
    public required SessionsSettings SessionsSettings { get; init; }

    /// <summary>Whether the block has anything to show. An empty block is omitted entirely.</summary>
    public bool HasData => Flows.Count > 0;

    /// <summary>
    /// Whether any row limits how much is brought, which decides whether the block carries the load
    /// column at all.
    /// </summary>
    /// <remarks>
    /// A column that would be empty down its whole length is dropped rather than printed blank: on an A5
    /// page its width is taken from the destinations, which are the longest thing on the row and the
    /// first to wrap. This is the same rule as the empty blocks and the absent limits line — nothing
    /// unset is given room.
    /// </remarks>
    public bool HasMaxLoads => Flows.Any(flow => flow.MaxLoads.Count > 0);
}

/// <summary>
/// One load limit of a cargo row: how much may be brought, and to where when that needs saying.
/// </summary>
/// <param name="Location">
/// The destination the limit belongs to, or null when it is the whole row's and needs no naming.
/// </param>
/// <param name="Capacity">The limit itself.</param>
public sealed record DestinationMaxLoad(string? Location, TrainCapacity Capacity);

/// <summary>
/// One row of the cargo block: a whole <see cref="CargoFlowTrainPart"/>, not one destination.
/// </summary>
/// <remarks>
/// A flow's destinations belong together as a single statement of where these wagons go, and the flow is
/// also what carries the per-occurrence behaviour the row must show — position, shunting, and whether
/// wagons are taken here at all.
/// </remarks>
public sealed class TrainPartCargoFlow
{
    /// <summary>The cargo flow this row describes.</summary>
    public required CargoFlowTrainPart Flow { get; init; }

    /// <summary>The sessions the flow runs, which are its train's.</summary>
    public Sessions Sessions => Flow.Train.Sessions;

    /// <summary>
    /// The wagon's place in the rake, so the driver and conductor can find it. Zero means anywhere in
    /// the train, which is stated as a word: a cell showing <c>0</c> would be read as a position at the
    /// front.
    /// </summary>
    /// <param name="anyText">The translated text for "anywhere in the train".</param>
    public string PositionText(string anyText) =>
        Flow.PositionInTrain == 0 ? anyText : Flow.PositionInTrain.ToString();

    /// <summary>
    /// Where the wagons come from: the flow's forwarded origins together with this station itself,
    /// de-duplicated so a name never appears twice.
    /// </summary>
    /// <remarks>
    /// The from-station is included unless the flow brings no wagons from here — that flag removes
    /// exactly one of the two sources, never the whole column, since a flow that takes nothing on here
    /// may still be forwarding wagons from its origins. The de-duplication matters because an origin
    /// list may already name the from-station.
    ///
    /// A shunting task working arrived wagons out to the cargo customers is the exception: there the
    /// station is where the wagons stand, not where they came from — some earlier train brought them
    /// in — so naming it would only repeat the destination in the From column. The flow's origins
    /// answer the question when it names any, and when it names none the wagons may have arrived from
    /// anywhere, which the column states as a globe rather than leaving blank.
    ///
    /// Markup, as <see cref="To"/> is: the location names are planner-entered text and so are encoded,
    /// and the globe is an icon.
    /// </remarks>
    /// <param name="alsoShuntText">Appended when the driver also shunts before departure.</param>
    /// <param name="anywhereText">The translated wording the globe carries as its label.</param>
    public MarkupString From(string alsoShuntText, string anywhereText)
    {
        IEnumerable<string> names = Flow.CargoFlowOptions.Origins.Select(o => o.Location.Name);
        if (!Flow.BringsNoWagonsFromHere && !TakesArrivedWagonsOut)
            names = names.Append(Flow.From.OperationLocation.Name);
        var html = string.Join(", ", names.Distinct().Select(WebUtility.HtmlEncode));
        if (html.Length == 0 && TakesArrivedWagonsOut) html = AnywhereGlobe(anywhereText);
        // The qualifier belongs beside the movement it qualifies, rather than in a column of its own
        // that would be empty on most rows.
        return new(Flow.AlsoShuntBeforeDeparture ? $"{html}, {alsoShuntText}" : html);
    }

    /// <summary>
    /// The globe that says the wagons arrived from anywhere, carrying the wording as its label.
    /// </summary>
    /// <remarks>
    /// A mark rather than the word, for the reason the load column's marks are: *From* is the narrowest
    /// column of the block, and "Hvor som helst" wrapped it onto a second line the page budget then had
    /// to pay for. The word survives as the icon's title and accessible name, so it is a click of the
    /// mouse away on screen and read aloud unchanged. This is not the blank cell the rule forbids — a
    /// blank says *unknown*, and the globe says *anywhere*, which is the fact.
    ///
    /// Public because the general instructions booklet's key explains this mark, and draws it through
    /// this method so that the mark explained cannot drift from the mark printed.
    /// </remarks>
    public static string AnywhereGlobe(string anywhereText)
    {
        var label = WebUtility.HtmlEncode(anywhereText);
        return $"""<i class="fa-solid fa-globe anywhere" role="img" title="{label}" aria-label="{label}"></i>""";
    }

    /// <summary>
    /// Whether the row is a shunting task's flow of wagons that arrived at the worked station and are
    /// taken out to its cargo customers — the case where the from-station is not an origin.
    /// </summary>
    /// <remarks>
    /// Read from <see cref="CargoFlowTrainPartExtensions"/>' <c>ShuntingWork</c>, which derives the
    /// direction of the working from where the flow's wagons are bound; the guard on the train keeps it
    /// from being consulted for a travelling train, where it means nothing.
    /// </remarks>
    private bool TakesArrivedWagonsOut =>
        Flow.Train.IsShuntingTask && Flow.ShuntingWork == ShuntingWork.ToCargoCustomers;

    /// <summary>
    /// Where the wagons go, with destination regions as the same coloured chips used in the cargo notes.
    /// How many may be brought is left out — that has a column of its own.
    /// </summary>
    /// <param name="alsoShuntText">Appended when the driver also shunts after arrival.</param>
    public MarkupString To(string alsoShuntText)
    {
        var text = Flow.ToHtml;
        return new(Flow.AlsoShuntAfterArrival && !Flow.BringsNoWagonsFromHere
            ? $"{text}, {alsoShuntText}"
            : text);
    }

    /// <summary>
    /// How much may be brought, one entry per limit the row states. Empty when the row limits nothing.
    /// </summary>
    /// <remarks>
    /// A figure standing on its own says "this is what the row may carry", so it may only stand alone
    /// when that is true: one destination, or every destination carrying the same limit. Where the
    /// destinations differ — or where only some of them are limited at all — each figure is named,
    /// because an unnamed one would be read as applying to the whole row.
    /// </remarks>
    public IReadOnlyList<DestinationMaxLoad> MaxLoads
    {
        get
        {
            var destinations = Flow.CargoFlowOptions.Destinations;
            var limited = destinations.Where(destination => destination.HasMaxLoad).ToList();
            if (limited.Count == 0) return [];

            var first = limited[0];
            var isRowLimit =
                limited.Count == destinations.Count &&
                limited.All(d => d.MaxNumberOfAxles == first.MaxNumberOfAxles &&
                                 d.MaxNumberOfWagons == first.MaxNumberOfWagons);

            return isRowLimit
                ? [new(null, first.MaxLoad)]
                : [.. limited.Select(d => new DestinationMaxLoad(d.Location.Name, d.MaxLoad))];
        }
    }

    /// <summary>
    /// The UIC wagon-class letters limiting which wagons the flow brings. An empty cell means no
    /// restriction, printed as nothing rather than "all" or a dash: an absent restriction says more by
    /// being absent than by being spelled out on every row.
    /// </summary>
    public string WagonClasses => Flow.CargoFlowOptions.OnlyWagonClasses;
}
