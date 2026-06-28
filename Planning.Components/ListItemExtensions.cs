namespace Tellurian.Trains.Schedules.Planning.Components;

/// <summary>
/// Projects sequences of model objects to <see cref="ListboxItem"/>s for HTML select/options. One
/// projection per object type owns that type's option text. They are plain <c>this</c> extension
/// methods, so filtering is applied first with LINQ, e.g.
/// <c>trains.Where(Train.CanHostCargoFlow).ToListItems(t =&gt; t.Number)</c>.
/// <para>
/// Each type has two overloads: the no-argument one sorts by the displayed description; the keyed one
/// sorts the source by the returned value before projecting — use it when the description does not sort
/// correctly as text, e.g. a call shown as "Sti 14:30" must be sorted by its <c>SortTime</c>:
/// <c>train.DepartureCalls.ToListItems(c =&gt; c.SortTime)</c>. A list that depends on another selection
/// re-sorts naturally because it is re-projected each render from a freshly filtered sequence.
/// </para>
/// </summary>
public static class ListItemExtensions
{
    // ---- Train ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<Train> trains) =>
        SortedByDescription(trains.Select(ToItem));

    public static IEnumerable<ListboxItem> ToListItems<TKey>(this IEnumerable<Train> trains, Func<Train, TKey> sorting) =>
        trains.OrderBy(sorting).Select(ToItem);

    private static ListboxItem ToItem(Train train) => new(train.Id.ToString(), train.ListLabel);

    // ---- StationCall ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<StationCall> calls) =>
        SortedByDescription(calls.Select(ToItem));

    public static IEnumerable<ListboxItem> ToListItems<TKey>(this IEnumerable<StationCall> calls, Func<StationCall, TKey> sorting) =>
        calls.OrderBy(sorting).Select(ToItem);

    private static ListboxItem ToItem(StationCall call) => new(call.Id.ToString(), $"{call.Station.Name} {call.SortTime.HHMM()}");

    // ---- CargoFlowOptions (cargo descriptions) ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<CargoFlowOptions> descriptions) =>
        SortedByDescription(descriptions.Select(ToItem));

    public static IEnumerable<ListboxItem> ToListItems<TKey>(this IEnumerable<CargoFlowOptions> descriptions, Func<CargoFlowOptions, TKey> sorting) =>
        descriptions.OrderBy(sorting).Select(ToItem);

    private static ListboxItem ToItem(CargoFlowOptions description) => new(description.Id.ToString(), description.Name);

    // The default order: by the text the user actually sees, using the current culture's rules.
    private static IEnumerable<ListboxItem> SortedByDescription(IEnumerable<ListboxItem> items) =>
        items.OrderBy(i => i.LocalizedDescription, StringComparer.CurrentCulture);
}
