using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Represents a named railway line or route consisting of multiple track stretches.
/// </summary>
/// <remarks>
/// A timetable stretch is used for organizing trains on a particular line for scheduling
/// and graphical timetable display purposes.
/// </remarks>
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

    /// <summary>
    /// Initializes a new instance of <see cref="TimetableStretch"/> with the specified id and number.
    /// </summary>
    /// <param name="id">The unique identifier for the timetable stretch.</param>
    /// <param name="number">The line number or designation.</param>
    public TimetableStretch(int id, string? number)
    {
        Id = id;
        Number = number.ValueOrException(nameof(number), string.Format(CultureInfo.CurrentCulture, Strings.NumberOfObjectIsRequired, Strings.TimetableStretch));
        Description = string.Empty;
        Stretches = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="TimetableStretch"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the timetable stretch.</param>
    /// <param name="number">The line number or designation.</param>
    /// <param name="description">A description of the timetable stretch.</param>
    public TimetableStretch(int id, string? number, string description) : this(id, number)
    {
        Description = description;
    }

    /// <summary>
    /// Gets or sets the unique identifier for this timetable stretch.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the line number or designation.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of this timetable stretch.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the collection of track stretches that make up this timetable stretch.
    /// </summary>
    public ICollection<TrackStretch> Stretches { get; set; }

    /// <summary>
    /// Gets all stations along this timetable stretch in order.
    /// </summary>
    public IEnumerable<OperationLocation> Stations => Stretches.Select(s => s.Start).Concat([Stretches.Last().End]);

    /// <inheritdoc/>
    public bool Equals(TimetableStretch? other) => other != null && Number.Equals(other?.Number, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TimetableStretch other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Number.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "{0}", this.ForwardDescription);
}

/// <summary>
/// Provides extension methods for <see cref="TimetableStretch"/>.
/// </summary>
public static class TimetableStretchExtensions
{
    extension(TimetableStretch stretch)
    {
        /// <summary>
        /// Includes stretch number, start and end station and an optional additional description.
        /// </summary>
        public string ForwardDescription => $"{stretch.Number}: {stretch.Description} {stretch.Starts}-{stretch.Ends}".Trim();
        /// <summary>
        /// Includes stretch number, start and end station and an optional additional description.
        /// </summary>
        public string BackwardDescription => $"{stretch.Number}: {stretch.Description} {stretch.Ends}-{stretch.Starts}".Trim();
        /// <summary>
        /// Finds a station along the timetable stretch. A station may occur more than once on a
        /// stretch that revisits it (reversing or branching lines); the first occurrence is returned.
        /// </summary>
        /// <param name="station">The station to find.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the station if found.</returns>
        public Maybe<OperationLocation> GetStation(OperationLocation station) =>
            new(stretch?.Stations.FirstOrDefault(s => s.Equals(station)), $"Station {station} is not in timetable stretch {stretch}.");

        /// <summary>
        /// Gets the starting station of the timetable stretch.
        /// </summary>
        /// <returns>The starting station.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the stretch has no track stretches.</exception>
        public OperationLocation Starts =>
            stretch?.Stretches.Count > 0 ? stretch.Stretches.First().Start : throw new InvalidOperationException($"No stretch in {stretch}.");

        /// <summary>
        /// Gets the ending station of the timetable stretch.
        /// </summary>
        /// <returns>The ending station.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the stretch has no track stretches.</exception>
        public OperationLocation Ends =>
           stretch?.Stretches.Count > 0 ? stretch.Stretches.Last().End : throw new InvalidOperationException($"No stretch in {stretch}.");

        /// <summary>
        /// Calculates the distance from the start of the timetable stretch to the specified station.
        /// </summary>
        /// <param name="station">The target station.</param>
        /// <returns>The distance in kilometers, or null if the station is not found.</returns>
        public double? DistanceToStation(OperationLocation station)
        {
            var to = stretch.GetStation(station);
            if (to.IsNone) return null;
            if (to.Value.Equals(stretch.Starts)) return 0.0;
            var distance = 0.0;
            foreach (var s in stretch.Stretches)
            {
                if (s.Start.Equals(to.Value)) break;
                distance += s.Distance;
            }
            return distance;
        }
        /// <summary>
        /// The first track stretch that does not continue from the previous one (its <c>Start</c> differs
        /// from the previous stretch's <c>End</c>), or <see langword="null"/> when the stretches form one
        /// continuous, contiguous route.
        /// </summary>
        public TrackStretch? FirstDiscontinuity()
        {
            var list = stretch.Stretches.ToList();
            for (var i = 1; i < list.Count; i++)
                if (!list[i - 1].End.Equals(list[i].Start)) return list[i];
            return null;
        }

        /// <summary>
        /// Whether the track stretches form one continuous route, each continuing from the previous one.
        /// </summary>
        public bool IsContiguous => stretch.FirstDiscontinuity() is null;

        /// <summary>
        /// Whether the given track stretch could be appended after the current last one: it must continue
        /// from where this stretch currently ends (or be the very first stretch added).
        /// </summary>
        /// <param name="trackStretch">The candidate track stretch.</param>
        public bool CanAppend(TrackStretch trackStretch) =>
            stretch.Stretches.Count == 0 || stretch.Stretches.Last().End.Equals(trackStretch.Start);

        /// <summary>
        /// Adds a track stretch to the end of the timetable stretch.
        /// </summary>
        /// <param name="trackStretch">The track stretch to add.</param>
        /// <returns>The added track stretch.</returns>
        public TrackStretch AddLast(TrackStretch trackStretch)
        {
            var timetableStretch = stretch.ValueOrException(nameof(TimetableStretch));
            ArgumentNullException.ThrowIfNull(trackStretch);
            {
                stretch.Stretches.Add(trackStretch);
                return trackStretch;
            }
        }
    }
}






