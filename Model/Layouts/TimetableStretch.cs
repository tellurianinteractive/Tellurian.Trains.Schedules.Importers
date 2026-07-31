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
    /// Gets or sets the colour used to draw this timetable stretch as a line in the layout's topology
    /// diagram and other schematic reports. Chosen from <c>Palette</c>, which contains only dark
    /// colours so a line stays legible drawn thin against a white background.
    /// </summary>
    public string Color { get; set; } = "#000000";

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
        /// The colours available for a timetable stretch's line: chosen so every entry is dark
        /// (see <c>Tellurian.Utilities.Web.ColorExtensions.IsDark</c>), keeping each line legible when
        /// drawn thin against a white background in the topology diagram and other schematic reports.
        /// </summary>
        public static IReadOnlyList<string> Palette =>
        [
            "#000000", // Black
            "#1a3a5c", // Dark blue
            "#8B0000", // Dark red
            "#006400", // Dark green
            "#4B0082", // Indigo
            "#8B4513", // Saddle brown
            "#2E8B57", // Sea green
            "#8B008B", // Dark magenta
            "#556B2F", // Dark olive
            "#B22222", // Firebrick
        ];

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
        /// <returns>The raw distance in metres, or null if the station is not found. Unscaled by
        /// <c>DistanceFactor</c>; see <c>DisplayedDistanceToStation</c> for the displayed kilometre.</returns>
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
        /// The displayed kilometre at the start of this timetable stretch. It is zero when the stretch begins at
        /// a terminal station (one that is not a through point of any other timetable stretch); when the stretch
        /// instead diverges from another line — its first station lies part-way along another timetable stretch —
        /// it is that station's displayed kilometre on the diverged-from stretch, so the branch keeps counting
        /// from where it leaves the main line. Divergences may chain (a branch off a branch); the offset then
        /// accumulates along the chain, and shared stations that form a cycle are guarded against. The raw
        /// metre offset is scaled by the layout's <c>TimeAndSpeedSettings.DistanceFactor</c> and rounded
        /// to a whole kilometre.
        /// </summary>
        public double StartKilometer => Kilometer(ComputeStartKilometer(stretch, []), stretch);

        /// <summary>
        /// The displayed kilometre at <paramref name="station"/>: its <c>DistanceToStation</c> along this
        /// stretch, shifted by the metre offset behind <c>StartKilometer</c> so a diverging branch continues
        /// counting from the kilometre of its junction on the line it leaves, then scaled by the layout's
        /// <c>TimeAndSpeedSettings.DistanceFactor</c> and rounded to a whole kilometre. Zero when the station
        /// is not on this stretch.
        /// </summary>
        /// <param name="station">The target station.</param>
        public double DisplayedDistanceToStation(OperationLocation station) =>
            stretch.DistanceToStation(station) is { } local ? Kilometer(ComputeStartKilometer(stretch, []) + local, stretch) : 0.0;

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
        /// Whether the timetable stretch may be reversed. Reversing turns each of its track stretches
        /// around, and a track stretch is shared with the layout and with any other timetable stretch
        /// that uses it, so reversal is only allowed when none of the track stretches belongs to
        /// another timetable stretch.
        /// </summary>
        public bool CanReverse => stretch.Stretches.Count > 0 && stretch.SharedStretches().Count == 0;

        /// <summary>
        /// The track stretches of this timetable stretch that are also used by another timetable stretch,
        /// and that therefore prevent it from being reversed. Empty when the stretch may be reversed.
        /// </summary>
        public IReadOnlyList<TrackStretch> SharedStretches()
        {
            if (stretch.Stretches.Count == 0) return [];
            var others = stretch.Stretches.First().Layout.TimetableStretches.Where(t => t.Id != stretch.Id).ToList();
            return [.. stretch.Stretches.Where(ts => others.Any(other => other.Stretches.Any(o => o.Id == ts.Id)))];
        }

        /// <summary>
        /// Reverses the timetable stretch, so that it runs from its end station to its start station:
        /// every track stretch is turned around and their order is reversed. Does nothing when the
        /// stretch may not be reversed; see <c>CanReverse</c>.
        /// </summary>
        public void Reverse()
        {
            if (!stretch.CanReverse) return;
            foreach (var trackStretch in stretch.Stretches) trackStretch.Reverse();
            stretch.Stretches = [.. stretch.Stretches.Reverse()];
        }

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

    // The layout's distance-display factor (Settings > Time & Speed), or 1 when the stretch has no track
    // stretches yet (e.g. a newly created, unsaved timetable stretch).
    private static double DistanceFactor(TimetableStretch stretch) =>
        stretch.Stretches.Count > 0 ? stretch.Stretches.First().Layout.Settings.TimeAndSpeed.DistanceFactor : 1.0;

    // Scales a raw metre distance to the displayed kilometre. Kilometres are whole numbers, so the scaled
    // value is rounded; the whole metre offset is added before scaling, so that a branch and the line it
    // leaves round the junction to the same kilometre.
    private static double Kilometer(double meters, TimetableStretch stretch) =>
        Math.Round(meters * DistanceFactor(stretch), MidpointRounding.AwayFromZero);

    // Walks the chain of stretches this one diverges from, summing each junction's distance. A stretch diverges
    // from another when its start station is a through point (distance > 0) of that other stretch; the lowest
    // stretch id is chosen when several qualify, matching how the timetable report attributes a shared station
    // to its owning stretch. The visited set stops a cycle of mutually diverging stretches from recursing forever.
    private static double ComputeStartKilometer(TimetableStretch stretch, HashSet<int> visited)
    {
        if (stretch.Stretches.Count == 0 || !visited.Add(stretch.Id)) return 0.0;
        var start = stretch.Starts;
        var divergedFrom = start.Layout.TimetableStretches
            .Where(s => s.Id != stretch.Id)
            .Select(s => (Stretch: s, Distance: s.DistanceToStation(start)))
            .Where(x => x.Distance is > 0.0)
            .OrderBy(x => x.Stretch.Id)
            .FirstOrDefault();
        return divergedFrom.Stretch is null ? 0.0 : ComputeStartKilometer(divergedFrom.Stretch, visited) + divergedFrom.Distance!.Value;
    }
}






