using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Layouts;

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
    /// Gets or sets the configurable settings for this layout, grouped by purpose.
    /// </summary>
    public LayoutSettings Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets the catalogue of countries saved with this layout. A <see cref="Company"/>,
    /// <see cref="Region"/> and the layout's default country all reference an entry here by its
    /// <see cref="Country.Id"/>, so a saved layout is self-contained. Seeded from the static
    /// <c>Country.Countries</c> catalogue, filtered by the layout's theme.
    /// </summary>
    public ICollection<Country> Countries { get; set; }

    /// <summary>
    /// Gets or sets the collection of companies operating on this layout.
    /// </summary>
    public ICollection<Company> Companies { get; set; }

    /// <summary>
    /// Gets or sets the catalogue of regions (domestic regions and foreign countries) available on
    /// this layout for cargo flow routing. A <see cref="Station"/>'s <see cref="Station.Regions"/>
    /// references a subset of these. Declared before <see cref="OperationLocations"/>, so a region is
    /// written here — the catalogue it belongs to — rather than under the first station using it.
    /// </summary>
    public ICollection<Region> Regions { get; set; }

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
    /// Gets or sets where the planner has put operation locations in the topology diagram. Only the
    /// locations that have been moved are here; the rest are placed automatically. See
    /// <see cref="TopologyPosition"/>.
    /// </summary>
    public ICollection<TopologyPosition> TopologyPositions { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="Layout"/> with empty collections.
    /// </summary>
    public Layout()
    {
        Countries = [];
        Companies = [];
        Regions = [];
        OperationLocations = [];
        TrackStretches = [];
        TimetableStretches = [];
        DispatchStretches = [];
        TopologyPositions = [];
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
/// Provides extension methods for managing the country catalogue saved within a <see cref="Layout"/>.
/// </summary>
public static class LayoutCountryExtensions
{
    extension(Layout layout)
    {
        /// <summary>
        /// Gets the default <see cref="Country"/> for the layout (if any)
        /// </summary>
        public Country? DefaultCountry =>
            layout.CountryById(layout.Settings.Identity.DefaultCountryId);
        /// <summary>
        /// Resolves a country by its <see cref="Country.Id"/>, preferring the layout's saved
        /// <see cref="Layout.Countries"/> and falling back to the static catalogue. Returns
        /// <c>null</c> when no country matches.
        /// </summary>
        /// <param name="id">The country id, or <c>null</c> for "no country".</param>
        public Country? CountryById(int? id) =>
            id is int countryId
                ? layout.Countries.FirstOrDefault(c => c.Id == countryId) ?? Country.ById(countryId)
                : null;

        /// <summary>
        /// Ensures the countries the layout actually uses are present in <see cref="Layout.Countries"/>:
        /// the default country and any country referenced by a company or region. The catalogue is a
        /// curated set (the user adds more on the Countries tab), so this only fills in references — it
        /// does not bulk-seed the whole theme. Idempotent. Returns <c>true</c> when an entry was added.
        /// </summary>
        public bool EnsureCountries()
        {
            var before = layout.Countries.Count;
            void AddIfMissing(int? id)
            {
                if (id is int countryId && layout.Countries.All(c => c.Id != countryId) && Country.ById(countryId) is { } country)
                    layout.Countries.Add(country);
            }
            AddIfMissing(layout.Settings.Identity.DefaultCountryId);
            foreach (var company in layout.Companies) AddIfMissing(company.CountryId);
            foreach (var region in layout.Regions) AddIfMissing(region.CountryId);
            return layout.Countries.Count != before;
        }
    }
}

/// <summary>
/// Provides extension methods for managing the region catalogue within a <see cref="Layout"/>.
/// </summary>
public static class LayoutRegionExtensions
{
    extension(Layout? layout)
    {
        /// <summary>
        /// Determines whether the layout's catalogue contains a region with the specified id.
        /// </summary>
        /// <param name="id">The region id to look for.</param>
        /// <returns><c>true</c> if a region with the id exists; otherwise, <c>false</c>.</returns>
        public bool HasRegion(int id) => layout?.Regions.Any(r => r.Id == id) ?? false;

        /// <summary>
        /// Adds a region to the layout's catalogue if a region with the same id is not already present.
        /// </summary>
        /// <param name="region">The region to add.</param>
        /// <returns>The added (or already present) region.</returns>
        public Region Add(Region region)
        {
            layout = layout.ValueOrException(nameof(layout));
            region = region.ValueOrException(nameof(region));
            if (!layout.HasRegion(region.Id)) layout.Regions.Add(region);
            return region;
        }

        /// <summary>
        /// Reconciles the region catalogue (<see cref="Layout.Regions"/>) with the regions the layout's
        /// stations actually hold: a region a station carries that the catalogue does not know is added
        /// to it, and every region is then given an id that is unique and greater than zero. Call this
        /// whenever a plan is written or imported.
        /// </summary>
        /// <remarks>
        /// A region is stored once, in the catalogue, and a station keeps only its id (see
        /// <see cref="Station.RegionIds"/>). A region the catalogue did not hold would therefore be
        /// written nowhere at all and lost on the next read, and two regions left sharing an id would
        /// read back as one. The Regions tab is the only way to make a region and puts it straight into
        /// the catalogue, so this normally finds nothing to do; it is what makes that a guarantee rather
        /// than a habit.
        /// </remarks>
        public void RebuildRegions()
        {
            layout = layout.ValueOrException(nameof(layout));
            Catalogue.Reconcile(
                layout.Regions,
                layout.OperationLocations.OfType<Station>().SelectMany(station => station.Regions),
                region => region.Id,
                (region, id) => region.Id = id);
        }

        /// <summary>
        /// Re-establishes the regions each station is read without, looking up every id in
        /// <see cref="Station.RegionIds"/> in the layout's own catalogue. Call this whenever a plan is
        /// read, once the whole layout is there.
        /// </summary>
        /// <remarks>
        /// The catalogue is the only place a region belongs, so it is the only place one is written
        /// (see <c>PlanJson</c>) and a station keeps just the ids. Regions that survived the reading —
        /// a plan written by an earlier version stored them whole on the station — are left alone, so
        /// nothing is lost by resolving against a catalogue that version filled with references only.
        /// An id the catalogue does not know is dropped; the Regions tab refuses to delete a region a
        /// station still uses, so a layout does not get into that state.
        /// </remarks>
        public void ResolveCatalogueReferences()
        {
            layout = layout.ValueOrException(nameof(layout));
            foreach (var station in layout.OperationLocations.OfType<Station>())
            {
                foreach (var id in station.UnresolvedRegionIds)
                {
                    if (layout.Regions.FirstOrDefault(region => region.Id == id) is { } region)
                        station.Add(region);
                }
                station.UnresolvedRegionIds = [];
            }
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
        /// Makes sure every location that exchanges passengers has somewhere to exchange them: where not
        /// one of its tracks has a platform, all of them are given
        /// <see cref="StationTrack.DefaultPlatformLength"/>. Returns <c>true</c> when something changed.
        /// Idempotent. Call this whenever a plan is read or imported, before it is displayed.
        /// </summary>
        /// <remarks>
        /// Platform lengths came after the plans that have none, and every track of a passenger location
        /// served passengers before there were any. Giving them all the minimum length keeps such a plan
        /// working exactly as it did, and leaves the planner to shorten or clear the tracks that in truth
        /// have no platform. A location where a platform has already been recorded is left alone — the
        /// planner has said which of its tracks have one, and the rest having none is the answer, not a
        /// gap to fill. So is a location that exchanges no passengers, where a platform means nothing.
        /// </remarks>
        public bool EnsurePlatforms()
        {
            layout = layout.ValueOrException(nameof(layout));
            var changed = false;
            foreach (var location in layout.OperationLocations) changed |= location.EnsurePlatforms();
            return changed;
        }

        /// <summary>
        /// The locations directly connected to <paramref name="location"/> by a track stretch, either way
        /// round, in name order. These are the only places a train calling here can have come from or go
        /// on to, so they are what a track's route may name (see
        /// <see cref="StationTrack.PreviousLocationId"/>).
        /// </summary>
        /// <param name="location">The location whose neighbours are wanted.</param>
        public IReadOnlyList<OperationLocation> NeighboursOf(OperationLocation? location)
        {
            if (layout is null || location is null) return [];
            return
            [
                .. layout.TrackStretches
                    .Where(stretch => stretch.Start.Equals(location) || stretch.End.Equals(location))
                    .Select(stretch => stretch.Start.Equals(location) ? stretch.End : stretch.Start)
                    .Where(neighbour => !neighbour.Equals(location))
                    .Distinct()
                    .OrderBy(neighbour => neighbour.Name, StringComparer.CurrentCulture)
            ];
        }

        /// <summary>
        /// Clears every track route naming <paramref name="location"/> (see
        /// <see cref="StationTrack.PreviousLocationId"/>), so no track is left reserved for trains to or
        /// from somewhere the layout no longer has. Call it when a location is removed. Returns
        /// <c>true</c> when a track was changed.
        /// </summary>
        /// <param name="location">The location being removed.</param>
        public bool ForgetTrackRoutesTo(OperationLocation? location)
        {
            if (layout is null || location is null) return false;
            var changed = false;
            foreach (var track in layout.StationTracks())
            {
                if (track.PreviousLocationId == location.Id) { track.PreviousLocationId = null; changed = true; }
                if (track.NextLocationId == location.Id) { track.NextLocationId = null; changed = true; }
                if (!track.HasRoute) track.AppliesInBothDirections = false;
            }
            return changed;
        }

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
        /// The track stretch that already joins the two operating locations, or <see langword="null"/>
        /// when nothing joins them yet. A stretch is bidirectional, so one defined the opposite way joins
        /// the same pair and is found too; adding a second stretch between the same pair — in either
        /// direction — would duplicate infrastructure that already exists.
        /// </summary>
        /// <param name="from">One end.</param>
        /// <param name="to">The other end.</param>
        /// <param name="excluding">
        /// A stretch to disregard, typically the one being edited: a stretch is never a duplicate of itself.
        /// Compared by reference, so another stretch with the same endpoints is still reported.
        /// </param>
        /// <remarks>
        /// Tolerates a layout that already holds more than one stretch between the same pair — a fault of
        /// its own — by returning the first, rather than failing the caller asking about it.
        /// </remarks>
        public TrackStretch? StretchBetween(OperationLocation from, OperationLocation to, TrackStretch? excluding = null) =>
            layout.TrackStretches.FirstOrDefault(ts =>
                !ReferenceEquals(ts, excluding) &&
                ((ts.Start.Equals(from) && ts.End.Equals(to)) ||
                 (ts.Start.Equals(to) && ts.End.Equals(from))));

        /// <summary>
        /// Determines whether a track stretch joins the two operating locations, so a train can run
        /// directly between them. A stretch is bidirectional, so the order of the arguments does not matter.
        /// </summary>
        /// <param name="from">One end.</param>
        /// <param name="to">The other end.</param>
        /// <remarks>
        /// Answers the question <c>TrackStretch(from, to)</c> does, but tolerates a layout that has more
        /// than one stretch between the same pair — a fault of its own, not a reason for a caller asking
        /// about connectivity to fail.
        /// </remarks>
        public bool IsConnected(OperationLocation from, OperationLocation to) =>
            layout.StretchBetween(from, to) is not null;


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
    extension(OperationLocation location)
    {
        /// <summary>
        /// Whether this location is a <em>dispatch endpoint</em>: somewhere with a person on duty who
        /// clears trains in and out. That is a manned station, and a shadow station whether manned or
        /// not — off-layout staging is always worked by someone, and trains have to be cleared onto and
        /// off the modelled railway there like anywhere else.
        /// </summary>
        /// <remarks>
        /// The single definition of the term. A dispatch stretch runs between two of these
        /// (see <c>CreateDispatchStretches</c>), and the station dispatch list is printed for each of
        /// them — the two must agree, or a station would be handed a list naming neighbours it has no
        /// dispatch stretch to.
        /// </remarks>
        public bool IsDispatchEndpoint =>
            location is Station { IsManned: true } or Station { IsShadow: true };
    }

    extension(Layout layout)
    {
        /// <summary>
        /// The layout's dispatch endpoints in layout order — the stations a
        /// <see cref="DispatchStretch"/> can begin or end at. See <c>IsDispatchEndpoint</c>.
        /// </summary>
        public IEnumerable<Station> DispatchEndpoints =>
            layout.OperationLocations.OfType<Station>().Where(station => station.IsDispatchEndpoint);

        /// <summary>
        /// The dispatch endpoints reachable from <paramref name="station"/> over a single dispatch
        /// stretch — the neighbours its dispatcher talks to, and so the ones whose phone numbers belong
        /// on its dispatch list.
        /// </summary>
        /// <remarks>
        /// Read from <see cref="Layout.DispatchStretches"/> and from nowhere else — never derived on the
        /// fly. Those stretches are the planner's own answer to who talks to whom, and a station handed a
        /// list naming neighbours it has no dispatch stretch to would be told to ring the wrong people.
        /// A layout with none recorded yields none here; the planner generates them on the Stretches tab.
        /// </remarks>
        /// <param name="station">The station whose dispatch neighbours are wanted.</param>
        public IReadOnlyList<Station> DispatchNeighboursOf(Station station)
        {
            layout = layout.ValueOrException(nameof(layout));
            if (station is null) return [];

            return
            [
                .. layout.DispatchStretches
                    .Where(stretch => stretch.From.Equals(station) || stretch.To.Equals(station))
                    .Select(stretch => stretch.From.Equals(station) ? stretch.To : stretch.From)
                    .Where(neighbour => !neighbour.Equals(station))
                    .DistinctBy(neighbour => neighbour.Signature, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(neighbour => neighbour.Name, StringComparer.CurrentCulture)
            ];
        }
    }

    extension(Layout layout)
    {
        /// <summary>
        /// Creates dispatch stretches between dispatch endpoints (manned or shadow stations) by following
        /// track stretches in their defined direction. Unmanned, non-shadow locations are passed through,
        /// so a dispatch stretch may span several contiguous track stretches. Each dispatch stretch records
        /// the ordered track stretches it comprises, and its Id is the Id of the first of those.
        /// </summary>
        /// <returns>The collection of dispatch stretches added to the layout.</returns>
        public ICollection<DispatchStretch> CreateDispatchStretches()
        {
            var result = new List<DispatchStretch>(layout.OperationLocations.Count);
            var endpoints = layout.DispatchEndpoints.ToList();

            foreach (var fromStation in endpoints)
            {
                foreach (var path in FindDirectlyReachableEndpoints(fromStation))
                {
                    result.Add(new DispatchStretch(path[0].Id, path));
                }
            }
            return result;

            // A dispatch stretch begins and ends at a manned or shadow station; everything in between
            // (unmanned stations, signal-controlled and other locations) is traversed without stopping.
            static bool IsDispatchEndpoint(OperationLocation location) =>
                location is Station { IsManned: true } or Station { IsShadow: true };

            List<List<TrackStretch>> FindDirectlyReachableEndpoints(Station from)
            {
                var reachable = new List<List<TrackStretch>>();
                var visited = new HashSet<OperationLocation> { from };
                var queue = new Queue<List<TrackStretch>>();

                foreach (var stretch in layout.TrackStretches.Where(ts => ts.Start.Equals(from)))
                {
                    queue.Enqueue([stretch]);
                }

                while (queue.Count > 0)
                {
                    var path = queue.Dequeue();
                    var current = path[^1].End;
                    if (visited.Contains(current)) continue;
                    visited.Add(current);

                    if (IsDispatchEndpoint(current))
                    {
                        reachable.Add(path);
                    }
                    else
                    {
                        foreach (var stretch in layout.TrackStretches.Where(ts => ts.Start.Equals(current)))
                        {
                            queue.Enqueue([.. path, stretch]);
                        }
                    }
                }
                return reachable;
            }
        }
    }
}

/// <summary>
/// The concrete kind of an <see cref="OperationLocation"/>, used when changing a location's type.
/// </summary>
public enum OperationLocationKind
{
    /// <summary>A <see cref="Station"/>.</summary>
    Station,

    /// <summary>A <see cref="SignalControlledLocation"/>.</summary>
    SignalControlled,

    /// <summary>An <see cref="IndustrialArea"/>.</summary>
    IndustrialArea,

    /// <summary>An <see cref="OtherLocation"/>.</summary>
    Other,
}

/// <summary>
/// Extensions for changing the type of an existing <see cref="OperationLocation"/>.
/// </summary>
public static class LayoutOperationLocationConversionExtensions
{
    extension(Layout layout)
    {
        /// <summary>
        /// Finds the station immediately preceding <paramref name="location"/> in the track network — the
        /// start of a track stretch ending at it. Falls back to any adjacent station, then to any other
        /// station in the layout. When several qualify, the first is returned. Returns <c>null</c> when the
        /// layout has no other station.
        /// </summary>
        /// <param name="location">The location whose preceding station to find.</param>
        public Station? PrecedingStation(OperationLocation location)
        {
            var incoming = layout.TrackStretches.Where(s => s.End.Equals(location)).Select(s => s.Start).OfType<Station>();
            var outgoing = layout.TrackStretches.Where(s => s.Start.Equals(location)).Select(s => s.End).OfType<Station>();
            var others = layout.OperationLocations.OfType<Station>().Where(s => !s.Equals(location));
            return incoming.Concat(outgoing).Concat(others).FirstOrDefault();
        }

        /// <summary>
        /// Replaces the operation location equal to <paramref name="existing"/> with a new one of the given
        /// <paramref name="kind"/>, preserving the shared state (name, signature, owner, tracks, timings, …)
        /// and repointing every reference (tracks, track stretches, controlled-by links) to the replacement.
        /// Type-specific data the new kind cannot hold (a station's regions, a signal location's controller)
        /// is dropped. When changing to a <see cref="SignalControlledLocation"/>, its controlling station
        /// defaults to the <see cref="PrecedingStation"/>. Dispatch stretches are derived and may need
        /// regenerating afterwards. Returns the replacement, or <paramref name="existing"/> unchanged when it
        /// is already of that kind.
        /// </summary>
        /// <param name="existing">The location to convert.</param>
        /// <param name="kind">The kind to convert it to.</param>
        public OperationLocation ChangeOperationLocationType(OperationLocation existing, OperationLocationKind kind)
        {
            existing = existing.ValueOrException(nameof(existing));
            if (IsOfKind(existing, kind)) return existing;

            OperationLocation replacement = kind switch
            {
                OperationLocationKind.Station => new Station(existing.Id, existing.Name, existing.Signature),
                OperationLocationKind.SignalControlled => new SignalControlledLocation(existing.Id, existing.Name, existing.Signature)
                {
                    ControlledBy = layout.PrecedingStation(existing),
                },
                OperationLocationKind.IndustrialArea => new IndustrialArea(existing.Id, existing.Name, existing.Signature),
                _ => new OtherLocation(existing.Id, existing.Name, existing.Signature),
            };

            // Carry over the shared base state and re-home the tracks (StationCall.Station follows Track.Station).
            replacement.CountryId = existing.CountryId;
            replacement.Owner = existing.Owner;
            replacement.IsSignal = existing.IsSignal;
            replacement.HideMeets = existing.HideMeets;
            replacement.HidePassings = existing.HidePassings;
            replacement.IsChangingTrainDirectionPossible = existing.IsChangingTrainDirectionPossible;
            replacement.LockKey = existing.LockKey;
            replacement.Timings = existing.Timings;
            replacement.Layout = existing.Layout;
            replacement.LayoutId = existing.LayoutId;
            replacement.Tracks = existing.Tracks;
            foreach (var track in replacement.Tracks)
            {
                track.Station = replacement;
                track.StationId = replacement.Id;
            }

            // Repoint the live references that hold the location instance directly.
            foreach (var stretch in layout.TrackStretches)
            {
                if (stretch.Start.Equals(existing)) { stretch.Start = replacement; stretch.StartId = replacement.Id; }
                if (stretch.End.Equals(existing)) { stretch.End = replacement; stretch.EndId = replacement.Id; }
            }
            foreach (var signal in layout.OperationLocations.OfType<SignalControlledLocation>())
            {
                if (signal.ControlledBy is { } controller && controller.Equals(existing))
                    signal.ControlledBy = replacement as Station; // null when the replacement is no longer a station
            }
            // Only a station can hold a lock key, so converting one is always converting it into
            // something that cannot: the locations whose keys it held are left without one rather than
            // pointing at a station the layout no longer has.
            foreach (var keyed in layout.OperationLocations.Where(o => o.LockKey is { } key && key.HeldAt.Equals(existing)))
                keyed.LockKey = null;

            layout.OperationLocations = [.. layout.OperationLocations.Select(o => o.Equals(existing) ? replacement : o)];
            return replacement;
        }
    }

    private static bool IsOfKind(OperationLocation location, OperationLocationKind kind) => kind switch
    {
        OperationLocationKind.Station => location is Station,
        OperationLocationKind.SignalControlled => location is SignalControlledLocation,
        OperationLocationKind.IndustrialArea => location is IndustrialArea,
        _ => location is OtherLocation,
    };
}

