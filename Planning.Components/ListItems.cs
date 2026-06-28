namespace Tellurian.Trains.Schedules.Planning.Components;

/// <summary>
/// Centralised projections of model objects to <see cref="ListboxItem"/>s for HTML select/options.
/// One method per object type owns that type's option text; pass a predicate from the matching
/// filter catalogue (e.g. <see cref="TrainFilters"/>) to limit which items are offered.
/// </summary>
public static class ListItems
{
    /// <summary>
    /// Projects trains to list items (value = id, description = <see cref="TrainExtensions.ListLabel"/>),
    /// ordered by train number. <paramref name="predicate"/> defaults to all trains.
    /// </summary>
    public static IEnumerable<ListboxItem> Trains(IEnumerable<Train> trains, Func<Train, bool>? predicate = null) =>
        trains.Where(predicate ?? Train.All)
              .OrderBy(t => t.Number)
              .Select(t => new ListboxItem(t.Id.ToString(), t.ListLabel));
}
