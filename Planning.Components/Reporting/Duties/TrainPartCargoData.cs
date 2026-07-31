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
}

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
    /// </remarks>
    /// <param name="alsoShuntText">Appended when the driver also shunts before departure.</param>
    public string From(string alsoShuntText)
    {
        IEnumerable<string> names = Flow.CargoFlowOptions.Origins.Select(o => o.Location.Name);
        if (!Flow.BringsNoWagonsFromHere) names = names.Append(Flow.From.OperationLocation.Name);
        var text = string.Join(", ", names.Distinct());
        // The qualifier belongs beside the movement it qualifies, rather than in a column of its own
        // that would be empty on most rows.
        return Flow.AlsoShuntBeforeDeparture ? $"{text}, {alsoShuntText}" : text;
    }

    /// <summary>
    /// Where the wagons go, with destination regions as the same coloured chips used in the cargo notes.
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
    /// The UIC wagon-class letters limiting which wagons the flow brings. An empty cell means no
    /// restriction, printed as nothing rather than "all" or a dash: an absent restriction says more by
    /// being absent than by being spelled out on every row.
    /// </summary>
    public string WagonClasses => Flow.CargoFlowOptions.OnlyWagonClasses;
}
