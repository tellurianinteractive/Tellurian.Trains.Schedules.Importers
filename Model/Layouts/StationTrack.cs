using System.Runtime.CompilerServices;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Provides extension methods for <see cref="StationTrack"/>.
/// </summary>
public static class StationTrackExtensions
{
    // The calls on each track: an index into Train.Calls, rebuilt from the trains whenever a timetable is read
    // (TimetableExtensions.RebuildStationCalls). The track is the shared model's, which knows nothing of calls.
    private static readonly ConditionalWeakTable<StationTrack, List<StationCall>> CallIndex = new();

    extension(StationTrack)
    {
        /// <summary>An example station track for testing purposes.</summary>
        public static StationTrack Example => new(1, "1") { Station = OperationLocation.Example };
    }

    extension(StationTrack track)
    {
        /// <summary>
        /// The station calls on this track: derived, never a source. A call lives in its train (<see cref="Train.Calls"/>,
        /// the aggregate); this is an index of them by track, kept outside the track, which is a type of the shared
        /// model and knows nothing of trains. It is rebuilt from the trains when a timetable is read
        /// (<c>RebuildStationCalls</c>) and kept in step where a call joins a train (<c>Train.Add</c>), changes track
        /// (<c>StationCall.MoveTo</c>) or is deleted. Never written to a file, and never changed to change a call.
        /// </summary>
        public ICollection<StationCall> Calls => CallIndex.GetOrCreateValue(track);

        /// <summary>The planner's name of <see cref="StationTrack.Designation"/>: what the track is called, e.g. "1".</summary>
        public string Number
        {
            get => track.Designation;
            set => track.Designation = value;
        }

        /// <summary>
        /// Whether passengers can be exchanged at this track: the location exchanges passengers at all
        /// (see <see cref="OperationLocation.HasPassengerExchange"/>) and this track has a platform for
        /// them to get on and off at.
        /// </summary>
        /// <remarks>
        /// This is a property of the track, not of the train standing on it. A passenger train may
        /// perfectly well stand at a track where this is false — meeting another train at a location
        /// that exchanges no passengers, for one — it simply exchanges nothing while it is there.
        /// </remarks>
        public bool HasPassengerExchange =>
            track.Station is { HasPassengerExchange: true } && track.HasPlatform;

        /// <summary>
        /// Whether this track is meant for trains running a particular way through the location: it names
        /// where such a train comes from (<see cref="StationTrack.PreviousLocationId"/>), where it goes on
        /// to (<see cref="StationTrack.NextLocationId"/>), or both.
        /// </summary>
        public bool HasRoute =>
            track.PreviousLocationId.HasValue || track.NextLocationId.HasValue;

        /// <summary>
        /// How well this track's route fits a train that comes from <paramref name="previousLocationId"/>
        /// and goes on to <paramref name="nextLocationId"/> — either of them <c>null</c> where the train
        /// starts or ends its run here. The bigger the number the better the fit, and <c>null</c> says the
        /// train's route contradicts one of the track's named locations, so this is not its track at all.
        /// </summary>
        /// <remarks>
        /// A named location the train's route confirms is what makes a track the one to use, so it weighs
        /// more than anything else; a named location the train has none of — it starts or ends its run
        /// here — counts a little against the track, leaving it behind one that names only what the train
        /// does confirm. A track naming no location comes to zero: it fits every train, claiming and
        /// contradicting none.
        /// </remarks>
        /// <param name="previousLocationId">The <see cref="OperationLocation.Id"/> the train comes from, or <c>null</c> where it starts here.</param>
        /// <param name="nextLocationId">The <see cref="OperationLocation.Id"/> the train goes on to, or <c>null</c> where it ends here.</param>
        public int? RouteMatch(int? previousLocationId, int? nextLocationId)
        {
            var asNamed = Fit(previousLocationId, nextLocationId);
            if (!track.AppliesInBothDirections) return asNamed;
            var reversed = Fit(nextLocationId, previousLocationId);
            return asNamed is null ? reversed :
                reversed is null ? asNamed :
                Math.Max(asNamed.Value, reversed.Value);

            // Both ends have to hold at once, so a fit is the two sides added together — and nothing at
            // all as soon as either side is contradicted.
            int? Fit(int? from, int? to) =>
                Side(track.PreviousLocationId, from) is { } fromSide && Side(track.NextLocationId, to) is { } toSide
                    ? fromSide + toSide
                    : null;

            // What one side of the track's route is worth against the train's.
            static int? Side(int? named, int? actual) =>
                named is null ? 0 :     // the track says nothing about this side: neither for nor against
                actual is null ? -1 :   // the train has no location here, starting or ending its run
                named == actual ? 2 :   // the train's route confirms the track's
                null;                   // the route contradicts it: not this train's track
        }
    }

    /// <summary>
    /// Adds a station call to the track.
    /// </summary>
    /// <param name="me">The track to add the call to.</param>
    /// <param name="call">The station call to add.</param>
    /// <returns>The added station call.</returns>
    internal static StationCall Add(this StationTrack me, StationCall call)
    {
        ArgumentNullException.ThrowIfNull(call);
        if (!me.Calls.Contains(call))
        {
            me.Calls.Add(call);
        }
        return call;
    }
}
