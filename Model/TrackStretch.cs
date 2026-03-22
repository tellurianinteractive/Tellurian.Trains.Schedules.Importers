using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a railway track section connecting two stations.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of <see cref="TrackStretch"/> with the specified values and default speed/time.
    /// </summary>
    /// <param name="id">The unique identifier for the track stretch.</param>
    /// <param name="start">The starting station.</param>
    /// <param name="end">The ending station.</param>
    /// <param name="distance">The distance in metres.</param>
    public TrackStretch(int id, OperationLocation start, OperationLocation end, double distance) : this(id, start, end, distance, 1, 100, (int)Math.Round(distance, 0)) { }

    /// <summary>
    /// Initializes a new instance of <see cref="TrackStretch"/> with the specified values and default speed/time.
    /// </summary>
    /// <param name="id">The unique identifier for the track stretch.</param>
    /// <param name="start">The starting station.</param>
    /// <param name="end">The ending station.</param>
    /// <param name="distance">The distance in metres.</param>
    /// <param name="tracksCount">The number of tracks in the stretch.</param>
    public TrackStretch(int id, OperationLocation start, OperationLocation end, double distance, int tracksCount) : this(id, start, end, distance, tracksCount, 100, (int)Math.Round(distance, 0)) { }

    /// <summary>
    /// Initializes a new instance of <see cref="TrackStretch"/> with all specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the track stretch.</param>
    /// <param name="start">The starting station.</param>
    /// <param name="end">The ending station.</param>
    /// <param name="distance">The distance in metres.</param>
    /// <param name="tracksCount">The number of tracks in the stretch.</param>
    /// <param name="speed">The maximum speed in km/h.</param>
    /// <param name="time">The travel time in minutes.</param>
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

    /// <summary>
    /// Gets or sets the unique identifier for this track stretch.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the starting station.
    /// </summary>
    public int StartId { get; set; }

    /// <summary>
    /// Gets or sets the starting station.
    /// </summary>
    [DataMember(IsRequired = true, Order = 2)]
    public OperationLocation Start { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the ending station.
    /// </summary>
    public int EndId { get; set; }

    /// <summary>
    /// Gets or sets the ending station.
    /// </summary>
    [DataMember(IsRequired = true, Order = 3)]
    public OperationLocation End { get; set; }

    /// <summary>
    /// Gets or sets the distance of this stretch in kilometers.
    /// </summary>
    [DataMember(IsRequired = true, Order = 4)]
    public double Distance { get; set; }

    /// <summary>
    /// Gets or sets the number of tracks in this stretch.
    /// </summary>
    [DataMember(IsRequired = true, Order = 4)]
    public int TracksCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum speed on this stretch in km/h.
    /// </summary>
    [DataMember(IsRequired = true, Order = 5)]
    public int Speed { get; set; }

    /// <summary>
    /// Gets or sets the travel time for this stretch in minutes.
    /// </summary>
    [DataMember(IsRequired = true, Order = 6)]
    public int Time { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the layout.
    /// </summary>
    public int LayoutId { get; set; }

    /// <summary>
    /// Gets or sets the layout this track stretch belongs to.
    /// </summary>
    public Layout Layout { get; set; }

    /// <summary>
    /// Gets all train passings on this track stretch.
    /// </summary>
    public IEnumerable<StretchPassing> Passings => [.. this.GetStretchPassings()];

    /// <inheritdoc/>
    public bool Equals(TrackStretch? other) => other != null && Start.Equals(other.Start) && End.Equals(other.End);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TrackStretch other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Start.GetHashCode() ^ End.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, Strings.StretchToString, Start, End);
}

/// <summary>
/// Provides extension methods for <see cref="TrackStretch"/>.
/// </summary>
public static class TrackStretchExtensions
{
    /// <summary>
    /// Gets all train passings on the track stretch.
    /// </summary>
    /// <param name="me">The track stretch.</param>
    /// <returns>A collection of stretch passings.</returns>
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
