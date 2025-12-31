using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

public sealed class TimetableStretch : IEquatable<TimetableStretch>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private TimetableStretch()
    {
        Number = string.Empty;
        Description = string.Empty;
        Stretches = [];
    }

    public TimetableStretch(int id, string? number)
    {
        Id = id;
        Number = number.TextOrException(nameof(number), string.Format(CultureInfo.CurrentCulture, Strings.NumberOfObjectIsRequired, Strings.TimetableStretch));
        Description = string.Empty;
        Stretches = [];
    }

    public TimetableStretch(int id, string? number, string description) : this(id, number)
    {
        Description = description;
    }

    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; }
    public ICollection<TrackStretch> Stretches { get; set; }

    public OperationLocation Starts => Stretches.First().Start;

    public OperationLocation Ends => Stretches.Last().End;

    public IEnumerable<OperationLocation> Stations => Stretches.Select(s => s.Start).Concat([Stretches.Last().End]);

    public bool Equals(TimetableStretch? other) => other != null && Number.Equals(other?.Number, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is TimetableStretch other && Equals(other);
    public override int GetHashCode() => Number.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "{0}: {1}", Number, this.GetDescription());
}

public static class TimetableStretchExtensions
{
    public static string GetDescription(this TimetableStretch me) => me is null ? string.Empty : me.Description.OrElse($"{me.Starts} - {me.Ends}");

    public static Maybe<OperationLocation> GetStation(this TimetableStretch me, OperationLocation station) =>
        new(me?.Stations.SingleOrDefault(s => s.Equals(station)), $"Station {station} is not in timetable stretch {me}.");
    public static OperationLocation Starts(this TimetableStretch me) =>
        me?.Stretches.Count > 0 ? me.Stretches.First().Start : throw new InvalidOperationException($"No stretch in {me}.");

    public static OperationLocation Ends(this TimetableStretch me) =>
       me?.Stretches.Count > 0 ? me.Stretches.Last().End : throw new InvalidOperationException($"No stretch in {me}.");

    public static double? DistanceToStation(this TimetableStretch me, OperationLocation station)
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
