using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers the four display forms a <see cref="Sessions"/> value takes (all-sessions, contiguous run,
/// individual, plus the additive on-demand marker) and the rule that the text and markup renderings
/// always agree about which form applies.
/// </summary>
[TestClass]
public class SessionsFormattingTests
{
    private static void WithCulture(string culture, Action test)
    {
        var c = CultureInfo.GetCultureInfo(culture);
        var (originalCulture, originalUiCulture) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        try
        {
            CultureInfo.CurrentCulture = c;
            CultureInfo.CurrentUICulture = c;
            test();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private static SessionsSettings Numbers(int maxSessions = 14) => SessionsSettings.UseSessions(maxSessions);

    private static SessionsSettings FullDays(int maxSessions = 7, DayOfWeek startDay = DayOfWeek.Monday) =>
        SessionsSettings.UseWeekdays(maxSessions, useShortDayNames: false, startDay);

    // Counts the session circles in a markup rendering.
    private static int CircleCount(string html) =>
        html.Split("<svg", StringSplitOptions.None).Length - 1;

    [TestMethod]
    public void ARunOfThreeOrMoreAbbreviatesToFirstAndLast() => WithCulture("en-GB", () =>
    {
        var target = Sessions.FromSessionNumbers(1, 2, 3);

        Assert.AreEqual("1–3", target.ToText(Numbers()));
        Assert.AreEqual(2, CircleCount(target.ToHtml(Numbers()).Value),
            "The short form stays graphical: two circles joined by a dash.");
    });

    [TestMethod]
    public void ARunOfTwoStaysIndividual() => WithCulture("en-GB", () =>
    {
        // Two circles joined by a dash occupy the same width as two circles, and read worse.
        var target = Sessions.FromSessionNumbers(4, 5);

        Assert.AreEqual("4, 5", target.ToText(Numbers()));
        Assert.AreEqual(2, CircleCount(target.ToHtml(Numbers()).Value));
    });

    [TestMethod]
    public void ScatteredSessionsRenderIndividually() => WithCulture("en-GB", () =>
    {
        var target = Sessions.FromSessionNumbers(1, 3, 5);

        Assert.AreEqual("1, 3, 5", target.ToText(Numbers()));
        Assert.AreEqual(3, CircleCount(target.ToHtml(Numbers()).Value));
    });

    [TestMethod]
    public void RunsAndLoneSessionsMixFreely() => WithCulture("en-GB", () =>
    {
        // 1,2,3,7,8,9 abbreviates each run independently.
        Assert.AreEqual("1–3, 7–9", Sessions.FromSessionNumbers(1, 2, 3, 7, 8, 9).ToText(Numbers()));
        // 1,2,3,5 mixes an abbreviated run with a lone session.
        Assert.AreEqual("1–3, 5", Sessions.FromSessionNumbers(1, 2, 3, 5).ToText(Numbers()));
    });

    [TestMethod]
    public void AllSessionsIsJudgedAgainstTheLayoutsOperatingPeriod() => WithCulture("en-GB", () =>
    {
        var target = Sessions.FromSessionNumbers(1, 2, 3);

        Assert.AreEqual("All sessions", target.ToText(Numbers(maxSessions: 3)),
            "A three-session meeting where the duty runs all three is 'all sessions' for that meeting.");
        Assert.AreEqual("1–3", target.ToText(Numbers(maxSessions: 14)),
            "Over a fourteen-session period the same value is just a run.");
    });

    [TestMethod]
    public void AnEmptyPatternSaysNoneRatherThanPrintingNothing() => WithCulture("en-GB", () =>
    {
        var target = Sessions.FromSessionNumbers();

        Assert.AreEqual("No sessions", target.ToText(Numbers()));
        Assert.AreEqual(0, CircleCount(target.ToHtml(Numbers()).Value));
        Assert.AreEqual("No operating days", Sessions.FromDays(Days.None).ToText(FullDays()),
            "A day-based layout says it in days.");
    });

    [TestMethod]
    public void OnDemandIsAddedToTheSessionsRatherThanReplacingThem() => WithCulture("en-GB", () =>
    {
        var target = Sessions.FromBitPattern(0b111 | CommonSessionPatterns.OnDemand);

        var text = target.ToText(Numbers());

        Assert.Contains("1–3", text, "The sessions stay visible: a duty can run 1-3 and be worked on demand.");
        Assert.Contains("On demand only", text);
        Assert.AreEqual(2, CircleCount(target.ToHtml(Numbers()).Value));
    });

    [TestMethod]
    public void ThreeContiguousDaysReadAsARange() => WithCulture("en-GB", () =>
    {
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday);

        Assert.AreEqual("Monday to Wednesday", target.ToText(FullDays()));
    });

    [TestMethod]
    public void ScatteredDaysAreListed() => WithCulture("en-GB", () =>
    {
        var target = Sessions.FromDays(Days.Monday | Days.Wednesday | Days.Friday);

        Assert.AreEqual("Monday, Wednesday, Friday", target.ToText(FullDays()));
    });

    [TestMethod]
    public void EveryDayOfThePeriodReadsAsDaily() => WithCulture("en-GB", () =>
    {
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday);

        Assert.AreEqual("Daily", target.ToText(FullDays(maxSessions: 3)));
    });

    [TestMethod]
    public void DaysStayTextualInTheMarkupForm() => WithCulture("en-GB", () =>
    {
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday);

        var html = target.ToHtml(FullDays()).Value;

        Assert.AreEqual(0, CircleCount(html), "Day names have a proven textual form and keep it.");
        Assert.Contains("Monday to Wednesday", html);
    });

    [TestMethod]
    public void DayPositionsAreNotCountedTwice() => WithCulture("en-GB", () =>
    {
        // A day pattern mirrors its in-week bits into the upper session bits. Reading the raw numbers
        // would render Monday twice; the day offsets are what the day form must use.
        var target = Sessions.FromDays(Days.Monday);

        Assert.AreEqual("Monday", target.ToText(FullDays()));
    });

    [TestMethod]
    public void TextAndMarkupAgreeAboutTheForm() => WithCulture("en-GB", () =>
    {
        // The form is a property of the value, so the abbreviating cases must match: where the text
        // abbreviates to first-last, the markup draws two circles, and where it lists, it draws one each.
        foreach (var numbers in new[]
        {
            new[] { 1, 2, 3 },
            [4, 5],
            [1, 3, 5],
            [1, 2, 3, 5],
            [1, 2, 3, 7, 8, 9],
        })
        {
            var target = Sessions.FromSessionNumbers(numbers);
            var text = target.ToText(Numbers());
            var circles = CircleCount(target.ToHtml(Numbers()).Value);
            Assert.AreEqual(ExpectedCircles(numbers), circles,
                $"Text '{text}' and its markup must describe the same shape.");
        }

        // An abbreviated run draws two circles whatever its length; a short run draws one per session.
        static int ExpectedCircles(int[] numbers)
        {
            var total = 0;
            var runLength = 1;
            for (var i = 1; i <= numbers.Length; i++)
            {
                if (i < numbers.Length && numbers[i] == numbers[i - 1] + 1) { runLength++; continue; }
                total += runLength >= 3 ? 2 : runLength;
                runLength = 1;
            }
            return total;
        }
    });

    [TestMethod]
    public void TwoDigitSessionNumeralsShrinkToStayInsideTheCircle() => WithCulture("en-GB", () =>
    {
        var single = Sessions.FromSessionNumbers(9).ToHtml(Numbers()).Value;
        var double_ = Sessions.FromSessionNumbers(14).ToHtml(Numbers()).Value;

        Assert.Contains("font-size=\"62\"", single);
        Assert.Contains("font-size=\"48\"", double_);
    });

    [TestMethod]
    public void AskingForShortFormsAbbreviatesTheWholeAndEmptyValues() => WithCulture("en-GB", () =>
    {
        var brief = new SessionsSettings { MaxNumberOfSessions = 4, UseShortWeekdayNames = true };
        var none = Sessions.FromBitPattern(0);

        // The printed reports ask for short forms, where the column is a few millimetres wide and the
        // word "sessions" is already the heading above it.
        Assert.AreEqual("All", Sessions.All.ToText(brief));
        Assert.AreEqual("None", none.ToText(brief));

        // The text and markup forms must not disagree about the wording.
        Assert.AreEqual("All", Sessions.All.ToHtml(brief).Value);
        Assert.AreEqual("None", none.ToHtml(brief).Value);

        // Anything asking for the long form is unchanged.
        Assert.AreEqual("All sessions", Sessions.All.ToText(Numbers(4)));
        Assert.AreEqual("No sessions", none.ToText(Numbers(4)));
    });

    [TestMethod]
    public void CirclesAreDrawnWithAnSvgFillSoTheyPrint() => WithCulture("en-GB", () =>
    {
        var html = Sessions.FromSessionNumbers(1).ToHtml(Numbers()).Value;

        // A CSS background colour would be dropped by the printer, on exactly the artefact this exists for.
        Assert.Contains("fill=\"black\"", html);
        Assert.DoesNotContain("background-color", html);
        Assert.Contains("aria-label=\"1\"", html, "The number stays available to screen readers and text extraction.");
    });
}
