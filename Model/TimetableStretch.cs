using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

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
        Number = number.TextOrException(nameof(number), string.Format(CultureInfo.CurrentCulture, Strings.NumberOfObjectIsRequired, Strings.TimetableStretch));
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
    /// Gets the starting station of this timetable stretch.
    /// </summary>
    public OperationLocation Starts => Stretches.First().Start;

    /// <summary>
    /// Gets the ending station of this timetable stretch.
    /// </summary>
    public OperationLocation Ends => Stretches.Last().End;

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
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "{0}: {1}", Number, this.GetDescription());
}

/// <summary>
/// Provides extension methods for <see cref="TimetableStretch"/>.
/// </summary>
public static class TimetableStretchExtensions
{
    /// <summary>
    /// Gets the description of the timetable stretch, or generates one from start/end stations.
    /// </summary>
    /// <param name="me">The timetable stretch.</param>
    /// <returns>The description or a generated description.</returns>
    public static string GetDescription(this TimetableStretch me) => me is null ? string.Empty : me.Description.OrElse($"{me.Starts} - {me.Ends}");

    /// <summary>
    /// Finds a station along the timetable stretch.
    /// </summary>
    /// <param name="me">The timetable stretch.</param>
    /// <param name="station">The station to find.</param>
    /// <returns>A <see cref="Maybe{T}"/> containing the station if found.</returns>
    public static Maybe<OperationLocation> GetStation(this TimetableStretch me, OperationLocation station) =>
        new(me?.Stations.SingleOrDefault(s => s.Equals(station)), $"Station {station} is not in timetable stretch {me}.");

    /// <summary>
    /// Gets the starting station of the timetable stretch.
    /// </summary>
    /// <param name="me">The timetable stretch.</param>
    /// <returns>The starting station.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the stretch has no track stretches.</exception>
    public static OperationLocation Starts(this TimetableStretch me) =>
        me?.Stretches.Count > 0 ? me.Stretches.First().Start : throw new InvalidOperationException($"No stretch in {me}.");

    /// <summary>
    /// Gets the ending station of the timetable stretch.
    /// </summary>
    /// <param name="me">The timetable stretch.</param>
    /// <returns>The ending station.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the stretch has no track stretches.</exception>
    public static OperationLocation Ends(this TimetableStretch me) =>
       me?.Stretches.Count > 0 ? me.Stretches.Last().End : throw new InvalidOperationException($"No stretch in {me}.");

    /// <summary>
    /// Calculates the distance from the start of the timetable stretch to the specified station.
    /// </summary>
    /// <param name="me">The timetable stretch.</param>
    /// <param name="station">The target station.</param>
    /// <returns>The distance in kilometers, or null if the station is not found.</returns>
    public static double? DistanceToStation(this TimetableStretch me, OperationLocation station)
    {
        var to = me.GetStation(station);
        if (to.IsNone) return null;
        if (to.Value.Equals(me.Starts)) return 0.0;
        return me.Stretches.Where(s => !s.Start.Equals(to.Value)).Sum(s => s.Distance);
    }

    /// <summary>
    /// Adds a track stretch to the end of the timetable stretch.
    /// </summary>
    /// <param name="timetableStretch">The timetable stretch to add to.</param>
    /// <param name="trackStretch">The track stretch to add.</param>
    /// <returns>The added track stretch.</returns>
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
