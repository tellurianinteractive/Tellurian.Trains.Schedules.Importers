using System.Globalization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="Layout"/> class to support XPLN import operations.
    /// </summary>
    public static class LayoutExtenstions
    {
        extension(Layout me)
        {
            /// <summary>
            /// Finds a station track by station signature and track number.
            /// </summary>
            /// <param name="stationSignature">The signature (short code) of the station.</param>
            /// <param name="trackNumber">The track number within the station.</param>
            /// <returns>
            /// A <see cref="Maybe{StationTrack}"/> containing the track if found,
            /// or a failure result with an error message if the station or track does not exist.
            /// </returns>
            public Maybe<StationTrack> Track(string stationSignature, string trackNumber)
            {
                var station = me.Station(stationSignature);
                if (station.IsNone)
                    return Maybe<StationTrack>.NoneWithReason(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ThereIsNoStation, stationSignature));
                var track = station.Value.Tracks.SingleOrDefault(t => t.Number.Equals(trackNumber, StringComparison.OrdinalIgnoreCase));
                if (track is null)
                    return Maybe<StationTrack>.NoneWithReason(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TheTrackIsItNotInStation, trackNumber, stationSignature));
                return new Maybe<StationTrack>(track);
            }
        }
    }
}
