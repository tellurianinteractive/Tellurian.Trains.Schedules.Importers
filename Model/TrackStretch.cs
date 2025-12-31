using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

public class TrackStretch : IEquatable<TrackStretch>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private TrackStretch()
    {
        Start = default!;
        End = default!;
        Layout = default!;
    }

    public TrackStretch(int id, OperationLocation start, OperationLocation end, double distance) : this(id, start, end, distance, 1, 100, (int)Math.Round(distance, 0)) { }
    public TrackStretch(int id, OperationLocation start, OperationLocation end, double distance, int tracksCount) : this(id, start, end, distance, tracksCount, 100, (int)Math.Round(distance, 0)) { }
    public TrackStretch(int id, OperationLocation start, OperationLocation end, double distance, int tracksCount, int speed, int time)
    {
        Id = id;
        Start = start.ValueOrException(nameof(start));
        End = end.ValueOrException(nameof(end));
        StartId = start.Id;
        EndId = end.Id;
        (!Start.Layout.Equals(end.Layout)).IfTrueThrows(nameof(end), $"Both {start} and {end} must be in the same layout.");
        Distance = distance;
        TracksCount = tracksCount;
        Speed = speed;
        Time = time;
        Layout = Start.Layout;
        LayoutId = Layout.Id;
    }

    public int Id { get; set; }

    // FK property for EF Core
    public int StartId { get; set; }
    [DataMember(IsRequired = true, Order = 2)]
    public OperationLocation Start { get; set; }

    // FK property for EF Core
    public int EndId { get; set; }
    [DataMember(IsRequired = true, Order = 3)]
    public OperationLocation End { get; set; }

    [DataMember(IsRequired = true, Order = 4)]
    public double Distance { get; set; }

    [DataMember(IsRequired = true, Order = 4)]
    public int TracksCount { get; set; }

    [DataMember(IsRequired = true, Order = 5)]
    public int Speed { get; set; }

    [DataMember(IsRequired = true, Order = 6)]
    public int Time { get; set; }

    // FK property for EF Core
    public int LayoutId { get; set; }
    public Layout Layout { get; set; }

    public IEnumerable<StretchPassing> Passings => [.. this.GetStretchPassings()];

    public bool Equals(TrackStretch? other) => other != null && Start.Equals(other.Start) && End.Equals(other.End);
    public override bool Equals(object? obj) => obj is TrackStretch other && Equals(other);
    public override int GetHashCode() => Start.GetHashCode() ^ End.GetHashCode();
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, Strings.StretchToString, Start, End);
}

public static class TrackStretchExtensions
{
    internal static IEnumerable<StretchPassing> GetStretchPassings(this TrackStretch me)
    {
        var trains = me.Start.Trains().Intersect(me.End.Trains());
        var result = new List<StretchPassing>(trains.Count());
        foreach (var train in trains)
        {
            var calls = train.Calls.ToArray();
            for (int i = 0; i < calls.Length - 1; i++)
            {
                if (calls[i].Station.Equals(me.Start) && calls[i + 1].Station.Equals(me.End)) result.Add(new StretchPassing(train, calls[i], calls[i + 1]));
                if (calls[i].Station.Equals(me.End) && calls[i + 1].Station.Equals(me.Start)) result.Add(new StretchPassing(train, calls[i], calls[i + 1]));
            }
        }
        return result;
    }
}

