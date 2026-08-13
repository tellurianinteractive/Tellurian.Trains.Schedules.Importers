using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// Paper geometry for a printed graphical timetable. Both orientations use the same numbers: a graph with a
/// horizontal time axis prints on A4 landscape with time running along the 297&#160;mm side, and one with a
/// vertical time axis prints on A4 portrait with time running down the same 297&#160;mm side. The two are exact
/// transposes, so one geometry and one paginator serve both; only the page wrapper component differs.
/// <para>
/// Measurements are millimetres. As in <see cref="PageGeometry"/> the arithmetic is deliberately pure, so
/// pagination is unit-testable and never measures the DOM. Unlike the tabular report, the graph's own extent is
/// not estimated from type sizes: it is measured by the very code that draws it (<c>Height()</c>/<c>Width()</c>
/// on a print-scaled <c>GraphSchedule</c>), so the fit cannot drift from the drawing.
/// </para>
/// </summary>
public sealed record GraphPageGeometry
{
    /// <summary>Length of the sheet along the time axis: the 297&#160;mm side of A4 in either orientation.</summary>
    public double TimeAxisLengthMm { get; init; } = 297;

    /// <summary>Length of the sheet across the time axis: the 210&#160;mm side of A4 in either orientation.</summary>
    public double CrossAxisLengthMm { get; init; } = 210;

    /// <summary>Margin kept on every edge of the sheet, matching the page wrapper's <c>Margin</c>.</summary>
    public double MarginMm { get; init; } = 10;

    /// <summary>Height of the heading printed above each graph (stretch description and time span).</summary>
    public double HeadingHeightMm { get; init; } = 6;

    /// <summary>Blank space between two graphs stacked on the same sheet.</summary>
    public double GraphGapMm { get; init; } = 5;

    /// <summary>
    /// The overlap a seam aims for: minutes repeated from the previous sheet, so a train crossing the cut can be
    /// read on both. The sheets of one time span are spread evenly, so the actual overlap is whatever slack the
    /// sheet count leaves and is usually well over this; the value's real job is to force one more sheet in the
    /// rare case where the span divides exactly and there would be no overlap at all. A seam at the break time is
    /// a clean cut and never overlaps.
    /// </summary>
    public int SeamOverlapMinutes { get; init; } = 15;

    /// <summary>Length available to the graphs along the time axis, once the margins are taken.</summary>
    public double UsableTimeLengthMm => TimeAxisLengthMm - (2 * MarginMm);

    /// <summary>Length available to the stacked graphs across the time axis, once the margins are taken. The
    /// report prints no page footer, so nothing else is reserved: a graph is identified by its own heading, and
    /// a footer would only cost a graph's worth of paper on every sheet.</summary>
    public double UsableCrossLengthMm => CrossAxisLengthMm - (2 * MarginMm);

    /// <summary>Default A4 geometry with 10&#160;mm margins.</summary>
    public static GraphPageGeometry A4 { get; } = new();
}

/// <summary>
/// One stretch offered to the paginator, already measured at the print scale.
/// </summary>
/// <param name="Stretch">The stretch to draw.</param>
/// <param name="CrossLengthMm">Its measured extent across the time axis, including its own axis gutters.</param>
/// <param name="StationSpacingMm">The minimum station spacing this stretch was measured with. Normally the
/// setting; reduced for a stretch that would otherwise be taller than one sheet, since a graph cannot be split
/// across the distance axis. Only this legibility floor is reduced, never the millimetres-per-kilometre scale,
/// so real distances stay comparable between sheets.</param>
public sealed record GraphPageItem(TimetableStretch Stretch, double CrossLengthMm, double StationSpacingMm);

/// <summary>One printed sheet: a single time span, and the stretches stacked on it in render order.</summary>
/// <param name="From">First time on the sheet's time axis.</param>
/// <param name="To">Last time on the sheet's time axis.</param>
/// <param name="Items">The stretches drawn on this sheet, in order along the stacking direction.</param>
/// <param name="IsContinued">True when this sheet continues the previous sheet's time span, so its heading is
/// suffixed with ", continued".</param>
public sealed record GraphPage(TimeSpan From, TimeSpan To, IReadOnlyList<GraphPageItem> Items, bool IsContinued);

/// <summary>
/// Splits the operating window into page-sized time spans and packs the stretches onto sheets.
/// </summary>
public static class GraphPaginator
{
    /// <summary>
    /// Builds the printed sheets. The time axis is sliced first — at the break time when there is one, then into
    /// page-sized spans with an overlap at each further seam — and every slice is then filled with as many
    /// stretches as the cross axis holds, in the order given. A slice therefore always shows the same time span
    /// for every stretch on the sheet, which is what makes meets readable across stacked graphs.
    /// </summary>
    /// <param name="items">The stretches to print, in report order.</param>
    /// <param name="start">First time of the operating window.</param>
    /// <param name="end">Last time of the operating window.</param>
    /// <param name="breakTime">The layout's break time. Used as the first seam when it falls strictly inside the
    /// window, matching the first/last half split operators already know; ignored otherwise.</param>
    /// <param name="minutesPerPage">Minutes that fit one sheet at the print scale, derived from the print
    /// settings by the caller so the split matches the drawing exactly.</param>
    /// <param name="geometry">The paper geometry.</param>
    public static IReadOnlyList<GraphPage> BuildPages(
        IEnumerable<GraphPageItem> items,
        TimeSpan start,
        TimeSpan end,
        TimeSpan? breakTime,
        int minutesPerPage,
        GraphPageGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(geometry);

        var list = items.ToList();
        var pages = new List<GraphPage>();
        if (list.Count == 0 || end <= start || minutesPerPage <= 0) return pages;

        foreach (var (from, to, isContinued) in TimeSlices(start, end, breakTime, minutesPerPage, geometry.SeamOverlapMinutes))
            foreach (var group in Pack(list, geometry))
                pages.Add(new GraphPage(from, to, group, isContinued));

        return pages;
    }

    /// <summary>
    /// The time spans the window is cut into. The break time, when strictly inside the window, is a clean cut:
    /// both halves are then sliced independently, so a sheet never straddles the break. Each half is then covered
    /// by the fewest sheets that hold it, spread evenly so the first begins at its start, the last ends at its
    /// end, and the seams between them overlap.
    /// </summary>
    public static IEnumerable<(TimeSpan From, TimeSpan To, bool IsContinued)> TimeSlices(
        TimeSpan start, TimeSpan end, TimeSpan? breakTime, int minutesPerPage, int overlapMinutes)
    {
        var seam = breakTime is { } b && b > start && b < end ? b : (TimeSpan?)null;
        (TimeSpan Start, TimeSpan End)[] segments = seam is { } s ? [(start, s), (s, end)] : [(start, end)];

        foreach (var (segmentStart, segmentEnd) in segments)
        {
            var length = (segmentEnd - segmentStart).TotalMinutes;
            if (length <= minutesPerPage)
            {
                yield return (segmentStart, segmentEnd, false);
                continue;
            }

            // At a fixed scale every sheet is the same width whatever it carries, so the only real question is
            // how many sheets, and the fewest is the right answer. Spreading that many evenly from the start of
            // the segment to its end then fills every sheet — no ragged tail, no sliver page — and hands all the
            // slack back as overlap at the seams, which is normally far more than the requested minimum. Only
            // when the segment divides exactly into whole sheets is there no slack at all; one more sheet is
            // then worth it, since a seam with no overlap cuts every train that crosses it.
            var count = (int)Math.Ceiling(length / minutesPerPage);
            if (overlapMinutes > 0 && (count * minutesPerPage) - length < count - 1) count++;

            var step = (length - minutesPerPage) / (count - 1);
            for (var i = 0; i < count; i++)
            {
                var from = i == count - 1 ? segmentEnd - TimeSpan.FromMinutes(minutesPerPage) : segmentStart + TimeSpan.FromMinutes(i * step);
                yield return (from, from + TimeSpan.FromMinutes(minutesPerPage), i > 0);
            }
        }
    }

    /// <summary>
    /// Groups the stretches into sheet-sized stacks along the cross axis. A stretch that is taller than a whole
    /// sheet even on its own still gets its own sheet: the caller has already reduced its station spacing as far
    /// as it may, and printing it slightly clipped beats dropping it silently.
    /// </summary>
    private static IEnumerable<IReadOnlyList<GraphPageItem>> Pack(IReadOnlyList<GraphPageItem> items, GraphPageGeometry geometry)
    {
        var usable = geometry.UsableCrossLengthMm;
        var current = new List<GraphPageItem>();
        var used = 0.0;

        foreach (var item in items)
        {
            var needed = geometry.HeadingHeightMm + item.CrossLengthMm + (current.Count > 0 ? geometry.GraphGapMm : 0);
            if (current.Count > 0 && used + needed > usable)
            {
                yield return current;
                current = [];
                used = geometry.HeadingHeightMm + item.CrossLengthMm;
            }
            else
            {
                used += needed;
            }
            current.Add(item);
        }

        if (current.Count > 0) yield return current;
    }
}
