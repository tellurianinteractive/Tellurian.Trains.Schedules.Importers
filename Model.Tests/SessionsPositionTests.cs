using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers the position-by-position view of a <see cref="Sessions"/> value — the one a printed tick-off
/// grid is laid out from — and the rule that it always agrees with the phrase rendered beside it.
/// </summary>
[TestClass]
public class SessionsPositionTests
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

    private static SessionsSettings ShortDays(int maxSessions = 14, DayOfWeek startDay = DayOfWeek.Monday) =>
        SessionsSettings.UseWeekdays(maxSessions, useShortDayNames: true, startDay);

    [TestMethod]
    public void ThePeriodHasOnePositionPerSession()
    {
        Assert.AreSequenceEqual(new[] { 1, 2, 3, 4 }, SessionsFormatting.PositionsOf(Numbers(4)));
    }

    [TestMethod]
    public void CountingInDaysCapsThePeriodAtOneWeek()
    {
        // A week has seven days however many sessions the period holds — the same cap the values
        // themselves are displayed under.
        Assert.HasCount(7, SessionsFormatting.PositionsOf(ShortDays(14)));
    }

    [TestMethod]
    public void OnlyTheSessionsTheValueCoversAreTicked()
    {
        var sessions = Sessions.FromSessionNumbers(1, 3);
        var settings = Numbers(4);

        Assert.AreSequenceEqual(
            new[] { true, false, true, false },
            SessionsFormatting.PositionsOf(settings).Select(p => sessions.Covers(p, settings)).ToArray());
    }

    [TestMethod]
    public void APositionIsAPlaceInTheOperatingWeek()
    {
        var firstTwoDays = Sessions.FromBitPattern(0b11);

        // The bits are positional, so which boxes are ticked never moves when the start day changes —
        // only which weekday each column is headed with does.
        foreach (var startDay in new[] { DayOfWeek.Monday, DayOfWeek.Saturday })
        {
            var settings = ShortDays(7, startDay);
            Assert.IsTrue(firstTwoDays.Covers(1, settings));
            Assert.IsTrue(firstTwoDays.Covers(2, settings));
            Assert.IsFalse(firstTwoDays.Covers(3, settings));
        }
    }

    [TestMethod]
    public void ASessionOutsideThePeriodIsNeverCovered()
    {
        // Capped the same way the phrase is, so a box is never ticked for a session the text beside it
        // does not name.
        Assert.IsFalse(Sessions.FromSessionNumbers(9).Covers(9, Numbers(4)));
    }

    [TestMethod]
    public void ASessionHeadingIsItsCircleAndADayHeadingItsShortName() => WithCulture("en-GB", () =>
    {
        Assert.Contains("<svg", SessionsFormatting.PositionHeadingOf(3, Numbers(4)).Value);
        Assert.AreEqual("Mo", SessionsFormatting.PositionHeadingOf(1, ShortDays(7)).Value);

        // Naming the positions is the only thing the start day changes; see APositionIsAPlaceInTheOperatingWeek.
        Assert.AreEqual("Sa", SessionsFormatting.PositionHeadingOf(1, ShortDays(7, DayOfWeek.Saturday)).Value);
    });
}
