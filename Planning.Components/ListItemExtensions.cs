using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.App.Translations;

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

    // ---- OperationLocation (stations and other places); also covers IEnumerable<Station> by covariance ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<OperationLocation> locations) =>
        SortedByDescription(locations.Select(ToItem));

    public static IEnumerable<ListboxItem> ToListItems<TKey>(this IEnumerable<OperationLocation> locations, Func<OperationLocation, TKey> sorting) =>
        locations.OrderBy(sorting).Select(ToItem);

    private static ListboxItem ToItem(OperationLocation location) => new(location.Id.ToString(), $"{location.Name} ({location.Signature})");

    // ---- Company ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<Company> companies) =>
        SortedByDescription(companies.Select(ToItem));

    public static IEnumerable<ListboxItem> ToListItems<TKey>(this IEnumerable<Company> companies, Func<Company, TKey> sorting) =>
        companies.OrderBy(sorting).Select(ToItem);

    private static ListboxItem ToItem(Company company) => new(company.Id.ToString(), company.Name);

    // ---- TrainCategory ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<TrainCategory> categories) =>
        SortedByDescription(categories.Select(ToItem));

    public static IEnumerable<ListboxItem> ToListItems<TKey>(this IEnumerable<TrainCategory> categories, Func<TrainCategory, TKey> sorting) =>
        categories.OrderBy(sorting).Select(ToItem);

    private static ListboxItem ToItem(TrainCategory category) => new(category.Id.ToString(), category.Name);

    // ---- StationTrack ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<StationTrack> tracks) =>
        SortedByDescription(tracks.Select(ToItem));

    public static IEnumerable<ListboxItem> ToListItems<TKey>(this IEnumerable<StationTrack> tracks, Func<StationTrack, TKey> sorting) =>
        tracks.OrderBy(sorting).Select(ToItem);

    private static ListboxItem ToItem(StationTrack track) => new(track.Id.ToString(), $"{track.Station.Name} {track.Number}");

    // ---- Country (label is the localised name, so a Translator is required) ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<Country> countries, Translator translator) =>
        countries.Select(c => new ListboxItem(c.Id.ToString(), translator(c.ResourceKey)))
                 .OrderBy(i => i.LocalizedDescription, StringComparer.CurrentCulture);

    // The default order: by the text the user actually sees, using the current culture's rules.
    private static IEnumerable<ListboxItem> SortedByDescription(IEnumerable<ListboxItem> items) =>
        items.OrderBy(i => i.LocalizedDescription, StringComparer.CurrentCulture);
}
