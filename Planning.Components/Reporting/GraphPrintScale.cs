using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// Turns the layout's print settings into the <see cref="GraphSettings"/> a graphical timetable is drawn with on
/// paper, at a true millimetre scale.
/// <para>
/// A printed graph draws in units of a hundredth of a millimetre rather than CSS pixels: the SVG is given its
/// real size in millimetres and a <c>viewBox</c> in these units, so 18&#160;mm per hour is exactly 30 units per
/// minute. Keeping the unit small enough that every spacing stays a whole number is what lets the drawing code —
/// integer geometry throughout, shared with the on-screen editor — print to an exact scale without being
/// reworked. It also means the graph is <em>re-laid out</em> at the print scale rather than uniformly shrunk:
/// text keeps its printed point size and the label thinning re-runs against the real printed density, instead of
/// shrinking every label into illegibility along with the geometry.
/// </para>
/// <para>
/// The type sizes below are mirrored by the <c>.print</c> rules in <c>GraphicalScheduleEditor.razor.css</c>, which
/// must express them in the same units. Change one and change the other: the gutters reserved here are sized from
/// these numbers, so a larger printed font than the gutter allows would run into the graph.
/// </para>
/// </summary>
public static class GraphPrintScale
{
    /// <summary>Drawing units per millimetre in a printed graph.</summary>
    public const int UnitsPerMillimetre = 100;

    /// <summary>One typographic point in printed drawing units (1&#160;pt = 1/72&#160;inch).</summary>
    public const double UnitsPerPoint = 25.4 / 72.0 * UnitsPerMillimetre;

    /// <summary>Printed size of the hour labels along the time axis, in millimetres (about 8.5&#160;pt).</summary>
    public const double HourFontMm = 3.0;

    /// <summary>Printed size of the station signatures, in millimetres (about 7.5&#160;pt).</summary>
    public const double StationNameFontMm = 2.6;

    /// <summary>Printed size of the kilometre labels, track numbers and arrival/departure minutes, in
    /// millimetres (about 5&#160;pt).</summary>
    public const double SmallFontMm = 1.8;

    // Average advance of a glyph as a fraction of the font size, the same estimate the label thinning uses.
    private const double GlyphWidthEm = 0.62;

    // Gaps and margins of the printed drawing, in millimetres.
    private const double LabelGapMm = 0.5;
    private const double KmLabelGapMm = 2.5;
    private const double TrackNumberGapMm = 0.3;
    private const double TrackNumberBaselineMm = 0.6;
    private const double StationNameBaselineMm = 2.5;
    private const double EndMarginMm = 2.0;
    private const double LabelLiftMm = 0.4;
    private const double TrainStrokeWidthMm = 0.25;

    /// <summary>Converts millimetres to printed drawing units.</summary>
    public static int Units(double millimetres) => (int)Math.Round(millimetres * UnitsPerMillimetre);

    /// <summary>Converts printed drawing units to millimetres.</summary>
    public static double Millimetres(double units) => units / UnitsPerMillimetre;

    /// <summary>Gutter across the time axis that the hour labels need.</summary>
    public static double TimeAxisGutterMm => HourFontMm + (2 * LabelGapMm);

    /// <summary>
    /// Gutter along the time axis that the distance axis needs with a horizontal time axis: the station
    /// signature and the kilometre label share this row, so it is sized from the longest signature printed.
    /// </summary>
    public static double DistanceAxisGutterMm(int longestSignatureLength) =>
        LabelGapMm
        + (Math.Max(1, longestSignatureLength) * StationNameFontMm * GlyphWidthEm)
        + LabelGapMm
        + (6 * SmallFontMm * GlyphWidthEm)   // "123 km"
        + KmLabelGapMm;

    /// <summary>
    /// Gutter along the time axis that the distance axis needs with a vertical time axis, where the signature
    /// and the kilometre label sit on two rows above their station column rather than side by side.
    /// </summary>
    public static double DistanceAxisGutterVerticalMm => StationNameBaselineMm + SmallFontMm + KmLabelGapMm;

    extension(LayoutSettings settings)
    {
        /// <summary>
        /// Builds the print-scaled settings for a graphical timetable. Every user preference (orientation, which
        /// minutes to show, what a train label carries) is inherited from the on-screen settings, so screen and
        /// paper show the same graph; only the geometry is re-scaled to millimetres.
        /// </summary>
        /// <param name="longestSignatureLength">Number of characters in the longest station signature printed,
        /// which sizes the distance-axis gutter. Taken across every stretch in the report so all sheets share
        /// one gutter, and therefore one number of minutes per sheet.</param>
        /// <param name="stationSpacingMm">Overrides the minimum station spacing, used to squeeze a stretch that
        /// is otherwise taller than a sheet. Never affects the millimetres-per-kilometre scale.</param>
        public GraphSettings ToPrintGraphSettings(int longestSignatureLength, double? stationSpacingMm = null)
        {
            var gt = settings.GraphicTimetable;
            return settings.ToGraphSettings() with
            {
                MinuteSpacing = Math.Max(1, (int)Math.Round(gt.PrintHourSpacingMm * UnitsPerMillimetre / 60.0)),
                KilometerSpacing = Math.Max(0, Units(gt.PrintKilometerSpacingMm)),
                MinStationSpacing = Math.Max(1, Units(stationSpacingMm ?? gt.PrintStationSpacingMm)),
                TrackSpacing = Math.Max(1, Units(gt.PrintTrackSpacingMm)),
                TimeAxisSpacing = new(Units(TimeAxisGutterMm), Units(TimeAxisGutterMm)),
                KilometerAxisSpacing = new(Units(DistanceAxisGutterMm(longestSignatureLength)), Units(DistanceAxisGutterVerticalMm)),
                EndMargin = Units(EndMarginMm),
                UnitsPerPoint = UnitsPerPoint,
                LabelLift = Units(LabelLiftMm),
                TrainStrokeWidth = Units(TrainStrokeWidthMm),
                LabelGap = Units(LabelGapMm),
                KmLabelGap = Units(KmLabelGapMm),
                TrackNumberGap = Units(TrackNumberGapMm),
                TrackNumberBaseline = Units(TrackNumberBaselineMm),
                StationNameBaseline = Units(StationNameBaselineMm),
            };
        }
    }

    extension(GraphSettings print)
    {
        /// <summary>
        /// How many minutes of the time axis fit one sheet at this scale, once the distance-axis gutter and the
        /// end margin are taken off the printable length. Derived from the settings actually drawn with — not
        /// from the requested millimetres per hour — so the page split cannot drift from the drawing by the
        /// rounding that made the spacing a whole number of units.
        /// </summary>
        public int MinutesPerPage(GraphPageGeometry geometry)
        {
            var gutter = print.AxisDirection == TimeAxisDirection.Vertical
                ? print.KilometerAxisSpacing.Y
                : print.KilometerAxisSpacing.X;
            var usable = Units(geometry.UsableTimeLengthMm) - gutter - print.EndMargin;
            return Math.Max(1, usable / Math.Max(1, print.MinuteSpacing));
        }
    }

    extension(GraphSchedule graph)
    {
        /// <summary>
        /// The graph's extent across the time axis in millimetres — its printed height with a horizontal time
        /// axis, its width with a vertical one. Measured by the same code that draws it, so what the paginator
        /// fits and what the browser prints cannot disagree.
        /// </summary>
        public double CrossAxisLengthMm() =>
            Millimetres(graph.AxisDirection == TimeAxisDirection.Vertical ? graph.Width() : graph.Height());

        /// <summary>The graph's extent along the time axis in millimetres.</summary>
        public double TimeAxisLengthMm() =>
            Millimetres(graph.AxisDirection == TimeAxisDirection.Vertical ? graph.Height() : graph.Width());
    }
}
