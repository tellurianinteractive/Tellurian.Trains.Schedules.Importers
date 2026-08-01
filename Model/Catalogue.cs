namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Reconciles a catalogue — the one place an entry of its kind is stored — with the entries that are
/// actually referenced from elsewhere in a plan. Used for the train categories of a timetable and the
/// companies of a layout, which are stored once and referred to everywhere else by id alone.
/// </summary>
internal static class Catalogue
{
    /// <summary>
    /// Adds every referenced entry the catalogue does not already hold, then gives each entry an id
    /// that is unique and greater than zero.
    /// </summary>
    /// <remarks>
    /// Entries are gathered by reference, not by value: two entries may hold the same values — or,
    /// before the renumbering, the same id — and still be two entries. The catalogue is read first, so
    /// its order survives and only what is missing is appended.
    /// <para>
    /// Each entry that already has an id of its own keeps it, so identifiers that mean something
    /// outside the plan (a company's Module Registry id, for example) are never taken away. The rest
    /// are numbered from the lowest free id upwards, in two passes, so that whether an entry keeps its
    /// id never depends on which ids the renumbering has handed out already.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The catalogue entry type.</typeparam>
    /// <param name="catalogue">The catalogue to reconcile, in place.</param>
    /// <param name="referenced">The entries referenced from elsewhere; nulls are ignored.</param>
    /// <param name="idOf">Reads an entry's id.</param>
    /// <param name="setId">Writes an entry's id.</param>
    internal static void Reconcile<T>(
        ICollection<T> catalogue,
        IEnumerable<T?> referenced,
        Func<T, int> idOf,
        Action<T, int> setId)
        where T : class
    {
        var known = new HashSet<T>(ReferenceEqualityComparer.Instance);
        var entries = new List<T>();
        foreach (var entry in catalogue.Concat(referenced.OfType<T>()))
            if (known.Add(entry)) entries.Add(entry);

        var takenIds = new HashSet<int>();
        var keepsId = new HashSet<T>(ReferenceEqualityComparer.Instance);
        foreach (var entry in entries)
            if (idOf(entry) > 0 && takenIds.Add(idOf(entry))) keepsId.Add(entry);
        var nextId = 0;
        foreach (var entry in entries)
        {
            if (keepsId.Contains(entry)) continue;
            while (!takenIds.Add(++nextId)) { }
            setId(entry, nextId);
        }

        catalogue.Clear();
        foreach (var entry in entries) catalogue.Add(entry);
    }
}
