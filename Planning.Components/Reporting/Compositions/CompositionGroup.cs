using Microsoft.AspNetCore.Components;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Compositions;

/// <summary>
/// One place in a departing train's composition: a wagonset with its wagons, or the cargo flow wagons
/// placed at one position. Each is drawn as a group of rectangles under a caption.
/// </summary>
public abstract record CompositionGroup
{
    /// <summary>
    /// Where in the train the group is placed, 1 at the front. Zero where no position is given — for a cargo
    /// flow that means anywhere in the train.
    /// </summary>
    public required int Position { get; init; }

    /// <summary>
    /// The order the groups are drawn in, front first: the positioned groups by position, then those with
    /// none. An unpositioned group is placed last because nothing puts it anywhere else, and a cargo flow
    /// without a position may be put anywhere — drawing it at the front would state a place it does not have.
    /// </summary>
    public int SortPosition => Position > 0 ? Position : int.MaxValue;
}

/// <summary>
/// A wagonset coupled to the train: its turnus rectangle, followed by one rectangle per wagon in the order the
/// wagons stand in the rake where it lists them.
/// </summary>
public sealed record WagonsetComposition : CompositionGroup
{
    /// <summary>The wagonset.</summary>
    public required ScheduledObject Wagonset { get; init; }

    /// <summary>The wagons it lists, in rake order; empty where it lists none.</summary>
    public required IReadOnlyList<CompositionWagon> Wagons { get; init; }

    /// <summary>
    /// The sessions or days the wagonset is in the train, where that is only some of the train's own;
    /// <c>null</c> where it is all of them, since saying so would only repeat the train.
    /// </summary>
    /// <remarks>
    /// This is what tells two wagonsets apart that work the same train on different sessions: both are drawn,
    /// each with when it runs beside its turnus.
    /// </remarks>
    public Sessions? Sessions { get; init; }

    /// <summary>
    /// The plain-text form of <see cref="Sessions"/> as it is drawn, for the estimate; <c>null</c> where
    /// <see cref="Sessions"/> is.
    /// </summary>
    public string? SessionsText { get; init; }

    /// <summary>
    /// The identity written on the wagonset's card, so the staff can match the rake to it. Drawn in a shaded
    /// rectangle at the front of the wagonset's own, where it costs width rather than a column of its own.
    /// </summary>
    public string Designation => Wagonset.Designation;
}

/// <summary>One listed wagon of a wagonset, with its class and number.</summary>
/// <param name="Class">The wagon class; empty where none is given.</param>
/// <param name="Number">The wagon's own number, where it has one.</param>
public sealed record CompositionWagon(string Class, string? Number = null);

/// <summary>
/// One unit of cargo flow wagons in the train, drawn as one rectangle listing where they go.
/// </summary>
/// <remarks>
/// A place in the train is given at two levels: the cargo flow says where its wagons stand in the train, and
/// each of its destinations says where they stand within that. Every unit is marshalled on its own, so each
/// gets a rectangle of its own. Every flow sharing both positions contributes its destinations to the same
/// rectangle: the rectangle stands for a place in the train, and wagons for several flows are gathered there.
/// </remarks>
public sealed record CargoPositionComposition : CompositionGroup
{
    /// <summary>
    /// Where within the cargo flow's own position the wagons stand, 1 at the front. Zero where the
    /// destinations give none, and then the wagons stand anywhere within that position.
    /// </summary>
    public int DestinationPosition { get; init; }

    /// <summary>
    /// The order the units at one cargo flow position are drawn in, front first. Zero sorts last, for the
    /// reason given in <see cref="CompositionGroup.SortPosition"/>.
    /// </summary>
    public int SortDestinationPosition => DestinationPosition > 0 ? DestinationPosition : int.MaxValue;

    /// <summary>Where the wagons of this unit go, one entry per destination.</summary>
    public required IReadOnlyList<CompositionDestination> Destinations { get; init; }
}

/// <summary>
/// One unit of cargo flow wagons uncoupled from an arriving train, drawn as one rectangle listing where the
/// wagons came from.
/// </summary>
/// <remarks>
/// One per position the flows take in the train, so the rectangles stand in the order the wagons arrive in.
/// Every flow sharing the position contributes its origins to the same rectangle.
/// </remarks>
public sealed record ArrivingCargoComposition : CompositionGroup
{
    /// <summary>Where the wagons came from, each place once, in the order the flows name them.</summary>
    public required IReadOnlyList<string> Origins { get; init; }
}

/// <summary>
/// One destination of a cargo position: the place, with its local destinations, its "and beyond"
/// qualifier and its regions, and the most that may be brought there.
/// </summary>
/// <param name="Text">
/// The plain text of the whole entry, the limit included, which the page-height estimate charges for.
/// </param>
/// <param name="Html">The printed form of the place, with the regions as coloured chips.</param>
/// <param name="Limit">
/// The most that may be brought to this destination, drawn after the place in its compact form.
/// Unspecified where the destination takes any number, and then nothing is drawn.
/// </param>
public sealed record CompositionDestination(string Text, MarkupString Html, TrainCapacity Limit = default);
