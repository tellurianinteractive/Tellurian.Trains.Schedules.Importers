using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

public static class GraphScheduleDrawingExtensions
{
    public static string OrientationCss(this TimeAxisDirection axisDirection, string classes) =>
        axisDirection == TimeAxisDirection.Horisontal ? $"{classes} horizontal".TrimStart() :
        axisDirection == TimeAxisDirection.Vertical ? $"{classes} vertical".TrimStart() :
        string.Empty;

    public static IEnumerable<(Offset Start, Offset End)> TrainBetweenStationsLine(this GraphSchedule me, int departureStationIndex, int departureTrackIndex, int arrivalStationIndex, int arrivalTrackIndex, Time departure, Time arrival)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var yd = me.Y(departureStationIndex, departureTrackIndex);
            var ya = me.Y(arrivalStationIndex, arrivalTrackIndex);
            foreach (var piece in me.ClipToWindow(WrapPieces(departure.Value, yd, arrival.Value, ya)))
                yield return (new(me.TimeOffset(piece.FromTime).X, piece.FromCross), new(me.TimeOffset(piece.ToTime).X, piece.ToCross));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var xd = me.X(departureStationIndex, departureTrackIndex);
            var xa = me.X(arrivalStationIndex, arrivalTrackIndex);
            foreach (var piece in me.ClipToWindow(WrapPieces(departure.Value, xd, arrival.Value, xa)))
                yield return (new(piece.FromCross, me.TimeOffset(piece.FromTime).Y), new(piece.ToCross, me.TimeOffset(piece.ToTime).Y));
        }
        else throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static IEnumerable<(Offset Start, Offset End)> TrainAtStationLine(this GraphSchedule me, int stationIndex, int trackIndex, StationCall stationCall)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var y = me.Y(stationIndex, trackIndex);
            foreach (var piece in me.ClipToWindow(WrapPieces(stationCall.Arrival.Value, y, stationCall.Departure.Value, y)))
                yield return (new(me.TimeOffset(piece.FromTime).X, piece.FromCross), new(me.TimeOffset(piece.ToTime).X, piece.ToCross));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var x = me.X(stationIndex, trackIndex);
            foreach (var piece in me.ClipToWindow(WrapPieces(stationCall.Arrival.Value, x, stationCall.Departure.Value, x)))
                yield return (new(piece.FromCross, me.TimeOffset(piece.FromTime).Y), new(piece.ToCross, me.TimeOffset(piece.ToTime).Y));
        }
        else throw new NotSupportedException(me.AxisDirection.ToString());
    }

    private static readonly TimeSpan OneDay = TimeSpan.FromHours(24);

    /// <summary>Reduces a time into the current day ([0, 24:00)), so a label for an after-midnight call
    /// (e.g. 24:05) is positioned at its wrapped place on the axis (00:05). A no-op for normal times.</summary>
    private static TimeSpan WrapTime(TimeSpan time) =>
        TimeSpan.FromMinutes(((time.TotalMinutes % OneDay.TotalMinutes) + OneDay.TotalMinutes) % OneDay.TotalMinutes);

    /// <summary>
    /// Splits a train line running from <paramref name="fromTime"/>/<paramref name="fromCross"/> to
    /// <paramref name="toTime"/>/<paramref name="toCross"/> at every 24-hour boundary, so the part at or
    /// past 24:00 wraps back to the start of the axis. Each returned piece has both times reduced into
    /// the current day ([0, 24:00]); the cross-axis position (station/track) is interpolated at the
    /// boundary so the line leaves the right edge and re-enters the left edge at the same position.
    /// When nothing crosses 24:00 (the normal case), a single unchanged piece is returned.
    /// </summary>
    private static IEnumerable<(TimeSpan FromTime, int FromCross, TimeSpan ToTime, int ToCross)> WrapPieces(TimeSpan fromTime, int fromCross, TimeSpan toTime, int toCross)
    {
        var totalMinutes = (toTime - fromTime).TotalMinutes;
        var cursorTime = fromTime;
        var cursorCross = (double)fromCross;
        while (true)
        {
            var dayIndex = (int)Math.Floor((cursorTime.TotalMinutes / OneDay.TotalMinutes) + 1e-6);
            var boundary = OneDay * (dayIndex + 1);
            var shift = OneDay * dayIndex;
            if (totalMinutes <= 0 || toTime <= boundary)
            {
                yield return (cursorTime - shift, (int)Math.Round(cursorCross), toTime - shift, toCross);
                yield break;
            }
            var boundaryCross = fromCross + ((toCross - fromCross) * ((boundary - fromTime).TotalMinutes / totalMinutes));
            yield return (cursorTime - shift, (int)Math.Round(cursorCross), boundary - shift, (int)Math.Round(boundaryCross));
            cursorTime = boundary;
            cursorCross = boundaryCross;
        }
    }

    /// <summary>
    /// Clips each piece to the visible time window [<see cref="GraphSchedule.StartTime"/>,
    /// <see cref="GraphSchedule.EndTime"/>]. A piece reaching outside the window is shortened to the
    /// window edge with its cross-axis position (station/track) interpolated at that edge; a piece
    /// wholly outside is dropped. Without this, an out-of-window time yields <see cref="Offset.Invalid"/>
    /// (0,0) from <see cref="TimeOffset"/>, drawing a stray line to the axis origin. Pieces already
    /// inside the window pass through unchanged. Each piece runs forward in time (FromTime ≤ ToTime).
    /// </summary>
    private static IEnumerable<(TimeSpan FromTime, int FromCross, TimeSpan ToTime, int ToCross)> ClipToWindow(
        this GraphSchedule me, IEnumerable<(TimeSpan FromTime, int FromCross, TimeSpan ToTime, int ToCross)> pieces)
    {
        var start = me.StartTime;
        var end = me.EndTime;
        foreach (var piece in pieces)
        {
            if (piece.ToTime < start || piece.FromTime > end) continue; // wholly outside the window

            var span = (piece.ToTime - piece.FromTime).TotalMinutes;
            int CrossAt(TimeSpan time) => span <= 0
                ? piece.FromCross
                : (int)Math.Round(piece.FromCross + ((piece.ToCross - piece.FromCross) * ((time - piece.FromTime).TotalMinutes / span)));

            var fromTime = piece.FromTime < start ? start : piece.FromTime;
            var toTime = piece.ToTime > end ? end : piece.ToTime;
            yield return (fromTime, CrossAt(fromTime), toTime, CrossAt(toTime));
        }
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

    public static string MinutesText(this Time time) => time.Value.Minutes.ToString("D2");

    private static Offset MinuteUnder(this GraphSchedule me, int stationIndex, int trackIndex, TimeSpan time)
    {
        time = WrapTime(time);
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
        time = WrapTime(time);
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

    /// <summary>Whether the given call time falls inside the visible time window, so its minute label
    /// has a valid position. After-midnight times are wrapped into the current day first, matching how
    /// they are positioned on the axis.</summary>
    public static bool IsTimeVisible(this GraphSchedule me, TimeSpan time)
    {
        var wrapped = WrapTime(time);
        return wrapped >= me.StartTime && wrapped <= me.EndTime;
    }

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
                var Δ1 = Math.Max(me.GraphSettings.MinStationSpacing, ((fromTrackCount - 1) * me.GraphSettings.TrackSpacing) + (int)Math.Round(me.GraphSettings.KilometerSpacing * stretch.Distance));
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

    // The distance-axis extent is the position of the last station's last track. This must use the same
    // accumulation as TrackOffset (which positions every drawn track): the hour-display margin
    // (TimeAxisSpacing) plus the per-stretch spacing, plus the last station's track fan-out added
    // *outside* the MinStationSpacing floor. Re-deriving it with a different formula under-reserved the
    // fan-out when KilometerSpacing was low, clipping the last station. Height()/Width() add EndMargin.
    public static Offset MaxTrackOffset(this GraphSchedule me)
    {
        var lastStationIndex = me.Stations.Length - 1;
        var lastTrackIndex = me.Stations[lastStationIndex].Tracks.Count - 1;
        return me.TrackOffset(lastStationIndex, lastTrackIndex);
    }
}
