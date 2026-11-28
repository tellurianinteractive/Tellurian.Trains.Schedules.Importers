using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Tellurian.Trains.Schedules.Model.Databases;

/// <summary>
/// The order of the plan's lists. A table has no order, so every item of a list has a column <see cref="Column"/> (in
/// the join table, where the list refers to what it holds rather than owning it), which is numbered from the list
/// before saving and puts the list back in order after reading. A plan read from the database is then written to a file
/// exactly as before.
/// </summary>
internal static class ListPositions
{
    /// <summary>The name of the shadow property and column that holds an item's position in its list.</summary>
    public const string Column = "ListPosition";

    /// <summary>The join tables of the lists that refer to what they hold, with their foreign keys.</summary>
    public static class Joins
    {
        public const string StationRegion = nameof(StationRegion);
        public const string TimetableStretchTrackStretch = nameof(TimetableStretchTrackStretch);
        public const string DispatchStretchTrackStretch = nameof(DispatchStretchTrackStretch);
        public const string DriverDutyTrainPart = nameof(DriverDutyTrainPart);

        public static (string Owner, string Item) Keys(string join) => join switch
        {
            StationRegion => ("StationId", "RegionId"),
            TimetableStretchTrackStretch => ("TimetableStretchId", "TrackStretchId"),
            DispatchStretchTrackStretch => ("DispatchStretchId", "TrackStretchId"),
            DriverDutyTrainPart => ("DriverDutyId", "TrainPartId"),
            _ => throw new ArgumentOutOfRangeException(nameof(join), join, null),
        };
    }

    private static readonly IOrderedList[] Lists =
    [
        Owned<Layout, Region>(l => l.Regions),
        Owned<Layout, OperationLocation>(l => l.OperationLocations),
        Owned<Layout, TrackStretch>(l => l.TrackStretches),
        Owned<OperationLocation, StationTrack>(l => l.Tracks),
        Owned<Timetable, Company>(t => t.Companies),
        Owned<Timetable, TimetableStretch>(t => t.TimetableStretches),
        Owned<Timetable, DispatchStretch>(t => t.DispatchStretches),
        Owned<Timetable, TrainCategory>(t => t.TrainCategories),
        Owned<Timetable, CargoFlowOptions>(t => t.CargoFlowOptions),
        Owned<Timetable, Train>(t => t.Trains),
        Owned<Train, StationCall>(t => t.Calls),
        Owned<Train, CargoFlowTrainPart>(t => t.CargoFlows),
        Owned<StationCall, CallNote>(c => c.Notes),
        Owned<CargoFlowOptions, Origin>(o => o.Origins),
        Owned<CargoFlowOptions, Destination>(o => o.Destinations),
        Owned<Plan, ScheduledObject>(p => p.ScheduledObjects),
        Owned<ScheduledObject, ScheduleAssignment>(o => o.ScheduleAssignments),
        Owned<ScheduledObject, ScheduledUnit>(o => o.Units),
        Owned<Plan, Schedule>(p => p.Schedules),
        Owned<Schedule, ScheduledTrainPart>(s => s.Parts),
        Owned<Plan, DriverDuty>(p => p.DriverDuties),
        Owned<DriverDuty, DriverDutyNote>(d => d.Notes),
        Referred<Station, Region>(Joins.StationRegion, s => s.Id, s => s.Regions, r => r.Id),
        Referred<TimetableStretch, TrackStretch>(Joins.TimetableStretchTrackStretch, s => s.Id, s => s.Stretches, t => t.Id),
        Referred<DispatchStretch, TrackStretch>(Joins.DispatchStretchTrackStretch, s => s.Id, s => s.Stretches, t => t.Id),
        Referred<DriverDuty, ScheduledTrainPart>(Joins.DriverDutyTrainPart, d => d.Id, d => d.Parts, p => p.Id),
    ];

    /// <summary>Gives every item of a list that is about to be saved its position in the list.</summary>
    public static void Number(ChangeTracker tracker)
    {
        tracker.DetectChanges();
        foreach (var list in Lists) list.Number(tracker);
    }

    /// <summary>
    /// Puts every list of what <paramref name="tracker"/> has read in the order it was saved in, and then stops tracking
    /// it all: reordering a list in place would otherwise look like items removed and added.
    /// </summary>
    public static void RestoreAndDetach(ChangeTracker tracker)
    {
        var orders = Lists.Select(l => l.Order(tracker)).ToList();
        tracker.Clear();
        foreach (var order in orders) order();
    }

    private static OwnedList<TOwner, TItem> Owned<TOwner, TItem>(Func<TOwner, ICollection<TItem>> items)
        where TOwner : class
        where TItem : class => new(items);

    private static ReferredList<TOwner, TItem> Referred<TOwner, TItem>(string join, Func<TOwner, int> ownerId, Func<TOwner, ICollection<TItem>> items, Func<TItem, int> itemId)
        where TOwner : class => new(join, ownerId, items, itemId);

    private static bool IsSaved(EntityEntry entry) => entry.State is EntityState.Added or EntityState.Modified;

    private static void Replace<TItem>(ICollection<TItem> list, List<TItem> ordered)
    {
        list.Clear();
        foreach (var item in ordered) list.Add(item);
    }

    private interface IOrderedList
    {
        void Number(ChangeTracker tracker);

        /// <summary>Reads the positions now, while tracked; the returned action reorders the lists.</summary>
        Action Order(ChangeTracker tracker);
    }

    /// <summary>A list that owns its items, each of which has the position column.</summary>
    private sealed class OwnedList<TOwner, TItem>(Func<TOwner, ICollection<TItem>> items) : IOrderedList
        where TOwner : class
        where TItem : class
    {
        public void Number(ChangeTracker tracker)
        {
            foreach (var owner in tracker.Entries<TOwner>().Where(IsSaved).Select(e => e.Entity))
            {
                var position = 0;
                foreach (var item in items(owner)) tracker.Context.Entry(item).Property(Column).CurrentValue = position++;
            }
        }

        public Action Order(ChangeTracker tracker)
        {
            var lists = tracker.Entries<TOwner>()
                .Select(e => items(e.Entity))
                .Select(list => (List: list, Ordered: list.OrderBy(i => (int)tracker.Context.Entry(i).Property(Column).CurrentValue!).ToList()))
                .ToList();
            return () =>
            {
                foreach (var (list, ordered) in lists) Replace(list, ordered);
            };
        }
    }

    /// <summary>A list that refers to items owned elsewhere, through a join table with the position column.</summary>
    private sealed class ReferredList<TOwner, TItem>(string join, Func<TOwner, int> ownerId, Func<TOwner, ICollection<TItem>> items, Func<TItem, int> itemId) : IOrderedList
        where TOwner : class
    {
        private readonly (string Owner, string Item) _keys = Joins.Keys(join);

        public void Number(ChangeTracker tracker)
        {
            var positions = tracker.Entries<TOwner>()
                .Select(e => e.Entity)
                .GroupBy(ownerId)
                .ToDictionary(g => g.Key, g => items(g.First()).Select((item, index) => (Id: itemId(item), Index: index)).GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.First().Index));
            foreach (var entry in JoinEntries(tracker).Where(IsSaved))
            {
                var owner = (int)entry.Property(_keys.Owner).CurrentValue!;
                var item = (int)entry.Property(_keys.Item).CurrentValue!;
                if (positions.TryGetValue(owner, out var list) && list.TryGetValue(item, out var position)) entry.Property(Column).CurrentValue = position;
            }
        }

        public Action Order(ChangeTracker tracker)
        {
            var positions = JoinEntries(tracker)
                .ToLookup(e => (int)e.Property(_keys.Owner).CurrentValue!, e => (Item: (int)e.Property(_keys.Item).CurrentValue!, Position: (int)e.Property(Column).CurrentValue!));
            var lists = tracker.Entries<TOwner>()
                .Select(e => e.Entity)
                .Select(owner =>
                {
                    var position = positions[ownerId(owner)].GroupBy(p => p.Item).ToDictionary(g => g.Key, g => g.First().Position);
                    var list = items(owner);
                    return (List: list, Ordered: list.OrderBy(i => position.GetValueOrDefault(itemId(i), int.MaxValue)).ToList());
                })
                .ToList();
            return () =>
            {
                foreach (var (list, ordered) in lists) Replace(list, ordered);
            };
        }

        private IEnumerable<EntityEntry<Dictionary<string, object>>> JoinEntries(ChangeTracker tracker) =>
            tracker.Entries<Dictionary<string, object>>().Where(e => e.Metadata.Name == join);
    }
}
