namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Compositions;

/// <summary>Which way a train travels across a printed composition sheet, and so which end its loco is drawn at.</summary>
public enum CompositionHeading
{
    /// <summary>Towards the left of the sheet: the loco is drawn at the left end and the wagons follow to the right.</summary>
    Leftwards,

    /// <summary>Towards the right of the sheet: the loco is drawn at the right end and the wagons follow to the left.</summary>
    Rightwards,
}

/// <summary>Which side of the tracks a composition sheet is read from.</summary>
public enum CompositionView
{
    /// <summary>Trains running in the direction of their track stretch head right.</summary>
    AsDrawn,

    /// <summary>From the other side of the tracks: every train mirrored.</summary>
    Mirrored,
}

/// <summary>
/// Which way a train travels past a station, from the defined direction of the track stretch it runs on, so
/// that it can be drawn travelling the way it passes whoever reads the sheet.
/// </summary>
/// <remarks>
/// A train running in the stretch's own direction, from its start towards its end, travels forward, and is
/// drawn heading right on the sheet as drawn; one running the other way heads left. A departing train runs on
/// the stretch to its next location, an arriving one on the stretch from its previous location. Nobody knows
/// which side of a station the yard master stands on, so the sheet is printed both ways round
/// (<see cref="CompositionView"/>): only that forward and backward trains face opposite ways matters.
/// </remarks>
/// <param name="layout">The layout whose track stretches give the directions.</param>
public sealed class CompositionOrientation(Layout layout)
{
    private readonly Layout _layout = layout.ValueOrException(nameof(layout));

    /// <summary>
    /// The heading of a train travelling from one location to another: rightwards where it runs in the
    /// direction of the track stretch joining them, leftwards where it runs against it. Where no stretch joins
    /// them the train travels leftwards, so the loco is drawn first in reading order.
    /// </summary>
    /// <param name="from">Where the train travels from.</param>
    /// <param name="to">Where it travels to.</param>
    public CompositionHeading HeadingFrom(OperationLocation from, OperationLocation to) =>
        _layout.StretchBetween(from, to) is { } stretch && stretch.Start.Equals(from)
            ? CompositionHeading.Rightwards
            : CompositionHeading.Leftwards;

    /// <summary>The station's neighbours a train departing towards one side runs to, by name.</summary>
    /// <param name="station">The station.</param>
    /// <param name="heading">The side: a train travelling that way leaves for these neighbours.</param>
    public IReadOnlyList<OperationLocation> NeighboursTowards(OperationLocation station, CompositionHeading heading) =>
        [.. _layout.NeighboursOf(station).Where(neighbour => HeadingFrom(station, neighbour) == heading)];
}

/// <summary>Turns a heading round for the side of the tracks a sheet is read from.</summary>
public static class CompositionHeadingExtensions
{
    extension(CompositionHeading heading)
    {
        /// <summary>The heading as it is seen in the given view: the same as drawn, the other way when mirrored.</summary>
        /// <param name="view">The side the sheet is read from.</param>
        public CompositionHeading SeenIn(CompositionView view) =>
            view == CompositionView.AsDrawn ? heading
            : heading == CompositionHeading.Leftwards ? CompositionHeading.Rightwards : CompositionHeading.Leftwards;
    }
}
