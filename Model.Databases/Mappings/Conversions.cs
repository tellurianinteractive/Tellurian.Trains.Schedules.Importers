using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Tellurian.Trains.Schedules.Model.Databases.Mappings;

/// <summary>The value converters shared by the mappings.</summary>
internal static class Conversions
{
    /// <summary>
    /// A <see cref="Timetables.Time"/> is a value object wrapping a <see cref="TimeSpan"/>, kept as its ticks. EF Core applies
    /// it to <c>Time?</c> too, such as the overridden times of a driver duty.
    /// </summary>
    public static readonly ValueConverter<Time, long> Time = new(
        v => v.Value.Ticks,
        v => Timetables.Time.FromTimeSpan(TimeSpan.FromTicks(v)));

    /// <summary>The sessions a train, assignment or duty runs on, kept as their bit pattern.</summary>
    public static readonly ValueConverter<Sessions, int> Sessions = new(
        v => v.Flags,
        v => new Sessions(v));

    /// <summary>
    /// The timetable's session catalogue: a small list of bit patterns referred to by value only, kept as one
    /// comma-separated column.
    /// </summary>
    public static readonly ValueConverter<IList<Sessions>, string> SessionsCatalogue = new(
        v => string.Join(',', v.Select(s => (int)s.Flags)),
        v => ParseSessionsCatalogue(v));

    /// <summary>The comparer the session catalogue needs, as a mutable collection.</summary>
    public static readonly ValueComparer<IList<Sessions>> SessionsCatalogueComparer = new(
        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
        v => v.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.Flags)),
        v => v.ToList());

    private static IList<Sessions> ParseSessionsCatalogue(string value) =>
        string.IsNullOrEmpty(value)
            ? []
            : [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => new Sessions(int.Parse(s, System.Globalization.CultureInfo.InvariantCulture)))];
}
