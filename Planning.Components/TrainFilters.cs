namespace Tellurian.Trains.Schedules.Planning.Components;

/// <summary>
/// Reusable predicates for filtering trains in drop-downs, kept in one place and exposed as static
/// extension members so they read from the type, e.g. <c>Train.All</c> and
/// <c>Train.CanHostCargoFlow</c>. Pass one to <see cref="ListItems.Trains"/>.
/// </summary>
public static class TrainFilters
{
    extension(Train)
    {
        /// <summary>Includes every train.</summary>
        public static Func<Train, bool> All => _ => true;

        /// <summary>
        /// Includes trains that can carry a cargo flow: a cargo (freight) train with a departure call
        /// that has a later arrival.
        /// </summary>
        public static Func<Train, bool> CanHostCargoFlow => t => t.IsCargo && t.DepartureCalls.Any(f => t.ArrivalCallsAfter(f).Any());
    }
}
