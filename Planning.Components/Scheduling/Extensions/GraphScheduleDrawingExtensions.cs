using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

public static class GraphScheduleDrawingExtensions
{
    public static string OrientationCss(this TimeAxisDirection axisDirection, string classes) =>
        axisDirection == TimeAxisDirection.Horisontal ? $"{classes} horizontal".TrimStart() :
        axisDirection == TimeAxisDirection.Vertical ? $"{classes} vertical".TrimStart() :
        string.Empty;

    public static (Offset Start, Offset End) TrainBetweenStationsLine(this GraphSchedule me, int departureStationIndex, int departureTrackIndex, int arrivalStationIndex, int arrivalTrackIndex, Time departure, Time arrival)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var yd = me.Y(departureStationIndex, departureTrackIndex);
            var ya = me.Y(arrivalStationIndex, arrivalTrackIndex);
            return (new(me.TimeOffset(departure.Value).X, yd), new(me.TimeOffset(arrival.Value).X, ya));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var xd = me.X(departureStationIndex, departureTrackIndex);
            var xa = me.X(arrivalStationIndex, arrivalTrackIndex);
            return (new(xd, me.TimeOffset(departure.Value).Y), new(xa, me.TimeOffset(arrival.Value).Y));
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static (Offset Start, Offset End) TrainAtStationLine(this GraphSchedule me, int stationIndex, int trackIndex, StationCall stationCall)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var y = me.Y(stationIndex, trackIndex);
            return (new(me.TimeOffset(stationCall.Arrival.Value).X, y), new(me.TimeOffset(stationCall.Departure.Value).X, y));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var x = me.X(stationIndex, trackIndex);
            return (new(x, me.TimeOffset(stationCall.Arrival.Value).Y), new(x, me.TimeOffset(stationCall.Departure.Value).Y));
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static Offset ArrivalMinuteOver(this GraphSchedule me, int stationIndex, StationCall stationCall)
    {
        var offset = me.MinuteOver(stationIndex, stationCall.Arrival.Value);
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new Offset(offset.X - 13, offset.Y - 2),
            TimeAxisDirection.Vertical => new Offset(offset.X - 10, offset.Y - 6),
            _ => throw new NotSupportedException(me.AxisDirection.ToString())
        };
    }

    public static Offset ArrivalMinuteUnder(this GraphSchedule me, int stationIndex, int trackIndex, StationCall stationCall)
    {
        var offset = me.MinuteUnder(stationIndex, trackIndex, stationCall.Arrival.Value);
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new Offset(offset.X - 16, offset.Y + 7),
            TimeAxisDirection.Vertical => new Offset(offset.X + 2, offset.Y - 7),
            _ => throw new NotSupportedException(me.AxisDirection.ToString())
        };
    }

    public static Offset DepartureMinuteOver(this GraphSchedule me, int stationIndex, StationCall stationCall)
    {
        var offset = me.MinuteOver(stationIndex, stationCall.Departure.Value);
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new Offset(offset.X + 4, offset.Y - 2),
            TimeAxisDirection.Vertical => new Offset(offset.X - 10, offset.Y + 10),
            _ => throw new NotSupportedException(me.AxisDirection.ToString())
        };
    }

    public static Offset DepartureMinuteUnder(this GraphSchedule me, int stationIndex, int trackIndex, StationCall stationCall)
    {
        var offset = me.MinuteUnder(stationIndex, trackIndex, stationCall.Departure.Value);
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new Offset(offset.X + 4, offset.Y + 8),
            TimeAxisDirection.Vertical => new Offset(offset.X + 1, offset.Y + 11),
            _ => throw new NotSupportedException(me.AxisDirection.ToString())
        };
    }

    public static int Minutes(this Time time) => time.Value.Minutes;

    private static Offset MinuteUnder(this GraphSchedule me, int stationIndex, int trackIndex, TimeSpan time)
    {
        var station = me.Stations[stationIndex];
        var lastTrackIndex = station.Tracks.Count - 1;
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var x = me.TimeOffset(time).X + (lastTrackIndex - trackIndex);
            var y = me.Y(stationIndex, lastTrackIndex);
            return new Offset(x, y);
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var x = me.X(stationIndex, lastTrackIndex);
            var y = me.TimeOffset(time).Y + (lastTrackIndex - trackIndex);
            return new Offset(x, y);
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    private static Offset MinuteOver(this GraphSchedule me, int stationIndex, TimeSpan time)
    {
        var trackIndex = 0;
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var x = me.TimeOffset(time).X;
            var y = me.Y(stationIndex, trackIndex);
            return new Offset(x, y);
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var x = me.X(stationIndex, trackIndex);
            var y = me.TimeOffset(time).Y;
            return new Offset(x, y);
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static (Offset Start, Offset End) TrackLine(this GraphSchedule me, int stationIndex, int trackIndex)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var y = me.Y(stationIndex, trackIndex);
            return (new(me.GraphSettings.KilometerAxisSpacing.X, y), new(me.MaxTimeOffset().X, y));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var x = me.X(stationIndex, trackIndex);
            return (new(x, me.GraphSettings.KilometerAxisSpacing.Y), new(x, me.MaxTimeOffset().Y));
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static (Offset Start, Offset End) TimeLine(this GraphSchedule me, TimeSpan time)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var x = me.TimeOffset(time).X;
            return (new(x, me.GraphSettings.TimeAxisSpacing.Y), new(x, me.MaxTrackOffset().Y));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var y = me.TimeOffset(time).Y;
            return (new(me.GraphSettings.TimeAxisSpacing.X, y), new(me.MaxTrackOffset().X, y));
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static int Height(this GraphSchedule me) =>
        me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => me.MaxTrackOffset().Y + me.GraphSettings.EndMargin,
            TimeAxisDirection.Vertical => me.MaxTimeOffset().Y + me.GraphSettings.EndMargin,
            _ => 0
        };

    public static int Width(this GraphSchedule me) =>
        me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => me.MaxTimeOffset().X + me.GraphSettings.EndMargin,
            TimeAxisDirection.Vertical => me.MaxTrackOffset().X + me.GraphSettings.EndMargin,
            _ => 0
        };

    public static Offset TimeAxisLabelOffset(this GraphSchedule me, TimeSpan time) =>
       me.AxisDirection switch
       {
           TimeAxisDirection.Horisontal => me.TimeLine(time).Start - new Offset(5, 5),
           TimeAxisDirection.Vertical => me.TimeLine(time).Start - new Offset(me.GraphSettings.TimeAxisSpacing.X - 5, 0),
           _ => throw new NotSupportedException()
       };

    public static Offset StationLabelOffset(this GraphSchedule me, int stationIndex)
    {
        var offset = me.Stations[stationIndex].Tracks.Count / 2 * me.GraphSettings.TrackSpacing;
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new(5, me.Y(stationIndex, 0) + offset),
            TimeAxisDirection.Vertical => new(me.X(stationIndex, 0) + offset, 25),
            _ => Offset.Invalid
        };
    }

    public static Offset KmLabelOffset(this GraphSchedule me, int stationIndex)
    {
        var offset = me.Stations[stationIndex].Tracks.Count / 2 * me.GraphSettings.TrackSpacing;
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new(me.GraphSettings.KilometerAxisSpacing.X - 15, me.Y(stationIndex, 0) + offset),
            TimeAxisDirection.Vertical => new(me.X(stationIndex, 0) + offset, me.GraphSettings.KilometerAxisSpacing.Y - 15),
            _ => Offset.Invalid
        };
    }

    public static Offset TrackNumberOffset(this GraphSchedule me, int stationIndex, int trackIndex)
    {
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new(me.GraphSettings.KilometerAxisSpacing.X - 2, me.Y(stationIndex, trackIndex) + 3),
            TimeAxisDirection.Vertical => new(me.X(stationIndex, trackIndex) + 0, me.GraphSettings.KilometerAxisSpacing.Y - 2),
            _ => Offset.Invalid
        };
    }

    public static Offset TimeOffset(this GraphSchedule me, TimeSpan time)
    {
        if (time < me.StartTime || time > me.EndTime) return Offset.Invalid;
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var x = me.GraphSettings.KilometerAxisSpacing.X + (me.GraphSettings.MinuteSpacing * (int)(time - me.StartTime).TotalMinutes);
            return new(x, 0);
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var y = me.GraphSettings.KilometerAxisSpacing.Y + (me.GraphSettings.MinuteSpacing * (int)(time - me.StartTime).TotalMinutes);
            return new(0, y);
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static Offset MaxTimeOffset(this GraphSchedule me) =>
        TimeOffset(me, me.EndTime);

    public static int X(this GraphSchedule me, int stationIndex, int trackIndex) =>
        me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => TrackOffset(me, stationIndex, trackIndex).Y,
            TimeAxisDirection.Vertical => TrackOffset(me, stationIndex, trackIndex).X,
            _ => 0
        };

    public static int Y(this GraphSchedule me, int stationIndex, int trackIndex) =>
         me.AxisDirection switch
         {
             TimeAxisDirection.Horisontal => TrackOffset(me, stationIndex, trackIndex).Y,
             TimeAxisDirection.Vertical => TrackOffset(me, stationIndex, trackIndex).X,
             _ => 0
         };

    public static Offset TrackOffset(this GraphSchedule me, int stationIndex, int trackIndex)
    {
        var tracks = me.Stations[stationIndex].Tracks.ToArray();
        var x = me.GraphSettings.TimeAxisSpacing.X;
        var y = me.GraphSettings.TimeAxisSpacing.Y;
        if (stationIndex == 0)
        {
            y += me.GraphSettings.TrackSpacing * trackIndex;
            x += me.GraphSettings.TrackSpacing * trackIndex;
        }
        else
        {
            for (var i = 0; i < stationIndex; i++)
            {
                var stretch = me.TrackStretches[i];
                var fromTrackCount = me.Stations[i].Tracks.Count;
                var Δ1 = Math.Max(me.GraphSettings.MinStationSpacing, ((fromTrackCount - 1) * me.GraphSettings.TrackSpacing) + (me.GraphSettings.KilometerSpacing * (int)Math.Round(stretch.Distance)));
                x += Δ1;
                y += Δ1;
            }
            var Δ2 = trackIndex * me.GraphSettings.TrackSpacing;
            x += Δ2;
            y += Δ2;
        }
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new(0, y),
            TimeAxisDirection.Vertical => new(x, 0),
            _ => Offset.Invalid
        };
    }

    public static Offset MaxTrackOffset(this GraphSchedule me)
    {
        var firstTrackCount = me.Stations[0].Tracks.Count;
        var x = me.GraphSettings.TimeAxisSpacing.X + ((firstTrackCount - 1) * me.GraphSettings.TrackSpacing);
        var y = me.GraphSettings.TimeAxisSpacing.Y + ((firstTrackCount - 1) * me.GraphSettings.TrackSpacing);
        for (var i = 0; i < me.TrackStretches.Length; i++)
        {
            var stretch = me.TrackStretches[i];
            var toTrackCount = me.Stations[i + 1].Tracks.Count;
            var Δ = Math.Max(me.GraphSettings.MinStationSpacing, (me.GraphSettings.KilometerSpacing * (int)Math.Round(stretch.Distance)) + ((toTrackCount - 1) * me.GraphSettings.TrackSpacing));
            x += Δ;
            y += Δ;
        }

        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new(0, y),
            TimeAxisDirection.Vertical => new(x, 0),
            _ => Offset.Invalid
        };
    }
}
