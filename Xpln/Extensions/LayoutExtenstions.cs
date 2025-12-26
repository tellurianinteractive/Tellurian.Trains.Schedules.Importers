using System.Globalization;
using Tellurian.Trains.Schedules.Importers.Model;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Extensions
{
    public static class LayoutExtenstions
    {
        extension(Layout me)
        {
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
