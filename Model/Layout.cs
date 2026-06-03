using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a railway track layout containing stations, companies, and track stretches.
/// </summary>
/// <remarks>
/// A layout defines the physical infrastructure of a railway network, including
/// all stations, track connections between them, and the companies operating on the layout.
/// </remarks>
public sealed class Layout : IEquatable<Layout>
{
    /// <summary>
    /// Gets or sets the unique identifier for this layout.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this layout.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of companies operating on this layout.
    /// </summary>
    public ICollection<Company> Companies { get; set; }

    /// <summary>
    /// Gets or sets the collection of stations (operation locations) on this layout.
    /// </summary>
    public ICollection<OperationLocation> OperationLocations { get; set; }

    /// <summary>
    /// Gets or sets the collection of track stretches connecting stations.
    /// </summary>
    public ICollection<TrackStretch> TrackStretches { get; set; }

    /// <summary>
    /// Gets or sets the collection of timetable stretches for scheduling purposes.
    /// </summary>
    public ICollection<TimetableStretch> TimetableStretches { get; set; }

    /// <summary>
    /// Gets or sets the collection of <see cref="DispatchStretch"/>.
    /// </summary>
    public ICollection<DispatchStretch> DispatchStretches { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="Layout"/> with empty collections.
    /// </summary>
    public Layout()
    {
        Companies = [];
        OperationLocations = [];
        TrackStretches = [];
        TimetableStretches = [];
        DispatchStretches = [];
    }

    /// <inheritdoc/>
    public bool Equals(Layout? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Layout other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>
/// Provides extension methods for managing companies within a <see cref="Layout"/>.
/// </summary>
public static class LayoutCompanyExtensions
{
    extension(Layout? layout)
    {
        /// <summary>
        /// Determines whether the layout contains the specified company.
        /// </summary>
        /// <param name="company">The company to look for.</param>
        /// <returns><c>true</c> if the company exists in the layout; otherwise, <c>false</c>.</returns>
        public bool HasCompany(Company company) =>
            layout?.Companies.Any(c => c.Equals(company)) ?? false;

        /// <summary>
        /// Determines whether the layout contains a company with the specified signature.
        /// </summary>
        /// <param name="signature">The company signature to look for.</param>
        /// <returns><c>true</c> if a company with the signature exists; otherwise, <c>false</c>.</returns>
        public bool HasCompany(string signature) => layout?.Companies.Any(c => c.Signature.Equals(signature, StringComparison.OrdinalIgnoreCase)) ?? false;

        /// <summary>
        /// Finds a company by its signature.
        /// </summary>        /// <param name="signature">The company signature to find.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the company if found.</returns>
        public Maybe<Company> Company(string signature) =>
            new(layout?.Companies.SingleOrDefault(c => c.Signature.Equals(signature, StringComparison.OrdinalIgnoreCase)),
                $"Company with signature '{signature}' not found.");

        /// <summary>
        /// Adds a company to the layout.
        /// </summary>
        /// <param name="company">The company to add.</param>
        /// <returns>The added company.</returns>
        public Company Add(Company company)
        {
            layout = layout.ValueOrException(nameof(layout));
            company = company.ValueOrException(nameof(company));
            if (!layout.HasCompany(company))
            {
                company.Layout = layout;
                company.LayoutId = layout.Id;
                layout.Companies.Add(company);
            }
            return company;
        }
    }
}

/// <summary>
/// Provides extension methods for managing stations within a <see cref="Layout"/>.
/// </summary>
public static class LayoutOperationLocationExtensions
{
    extension(Layout layout)
    {
        /// <summary>
        /// Determines whether the layout contains the specified station.
        /// </summary>
        /// <param name="station">The station to look for.</param>
        /// <returns><c>true</c> if the station exists in the layout; otherwise, <c>false</c>.</returns>
        public bool HasStation(OperationLocation station) =>
            layout?.OperationLocations.Any(s => s.Equals(station)) ?? false;

        /// <summary>
        /// Determines whether the layout contains the specified track.
        /// </summary>        /// <param name="track">The track to look for.</param>
        /// <returns><c>true</c> if the track exists in the layout; otherwise, <c>false</c>.</returns>
        public bool HasTrack(StationTrack track) =>
            layout?.StationTracks().Any(t => t.Equals(track)) ?? false;

        /// <summary>
        /// Finds a station by its name or signature.
        /// </summary>        /// <param name="nameOrSignature">The station name or signature to find.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the station if found.</returns>
        public Maybe<OperationLocation> Station(string nameOrSignature) =>
           new(layout?.OperationLocations.SingleOrDefault(s => s.Signature.Equals(nameOrSignature, StringComparison.OrdinalIgnoreCase) || s.Name.Equals(nameOrSignature, StringComparison.OrdinalIgnoreCase)),
               Strings.ThereIsNoStationWithNameOrSignature, nameOrSignature);

        /// <summary>
        /// Gets all station tracks from all stations in the layout.
        /// </summary>        /// <returns>A collection of all station tracks.</returns>
        public IEnumerable<StationTrack> StationTracks() => layout is null ? [] : layout.OperationLocations.SelectMany(s => s.Tracks);

        /// <summary>
        /// Adds a station to the layout.
        /// </summary>
        /// <param name="station">The station to add.</param>
        /// <returns>The added station.</returns>
        public OperationLocation Add(OperationLocation station)
        {
            layout = layout.ValueOrException(nameof(layout));
            station = station.ValueOrException(nameof(station));
            if (!layout.HasStation(station))
            {
                station.Layout = layout;
                station.LayoutId = layout.Id;
                layout.OperationLocations.Add(station);
            }
            return station;
        }

        /// <summary>
        /// Adds a track stretch to the layout.
        /// </summary>
        /// <param name="stretch">The track stretch to add.</param>
        /// <returns>The added track stretch.</returns>
        public TrackStretch Add(TrackStretch stretch)
        {
            layout = layout.ValueOrException(nameof(layout));
            stretch = stretch.ValueOrException(nameof(stretch));
            if (!layout.TrackStretches.Contains(stretch))
            {
                layout.TrackStretches.Add(stretch);
            }
            return stretch;
        }

        /// <summary>
        /// Creates and adds a track stretch between two stations.
        /// </summary>
        /// <param name="id">The unique identifier for the track stretch.</param>
        /// <param name="fromStationName">The name of the starting station.</param>
        /// <param name="toStationName">The name of the ending station.</param>
        /// <param name="distance">The distance of the stretch.</param>
        /// <param name="tracksCount">The number of tracks in the stretch.</param>
        /// <returns>The created track stretch.</returns>
        public TrackStretch Add(int id, string fromStationName, string toStationName, double distance, int tracksCount)
        {
            var fromStation = layout.OperationLocations.Single(s => s.Name == fromStationName);
            var toStation = layout.OperationLocations.Single(s => s.Name == toStationName);
            var trackStretch = new TrackStretch(id, fromStation, toStation, distance, tracksCount);
            layout.Add(trackStretch);
            return trackStretch;
        }
    }
}

/// <summary>
/// Provides extension methods for managing timetable stretches within a <see cref="Layout"/>.
/// </summary>
public static class LayoutExtensions
{
    extension(Layout? layout)
    {
        /// <summary>
        /// Determines whether the layout contains a timetable stretch with the specified number.
        /// </summary>
        /// <param name="number">The timetable stretch number to look for.</param>
        /// <returns><c>true</c> if a timetable stretch with the number exists; otherwise, <c>false</c>.</returns>
        public bool HasTimetableStretch(string number) =>
                layout is not null && layout.TimetableStretches.Any(ts => ts.Number.Equals(number, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Finds a timetable stretch by its number.
        /// </summary>
        /// <param name="number">The timetable stretch number to find.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the timetable stretch if found.</returns>
        public Maybe<TimetableStretch> TimetableStretch(string number)
        {
            layout = layout.ValueOrException(nameof(layout));
            return new Maybe<TimetableStretch>(layout.TimetableStretches.SingleOrDefault(ts => ts.Number.Equals(number, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Adds a timetable stretch to the layout.
        /// </summary>
        /// <param name="timetableStretch">The timetable stretch to add.</param>
        /// <returns>The added timetable stretch.</returns>
        public TimetableStretch Add(TimetableStretch timetableStretch)
        {
            layout = layout.ValueOrException(nameof(layout));
            timetableStretch = timetableStretch.ValueOrException(nameof(timetableStretch));
            ArgumentNullException.ThrowIfNull(timetableStretch);
            if (!layout.TimetableStretches.Contains(timetableStretch))
            {
                layout.TimetableStretches.Add(timetableStretch);
            }
            return timetableStretch;
        }
    }
}

/// <summary>
/// Provides extension methods for managing track stretches within a <see cref="Layout"/>.
/// </summary>
public static class LayoutTracksExtensions
{
    extension(Layout layout)
    {
        /// <summary>
        /// Creates and adds a track stretch between two stations with full parameters.
        /// </summary>
        /// <param name="id">The unique identifier for the track stretch.</param>
        /// <param name="fromStationName">The name of the starting station.</param>
        /// <param name="toStationName">The name of the ending station.</param>
        /// <param name="distance">The distance of the stretch.</param>
        /// <param name="tracksCount">The number of tracks in the stretch.</param>
        /// <param name="speed">The maximum speed on the stretch.</param>
        /// <param name="time">The travel time in minutes.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the created track stretch if both stations exist.</returns>
        public Maybe<TrackStretch> Add(int id, string fromStationName, string toStationName, double distance, int tracksCount, int speed, int time)
        {
            var from = layout.Station(fromStationName);
            var to = layout.Station(toStationName);
            if (from.HasValue && to.HasValue)
                return new Maybe<TrackStretch>(layout.Add(new TrackStretch(id, from.Value, to.Value, distance, tracksCount, speed, time)));
            return new Maybe<TrackStretch>($"From {from} to {to}");
        }

        /// <summary>
        /// Finds a track stretch between two stations.
        /// </summary>
        /// <param name="from">The starting station.</param>
        /// <param name="to">The ending station.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the track stretch if found.</returns>
        public Maybe<TrackStretch> TrackStretch(OperationLocation from, OperationLocation to)
            => new(layout?.TrackStretches.SingleOrDefault(ts =>
                (ts.Start.Equals(from) && ts.End.Equals(to)) ||
                (ts.Start.Equals(to) && ts.End.Equals(from))),
                string.Format(CultureInfo.CurrentCulture, Strings.MoreThanOneStretchBetweenStations, from, to));


        /// <summary>
        /// Finds a track stretch between two stations by name or signature.
        /// </summary>
        /// <param name="fromStationNameOrSignature">The name or signature of the starting station.</param>
        /// <param name="toStationNameOrSignature">The name or signature of the ending station.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the track stretch if found.</returns>
        public Maybe<TrackStretch> TrackStretch(string fromStationNameOrSignature, string toStationNameOrSignature)
        {
            layout = layout.ValueOrException(nameof(layout));
            return new Maybe<TrackStretch>(
                layout.Between(fromStationNameOrSignature, toStationNameOrSignature).Concat(
                layout.Between(toStationNameOrSignature, fromStationNameOrSignature)).SingleOrDefault(),
                string.Format(CultureInfo.CurrentCulture, Strings.ThereIsNoStretchBetweenStation1AndStation2, fromStationNameOrSignature, toStationNameOrSignature));
        }

        private IEnumerable<TrackStretch> Between(string fromStationNameOrSignature, string? toStationNameOrSignature = null)
            => layout.TrackStretches.Where(ts =>
                (ts.Start.Name.EqualsCaseInsensitive(fromStationNameOrSignature)
                || ts.Start.Signature.EqualsCaseInsensitive(fromStationNameOrSignature))
                && (ts.End.Name.EqualsCaseInsensitive(toStationNameOrSignature)
                || ts.End.Signature.EqualsCaseInsensitive(toStationNameOrSignature)));
    }
}

/// <summary>
/// Extensions for managing <see cref="DispatchStretch"/>
/// </summary>
public static class LayoutDispatchStretchExtensions
{
    extension(Layout layout)
    {
        /// <summary>
        /// Creates dispatch stretches between all stations by following track stretches in their defined direction.
        /// The Id of each dispatch stretch is set to the Id of the first track stretch it comprises.
        /// </summary>
        /// <returns>The collection of dispatch stretches added to the layout.</returns>
        public ICollection<DispatchStretch> CreateDispatchStretches()
        {
            var result = new List<DispatchStretch>(layout.OperationLocations.Count);
            var stations = layout.OperationLocations.OfType<Station>().ToList();

            foreach (var fromStation in stations)
            {
                var reachable = FindDirectlyReachableStations(fromStation);
                foreach (var (toStation, firstStretchId) in reachable)
                {
                    result.Add(new DispatchStretch(firstStretchId, fromStation, toStation));
                }
            }
            return result;

            List<(Station station, int firstStretchId)> FindDirectlyReachableStations(Station from)
            {
                var reachable = new List<(Station, int)>();
                var visited = new HashSet<OperationLocation> { from };
                var queue = new Queue<(OperationLocation current, int firstStretchId)>();

                foreach (var stretch in layout.TrackStretches.Where(ts => ts.Start.Equals(from)))
                {
                    queue.Enqueue((stretch.End, stretch.Id));
                }

                while (queue.Count > 0)
                {
                    var (current, firstStretchId) = queue.Dequeue();
                    if (visited.Contains(current)) continue;
                    visited.Add(current);

                    if (current is Station station)
                    {
                        reachable.Add((station, firstStretchId));
                    }
                    else
                    {
                        foreach (var stretch in layout.TrackStretches.Where(ts => ts.Start.Equals(current)))
                        {
                            queue.Enqueue((stretch.End, firstStretchId));
                        }
                    }
                }
                return reachable;
            }
        }
    }
}

