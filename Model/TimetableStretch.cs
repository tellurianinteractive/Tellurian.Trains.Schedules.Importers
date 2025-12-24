using System.Globalization;

namespace Tellurian.Trains.Schedules.Importers.Model;

public sealed record TimetableStretch : IEquatable<TimetableStretch>
{
    public int Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public string Description { get; }
    public ICollection<TrackStretch> Stretches { get; }

    public TimetableStretch(int id, string? number)
    {
        Id = id;
        Number = number.TextOrException(nameof(number), string.Format(CultureInfo.CurrentCulture, Resources.Strings.NumberOfObjectIsRequired, Resources.Strings.TimetableStretch));
        Description = string.Empty;
        Stretches = [];
    }

    public TimetableStretch(int id, string? number, string description) : this(id, number)
    {
        Description = description;
    }

    public Station Starts => Stretches.First().Start;

    public Station Ends => Stretches.Last().End;

    public IEnumerable<Station> Stations => Stretches.Select(s => s.Start).Concat([Stretches.Last().End]);

    public bool Equals(TimetableStretch? other) => other != null && Number.Equals(other?.Number, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => Number.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "{0}: {1}", Number, this.GetDescription());

}

public static class TimetableStretchExtensions
{
    public static string GetDescription(this TimetableStretch me) => me is null ? string.Empty : me.Description.TextOrDefault($"{me.Starts} - {me.Ends}");

    public static Maybe<Station> GetStation(this TimetableStretch me, Station station) =>
        new(me?.Stations.SingleOrDefault(s => s.Equals(station)), $"Station {station} is not in timetable stretch {me}.");
    public static Station Starts(this TimetableStretch me) =>
        me?.Stretches.Count > 0 ? me.Stretches.First().Start : throw new InvalidOperationException($"No stretch in {me}.");

    public static Station Ends(this TimetableStretch me) =>
       me?.Stretches.Count > 0 ? me.Stretches.Last().End : throw new InvalidOperationException($"No stretch in {me}.");

    public static double? DistanceToStation(this TimetableStretch me, Station station)
    {
        var to = me.GetStation(station);
        if (to.IsNone) return null;
        if (to.Value.Equals(me.Starts)) return 0.0;
        return me.Stretches.Where(s => !s.Start.Equals(to.Value)).Sum(s => s.Distance);
    }

    public static TrackStretch AddLast(this TimetableStretch timetableStretch, TrackStretch trackStretch)
    {
        var me = timetableStretch.ValueOrException(nameof(timetableStretch));
        ArgumentNullException.ThrowIfNull(trackStretch);
        {
            me.Stretches.Add(trackStretch);
            return trackStretch;
        }
    }
}
