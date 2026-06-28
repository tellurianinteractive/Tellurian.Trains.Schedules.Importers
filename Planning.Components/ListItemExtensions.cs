namespace Tellurian.Trains.Schedules.Planning.Components;

/// <summary>
/// Projects sequences of model objects to <see cref="ListboxItem"/>s for HTML select/options. One
/// method per object type owns that type's option text. They are plain <c>this</c> extension methods,
/// so filtering and ordering are applied first with LINQ (e.g.
/// <c>trains.Where(Train.CanHostCargoFlow).OrderBy(t =&gt; t.Number).ToListItems()</c>), and a list that
/// depends on another selection is re-projected from a freshly filtered sequence (e.g. the arrivals
/// after a chosen departure: <c>train.ArrivalCallsAfter(from).ToListItems()</c>).
/// </summary>
public static class ListItemExtensions
{
    /// <summary>Value = train id, description = <see cref="TrainExtensions.ListLabel"/>.</summary>
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<Train> trains) =>
        trains.Select(t => new ListboxItem(t.Id.ToString(), t.ListLabel));

    /// <summary>Value = call id, description = station name and time.</summary>
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<StationCall> calls) =>
        calls.Select(c => new ListboxItem(c.Id.ToString(), $"{c.Station.Name} {c.SortTime.HHMM()}"));

    /// <summary>Value = description id, description = its name.</summary>
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<CargoFlowOptions> descriptions) =>
        descriptions.Select(d => new ListboxItem(d.Id.ToString(), d.Name));
}
