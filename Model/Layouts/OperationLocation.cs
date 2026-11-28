using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Provides extension methods for <see cref="OperationLocation"/>.
/// </summary>
public static class OperationLocationExtensions
{
    extension(OperationLocation location)
    {
        /// <summary>
        /// The <see cref="Country.Id"/> of the country the location is operated as part of, or <c>null</c>: the
        /// planner's catalogue id of <see cref="OperationLocation.CountryCode"/>, which is what is stored.
        /// </summary>
        public int? CountryId
        {
            get => location.CountryCode is { Length: > 0 } code ? Country.ByCountryCode(code)?.Id : null;
            set => location.CountryCode = value is int id ? Country.ById(id)?.CountryCode : null;
        }
    }

    extension(OperationLocation? operationLocation)
    {
        /// <summary>
        /// Gets an example operation location for testing purposes.
        /// </summary>
        public static Station Example => new(1, "Ytterby", "Yb");

        /// <summary>
        /// Gets all trains that call at this station.
        /// </summary>
        /// <returns>A collection of distinct trains calling at this station.</returns>
        public IEnumerable<Train> Trains() =>
            operationLocation is null ? [] :
            operationLocation.Calls()
                .Where(c => c.Train.HasValue)
                .Select(c => c.Train!)
                .Distinct();

        /// <summary>
        /// Gets all station calls at this operation location.
        /// </summary>
        /// <returns>A collection of all station calls at this location.</returns>
        public IEnumerable<StationCall> Calls() =>
            operationLocation is null ? [] :
            operationLocation.Tracks.SelectMany(t => t.Calls);

        /// <summary>
        /// The location's tracks in the order they are shown: the order the planner put them in, and
        /// within that by track number, so tracks that were never given an order of their own still come
        /// out the same way everywhere. The first of them is the one a call moved here is put on.
        /// </summary>
        /// <remarks>
        /// The single definition of that order. The tracks table and the track drop-down both read it,
        /// so a planner reordering the tracks sees the drop-down follow.
        /// </remarks>
        public IReadOnlyList<StationTrack> TracksInDisplayOrder =>
            operationLocation is null ? [] :
            [.. operationLocation.Tracks
                .OrderBy(track => track.DisplayOrder)
                .ThenBy(track => track.Number, StringComparer.OrdinalIgnoreCase)];

        /// <summary>
        /// Finds a track by its number.
        /// </summary>
        /// <param name="number">The track number to find.</param>
        /// <returns>A <see cref="Maybe{T}"/> containing the track if found.</returns>
        public Maybe<StationTrack> Track(string number) =>
            new(operationLocation?.Tracks.SingleOrDefault(t => t.Number == number),
                string.Format(CultureInfo.CurrentCulture, Strings.StationHasNotTrackNumber, operationLocation?.Name, number));

        /// <summary>
        /// Determines whether the station has a track with the specified number.
        /// </summary>
        /// <param name="number">The track number to look for.</param>
        /// <returns><c>true</c> if a track with the number exists; otherwise, <c>false</c>.</returns>
        public bool HasTrack(string number)
            => operationLocation?.Tracks.Any(t => t.Number == number) ?? false;

        /// <summary>
        /// Adds a track to the station.
        /// </summary>
        /// <param name="stationTrack">The track to add.</param>
        /// <returns>The added track.</returns>
        public StationTrack Add(StationTrack stationTrack)
        {
            stationTrack = stationTrack.ValueOrException(nameof(stationTrack));
            ArgumentNullException.ThrowIfNull(stationTrack);
            operationLocation = operationLocation.ValueOrException(nameof(operationLocation));
            stationTrack.Station = operationLocation;
            stationTrack.StationId = operationLocation.Id;
            operationLocation.Tracks.Add(stationTrack);
            return stationTrack;
        }

        /// <summary>
        /// The track a train is put on here, given where it comes from and where it goes on to — either
        /// of them <c>null</c> where the train starts or ends its run at this location — and what it
        /// wants of the track it stands on. Returns <c>null</c> only where the location has no track.
        /// </summary>
        /// <remarks>
        /// The tracks are ranked best first: the scheduled ones; then those whose own route fits the
        /// train's most closely (see <c>StationTrackRoute.Match</c>); then those that give the train what
        /// it asked for — a platform where it stops to exchange passengers, the main track where it does
        /// not — with the main track deciding between equals; and last by display order, so the choice is
        /// the one the planner sees first. Where every track is reserved for some other way through, the
        /// routes are set aside and the rest of the ranking decides: the train has to stand somewhere.
        /// </remarks>
        /// <param name="timetable">The timetable that keeps the tracks' routes.</param>
        /// <param name="previous">The location the train comes from, or <c>null</c> where it starts here.</param>
        /// <param name="next">The location the train goes on to, or <c>null</c> where it ends here.</param>
        /// <param name="preference">What the train wants of the track it is put on.</param>
        public StationTrack? PreferredTrack(Timetable timetable, OperationLocation? previous, OperationLocation? next, TrackPreference preference)
        {
            if (operationLocation is null || operationLocation.Tracks.Count == 0) return null;

            var candidates = operationLocation.Tracks
                .Select(track => (track, fit: timetable.RouteOf(track).Match(previous?.Id, next?.Id)))
                .Where(candidate => candidate.fit.HasValue)
                .ToList();
            if (candidates.Count == 0)
                candidates = [.. operationLocation.Tracks.Select(track => (track, fit: (int?)0))];

            return candidates
                .OrderByDescending(candidate => candidate.track.IsScheduled)
                .ThenByDescending(candidate => candidate.fit!.Value)
                .ThenByDescending(candidate => Wanted(candidate.track))
                .ThenByDescending(candidate => candidate.track.IsMain)
                .ThenBy(candidate => candidate.track.DisplayOrder)
                .ThenBy(candidate => candidate.track.Number, StringComparer.OrdinalIgnoreCase)
                .First().track;

            bool Wanted(StationTrack track) =>
                preference is TrackPreference.Platform ? track.HasPassengerExchange : track.IsMain;
        }

        /// <summary>
        /// Decides whether trains can meet on an <see cref="Station"/>
        /// </summary>
        public bool CanHaveTrainsMeets =>
            operationLocation is Station station &&
            station.Tracks.Count(t => t.IsScheduled) > 1;
    }

    extension(OperationLocation location)
    {
        /// <summary>
        /// Whether <see cref="OperationLocation.Instructions"/> apply here: something has to be
        /// exchanged at the location — passengers, cargo or both — for there to be anything to say
        /// about how it is worked. Somewhere trains only ever run past, such as a block post, is
        /// worked from the timetable alone and is given no instructions. Neither is an
        /// <see cref="OtherLocation"/>: nobody works one, so a train there does what its call says
        /// and nothing more, whatever it exchanges.
        /// </summary>
        public bool HasInstructions =>
            location is not OtherLocation &&
            (location.HasPassengerExchange || location.HasCargoExchange);

        /// <summary>
        /// Whether a locomotive can be turned here, which takes a station with a
        /// <see cref="Station.HasTurntable">turntable</see>. Nowhere else has one: turning is a
        /// station facility, not something a train does on the line.
        /// </summary>
        public bool CanTurnLoco => location is Station { HasTurntable: true };

        /// <summary>
        /// Whether <see cref="Settings.StationTimings.LocoRunaroundRealMinutes"/> applies here: only a
        /// <see cref="Station"/>, shadow yards included, has the tracks for a locomotive to run around
        /// its train.
        /// </summary>
        public bool HasLocoRunaroundTime => location is Station;

        /// <summary>
        /// Whether <see cref="Settings.StationTimings.TrainClearanceRealMinutes"/> applies here: clearing
        /// a train takes a dispatcher on duty, so only a manned <see cref="Station"/> has a clearance time.
        /// </summary>
        public bool HasTrainClearanceTime => location is Station { IsManned: true };

        /// <summary>
        /// Makes sure this location has somewhere to exchange the passengers it says it exchanges: where
        /// it exchanges passengers and not one of its tracks has a platform, all of them are given
        /// <see cref="StationTrack.DefaultPlatformLength"/>. Returns <c>true</c> when something changed.
        /// Idempotent. See <c>Layout.EnsurePlatforms</c> for why, and for the whole-layout form of it.
        /// </summary>
        public bool EnsurePlatforms()
        {
            if (!location.HasPassengerExchange) return false;
            if (location.Tracks.Count == 0 || location.Tracks.Any(track => track.HasPlatform)) return false;
            foreach (var track in location.Tracks) track.PlatformLength = StationTrack.DefaultPlatformLength;
            return true;
        }
    }
}
