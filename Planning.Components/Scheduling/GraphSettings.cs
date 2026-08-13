namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

public record GraphSettings
{
    public static GraphSettings Default => new()
    {
        AxisDirection = TimeAxisDirection.Horisontal,
        DefaultStartTime = TimeSpan.FromHours(6),
        DefaultEndTime = TimeSpan.FromHours(20),
        TimeAxisSpacing = new(30, 30),
        KilometerAxisSpacing = new(100, 60),
        EndMargin = 20,
        TrackSpacing = 8,
        MinStationSpacing = 100,
        MinuteSpacing = 2,
        KilometerSpacing = 3,
        ShowArrivalMinutes = false,
        ShowDepartureMinutes = true,
        TrainLabelFontPoints = 8,
    };

    public TimeAxisDirection AxisDirection { get; set; }
    public TimeSpan DefaultStartTime { get; set; }
    public TimeSpan DefaultEndTime { get; set; }

    /// <summary>Optional fast-clock break that splits the time axis into a first half
    /// (<see cref="DefaultStartTime"/>–<c>BreakTime</c>) and a last half (<c>BreakTime</c>–<see cref="DefaultEndTime"/>).
    /// <c>null</c> means no break, so only the whole graph can be shown. See <see cref="GraphHalf"/>.</summary>
    public TimeSpan? BreakTime { get; set; }
    public Offset TimeAxisSpacing { get; set; }
    public Offset KilometerAxisSpacing { get; set; }
    public int EndMargin { get; set; }
    public int TrackSpacing { get; set; }
    public int MinStationSpacing { get; set; }
    public int MinuteSpacing { get; set; }
    public int KilometerSpacing { get; set; }
    public bool ShowArrivalMinutes { get; set; }
    public bool ShowDepartureMinutes { get; set; }
    public bool ShowCompany { get; set; }
    public bool ShowTrainCategory { get; set; }

    /// <summary>Whether the train's operating sessions/days badge is suppressed on its graph label.
    /// Default is <c>false</c> (the badge is shown for trains that do not run every session/day).</summary>
    public bool HideSessionsOrDays { get; set; }

    /// <summary>Whether the operating pattern is presented as weekdays (e.g. <c>Mo-Fr</c>) rather than
    /// session numbers (e.g. <c>1-5</c>). Mirrors the layout's <c>General.UseDays</c> setting.</summary>
    public bool UseDays { get; set; }

    /// <summary>The number of operating sessions/days in the layout's period (1–14); higher session bits are
    /// ignored when formatting the label. Mirrors the layout's <c>General.MaxSessions</c> setting.</summary>
    public int MaxSessions { get; set; } = 14;

    /// <summary>The first weekday of the operating week, used to pick which weekdays fall within the period
    /// when the label shows days. Mirrors the layout's <c>General.StartDay</c> setting.</summary>
    public DayOfWeek StartDay { get; set; } = DayOfWeek.Monday;

    /// <summary>The number of loco drivers expected to be available, which the demand bars are coloured
    /// against. <c>0</c> means no expectation is set and the bars are not shown. Mirrors the layout's
    /// <c>General.ExpectedLocoDrivers</c> setting.</summary>
    public int ExpectedLocoDrivers { get; set; }

    /// <summary>Font size in points of the train identity labels along the train lines. Drives both the
    /// rendered text and the label-thinning overlap estimate, so changing it re-thins on the next render.</summary>
    public int TrainLabelFontPoints { get; set; }

    /// <summary>
    /// How many drawing units one typographic point occupies. On screen a drawing unit is a CSS pixel, so this
    /// is 96/72. A printed graph draws in hundredths of a millimetre instead (see <c>GraphPrintScale</c>), and
    /// sets this to the matching value so the label-thinning overlap estimate stays in the same units as the
    /// geometry — otherwise thinning would judge printed labels by screen sizes and drop far too few.
    /// </summary>
    public double UnitsPerPoint { get; set; } = 96.0 / 72.0;

    /// <summary>Perpendicular lift of a train identity label off its own line, in drawing units. The renderer
    /// applies it as the <c>textPath</c> <c>dy</c>, and the thinning estimate offsets the label box by the
    /// same amount.</summary>
    public int LabelLift { get; set; } = 2;

    /// <summary>Stroke width of a train line, in drawing units.</summary>
    public int TrainStrokeWidth { get; set; } = 2;

    /// <summary>Gap in drawing units between an axis label and the axis it labels, and the inset of the station
    /// signature from the edge of the sheet.</summary>
    public int LabelGap { get; set; } = 5;

    /// <summary>Gap in drawing units between the kilometre label and the distance axis. It must clear the track
    /// numbers, which are set closer to the axis (<see cref="TrackNumberGap"/>).</summary>
    public int KmLabelGap { get; set; } = 15;

    /// <summary>Gap in drawing units between a track number and the distance axis.</summary>
    public int TrackNumberGap { get; set; } = 2;

    /// <summary>How far in drawing units a track number is dropped below its track line, so the text sits
    /// centred on the line rather than resting on it. Roughly half the track-number cap height.</summary>
    public int TrackNumberBaseline { get; set; } = 3;

    /// <summary>Distance in drawing units from the top of the drawing to the row of station signatures, used
    /// only with a vertical time axis where the signatures sit above their station columns.</summary>
    public int StationNameBaseline { get; set; } = 25;
}

public enum TimeAxisDirection
{
    Horisontal,
    Vertical
}

/// <summary>Which part of the time axis a graphical schedule renders. When a break time is set,
/// the axis can be limited to the first half (start–break) or the last half (break–end); useful
/// on smaller screens, especially with a vertical time axis. <see cref="Whole"/> shows the full axis.</summary>
public enum GraphHalf
{
    Whole,
    First,
    Last
}
