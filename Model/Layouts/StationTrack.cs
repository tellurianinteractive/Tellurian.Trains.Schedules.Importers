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
