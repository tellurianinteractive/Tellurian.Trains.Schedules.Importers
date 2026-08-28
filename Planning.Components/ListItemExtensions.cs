using System.Globalization;
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
/// correctly as text, e.g. a company shown by name but listed in catalogue order. A station call takes
/// this further: what it is listed for decides which of its two times it shows, so the caller passes that
/// time and it is sorted by it. A list that depends on another selection re-sorts naturally because it is
/// re-projected each render from a freshly filtered sequence.
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
    // A call is shown as "Sti 14:30", and which of the call's two times that is depends on what the list
    // is for. A cargo flow, for one, connects its wagons at a departure and disconnects them at an
    // arrival — and on a shunting task, whose flow spans its single call from when the work starts to
    // when it ends, those two are the other way round (Train.CargoFlowConnectTime). So the caller names
    // the time to show, and the list is sorted by it, so that the times read in order.
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<StationCall> calls, Func<StationCall, Time> shownTime) =>
        calls.OrderBy(shownTime).Select(call => ToItem(call, shownTime(call)));

    // Without a time named, a call shows the one it sorts by.
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<StationCall> calls) =>
        SortedByDescription(calls.Select(call => ToItem(call, call.SortTime)));

    private static ListboxItem ToItem(StationCall call, Time time) => new(call.Id.ToString(), $"{call.OperationLocation.Name} {time.HHMM()}");

    // ---- CargoFlowOptions (cargo descriptions) ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<CargoFlowOptions> descriptions) =>
        SortedByDescription(descriptions.Select(ToItem));

    public static IEnumerable<ListboxItem> ToListItems<TKey>(this IEnumerable<CargoFlowOptions> descriptions, Func<CargoFlowOptions, TKey> sorting) =>
        descriptions.OrderBy(sorting).Select(ToItem);

    private static ListboxItem ToItem(CargoFlowOptions description) => new(description.Id.ToString(), description.OnlyWagonClasses);

    // Label describing the routing, leading with the wagon-class filter when set:
    //   classes + origins -> "U,Z from <origins> to <destinations>"
    //   classes, no origins -> "U,Z wagons to <destinations>"
    //   no classes, origins -> "Wagons from <origins> to <destinations>"
    //   no classes, no origins -> "Wagons to <destinations>"
    // Needs a Translator for the localised, word-order-preserving format strings — each takes the
    // destinations as a placeholder, so none of them is the bare To used as a report column header.
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<CargoFlowOptions> descriptions, Translator translator) =>
        SortedByDescription(descriptions.Select(d => ToItem(d, translator)));

    private static ListboxItem ToItem(CargoFlowOptions description, Translator translator)
    {
        var destinations = description.DestinationsSummary;
        var origins = description.OriginLocationNames;
        var classes = description.OnlyWagonClasses.Trim();
        var hasClasses = classes.Length > 0;
        var hasOrigins = description.Origins.Count > 0;
        var label =
            hasOrigins && hasClasses ? string.Format(CultureInfo.CurrentCulture, translator("WagonClassesFromTo"), classes, origins, destinations) :
            hasOrigins               ? string.Format(CultureInfo.CurrentCulture, translator("WagonsFromTo"), origins, destinations) :
            hasClasses               ? string.Format(CultureInfo.CurrentCulture, translator("WagonClassesTo"), classes, destinations) :
                                       string.Format(CultureInfo.CurrentCulture, translator("WagonsToDestinations"), destinations);
        return new(description.Id.ToString(), label);
    }

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

    // ---- StationTrack (the label names the track and what it is used for, so a Translator is required
    //      for the localised word for "track" and the word order) ----
    // The one projection that keeps the order it is given rather than sorting by the text shown. A track
    // list belongs to a station chosen beside it, and the order is the planner's own — pass
    // OperationLocation.TracksInDisplayOrder, which is where that order is defined. The station is not
    // named again here either, the drop-down beside it having just said which one.
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<StationTrack> tracks, Translator translator) =>
        tracks.Select(track => ToItem(track, translator));

    // "Track 5, Cargo" - the usage is left out when the track has none.
    private static ListboxItem ToItem(StationTrack track, Translator translator)
    {
        var usage = track.Usage.Trim();
        var label = usage.Length > 0
            ? string.Format(CultureInfo.CurrentCulture, translator("StationTrackOptionWithUsage"), track.Number, usage)
            : string.Format(CultureInfo.CurrentCulture, translator("StationTrackOption"), track.Number);
        return new(track.Id.ToString(), label);
    }

    // ---- Country (label is the localised name, so a Translator is required) ----
    public static IEnumerable<ListboxItem> ToListItems(this IEnumerable<Country> countries, Translator translator) =>
        countries.Select(c => new ListboxItem(c.Id.ToString(), translator(c.ResourceKey)))
                 .OrderBy(i => i.LocalizedDescription, StringComparer.CurrentCulture);

    // The default order: by the text the user actually sees, using the current culture's rules.
    private static IEnumerable<ListboxItem> SortedByDescription(IEnumerable<ListboxItem> items) =>
        items.OrderBy(i => i.LocalizedDescription, StringComparer.CurrentCulture);
}
