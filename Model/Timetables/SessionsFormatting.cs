using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Renders a <see cref="Sessions"/> value for display, as plain text or as markup, choosing between
/// session numbers and weekday names from the supplied <see cref="SessionsSettings"/>.
/// </summary>
/// <remarks>
/// <para>
/// A value takes one of three shapes, plus a marker that combines with any of them: it covers the whole
/// operating period ("All sessions" / "Daily"); it is a set of contiguous runs, each abbreviated to its
/// first and last when it is long enough; or it is a scattered set rendered item by item. On demand is
/// additive — it never replaces the sessions, because a duty can run sessions 1–3 <em>and</em> be worked
/// on demand on those three, and the reader needs both facts.
/// </para>
/// <para>
/// The shape is a property of the value rather than of the rendering, so it is decided once in
/// <see cref="FormOf"/> and the two renderers work from that result. Deciding it twice would guarantee
/// eventual disagreement — a tooltip saying something other than the circles beside it.
/// </para>
/// </remarks>
public static class SessionsFormatting
{
    /// <summary>
    /// The shortest run abbreviated to "first–last". A run of two is excluded because ②–③ occupies the
    /// same width as ② ③ while reading worse, and the day form gains nothing from "Monday to Tuesday"
    /// either. Both modes therefore abbreviate at the same point.
    /// </summary>
    private const int ShortFormThreshold = 3;

    extension(Sessions sessions)
    {
        /// <summary>
        /// The plain-text form: <c>All sessions</c>, <c>1–3</c>, <c>1, 3, 5</c>, <c>Monday to Wednesday</c>.
        /// </summary>
        /// <remarks>
        /// Used wherever markup cannot go — note bodies, validation messages, tooltips and <c>title</c>
        /// attributes, exports — and for report height estimation, which charges wrapping per character
        /// and would count a short value as a long one if it measured markup.
        /// </remarks>
        /// <param name="settings">Chooses sessions or days, the operating period and the day names.</param>
        public string ToText(SessionsSettings settings)
        {
            settings = settings.ValueOrException(nameof(settings));
            var form = FormOf(sessions, settings);
            var body = TextBody(form, settings);
            return form.IsOnDemand ? Join(body, DaysExtensions.DayResource("OnDemandOnly")) : body;
        }

        /// <summary>
        /// The markup form: the same content with session numbers drawn as filled circles, or day names as
        /// text. This is the single source of the session markup — the shared Blazor component is a thin
        /// wrapper that emits it, so the circles are drawn by one piece of code whether they are rendered
        /// directly or embedded inside another note's markup.
        /// </summary>
        /// <param name="settings">Chooses sessions or days, the operating period and the day names.</param>
        public MarkupString ToHtml(SessionsSettings settings)
        {
            settings = settings.ValueOrException(nameof(settings));
            var form = FormOf(sessions, settings);
            var body = settings.UseDaysInsteadOfSessionNumbers
                ? WebUtility.HtmlEncode(TextBody(form, settings))
                : HtmlBody(form);
            return new(form.IsOnDemand
                ? Join(body, WebUtility.HtmlEncode(DaysExtensions.DayResource("OnDemandOnly")))
                : body);
        }
    }

    // A contiguous run of session/day positions, inclusive at both ends.
    private readonly record struct Run(int First, int Last)
    {
        internal int Length => Last - First + 1;
    }

    // What a value looks like, independent of how it is rendered.
    private sealed record Form(bool IsNone, bool IsAll, IReadOnlyList<Run> Runs, bool IsOnDemand);

    private static Form FormOf(Sessions sessions, SessionsSettings settings)
    {
        var useDays = settings.UseDaysInsteadOfSessionNumbers;
        var maxSessions = settings.MaxNumberOfSessions;
        var onDemand = sessions.IsOnDemand;
        var capped = sessions.CappedForDisplay(useDays, maxSessions);

        // Day patterns mirror their in-week bits into the upper session bits, so the day positions must
        // come from DayOffsets; taking Numbers would count every day twice.
        var positions = useDays
            ? [.. capped.DayOffsets.Select(offset => offset + 1)]
            : capped.Numbers.Select(number => (int)number).ToArray();

        if (positions.Length == 0) return new(IsNone: true, IsAll: false, [], onDemand);

        // "All" is judged against this layout's operating period, not against fourteen: a three-session
        // meeting where the duty runs all three is "all sessions" for that meeting.
        if (sessions.CoversAllWithin(useDays, maxSessions)) return new(false, IsAll: true, [], onDemand);

        return new(false, false, RunsOf(positions), onDemand);
    }

    // Splits ascending positions into contiguous runs.
    private static List<Run> RunsOf(int[] positions)
    {
        var runs = new List<Run>();
        var start = 0;
        for (var i = 1; i <= positions.Length; i++)
        {
            if (i < positions.Length && positions[i] == positions[i - 1] + 1) continue;
            runs.Add(new Run(positions[start], positions[i - 1]));
            start = i;
        }
        return runs;
    }

    private static string TextBody(Form form, SessionsSettings settings)
    {
        var useDays = settings.UseDaysInsteadOfSessionNumbers;
        var useShort = useDays && settings.UseShortWeekdayNames;

        // "No operating days" and "No sessions" are different statements; a layout is one or the other.
        if (form.IsNone) return DaysExtensions.DayResource(useDays ? (useShort ? "NoneShort" : "None") : "NoSessions");
        if (form.IsAll) return DaysExtensions.DayResource(useDays ? (useShort ? "DailyShort" : "Daily") : "AllSessions");

        var parts = form.Runs.Select(run => useDays ? DayRunText(run, settings, useShort) : SessionRunText(run));
        return string.Join(", ", parts);
    }

    private static string DayRunText(Run run, SessionsSettings settings, bool useShort)
    {
        var startDay = settings.SessionFirstWeekday;
        // Positions are 1-based here; DayNameAt takes the 0-based operating-week offset.
        string NameOf(int position) => DaysExtensions.DayNameAt(startDay, position - 1, useShort);

        if (run.Length < ShortFormThreshold)
            return string.Join(", ", Enumerable.Range(run.First, run.Length).Select(NameOf));

        // Short day names use a hyphen (Mo-We); full names spell out the localised connector.
        return useShort
            ? $"{NameOf(run.First)}-{NameOf(run.Last)}"
            : $"{NameOf(run.First)} {DaysExtensions.DayResource("DayRangeConnector")} {NameOf(run.Last)}";
    }

    private static string SessionRunText(Run run) =>
        run.Length < ShortFormThreshold
            ? string.Join(", ", Enumerable.Range(run.First, run.Length).Select(n => n.ToString(CultureInfo.CurrentCulture)))
            : $"{run.First}–{run.Last}";

    private static string HtmlBody(Form form)
    {
        if (form.IsNone) return WebUtility.HtmlEncode(DaysExtensions.DayResource("NoSessions"));
        if (form.IsAll) return WebUtility.HtmlEncode(DaysExtensions.DayResource("AllSessions"));

        var builder = new StringBuilder();
        foreach (var run in form.Runs)
        {
            if (builder.Length > 0) builder.Append(' ');
            if (run.Length < ShortFormThreshold)
            {
                for (var number = run.First; number <= run.Last; number++)
                {
                    if (number > run.First) builder.Append(' ');
                    builder.Append(Circle(number));
                }
            }
            else
            {
                // The short form stays graphical — two circles joined by an en dash — so the visual
                // language survives abbreviation; only the count of circles falls.
                builder.Append(Circle(run.First)).Append("–").Append(Circle(run.Last));
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// One session number as a filled circle with the numeral centred in white — the same circle
    /// <see cref="SessionsFormatting.ToHtml"/> draws, for callers that have a bare number rather than a
    /// <see cref="Sessions"/> value.
    /// </summary>
    /// <remarks>
    /// Drawn as SVG with a <c>fill</c> attribute rather than a CSS background colour: browsers drop
    /// background colours when printing unless <c>print-color-adjust: exact</c> is set, which would make
    /// the indicator vanish on exactly the printed artefact it exists for. The element carries no width or
    /// height, so CSS sizes it in <c>em</c> and it is always as tall as the text beside it.
    /// </remarks>
    /// <param name="number">The session number to draw.</param>
    public static MarkupString SessionNumberHtml(int number) => new(Circle(number));

    private static string Circle(int number)
    {
        // Two-digit numerals must shrink to stay inside the circle; sessions run to 14.
        var fontSize = number < 10 ? 62 : 48;
        return $"""
            <svg class="sessionnumber" viewBox="0 0 100 100" role="img" aria-label="{number}"><circle cx="50" cy="50" r="50" fill="black" /><text x="50" y="50" fill="white" text-anchor="middle" dominant-baseline="central" font-size="{fontSize}" font-weight="bold">{number}</text></svg>
            """;
    }

    // Joins the sessions and the on-demand marker, tolerating an empty first half.
    private static string Join(string body, string marker) =>
        string.IsNullOrEmpty(body) ? marker : $"{body} {marker}";
}
