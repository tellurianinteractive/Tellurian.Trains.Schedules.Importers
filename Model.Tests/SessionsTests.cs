using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class SessionsTests
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

    [TestMethod]
    public void ActiveDaysStartingMondayMapsOddToMonWedFriSun()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var expected = new[] { Days.Monday, Days.Wednesday, Days.Friday, Days.Sunday };
        CollectionAssert.AreEqual(expected, target.ActiveDays(DayOfWeek.Monday));
    }

    [TestMethod]
    public void ActiveDaysStartingSundayMapsOddToSunTueThuSat()
    {
        // The session bits are positional: session 1 (bit 0) is the start day, so a Sunday start shifts
        // the whole odd pattern to Sun, Tue, Thu, Sat.
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var expected = new[] { Days.Sunday, Days.Tuesday, Days.Thursday, Days.Saturday };
        CollectionAssert.AreEqual(expected, target.ActiveDays(DayOfWeek.Sunday));
    }

    [TestMethod]
    public void DaysShortStartingSundayUsesEnglishShortNames() => WithCulture("en-US", () =>
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        Assert.AreEqual("Su, Tu, Th, Sa", target.DaysShort(DayOfWeek.Sunday));
    });

    [TestMethod]
    public void DaysFullStartingMondayUsesSwedishNamesInWeekOrder() => WithCulture("sv-SE", () =>
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        Assert.AreEqual("Tisdag, Torsdag, Lördag", target.DaysFull(DayOfWeek.Monday));
    });

    [TestMethod]
    public void DaysFullForAllSessionsIsTranslatedDaily() => WithCulture("sv-SE", () =>
    {
        Assert.AreEqual("Dagligen", Sessions.All.DaysFull(DayOfWeek.Monday));
    });

    [TestMethod]
    public void DaysFullForNoDaysIsTranslatedNone() => WithCulture("sv-SE", () =>
    {
        var target = Sessions.FromSessionNumbers();
        Assert.AreEqual("Inga kördagar", target.DaysFull(DayOfWeek.Monday));
    });

    [TestMethod]
    public void ConsecutiveDaysAreShownAsRange() => WithCulture("en-US", () =>
    {
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday);
        Assert.AreEqual("Mo-Fr", target.DaysShort(DayOfWeek.Monday));
        Assert.AreEqual("Monday to Friday", target.DaysFull(DayOfWeek.Monday));
    });

    [TestMethod]
    public void LongRangeUsesLocalisedConnector() => WithCulture("sv-SE", () =>
    {
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday);
        Assert.AreEqual("Måndag till Fredag", target.DaysFull(DayOfWeek.Monday));
    });

    [TestMethod]
    public void FirstThreeSessionsAreConsecutiveFromAnyStartDay() => WithCulture("en-US", () =>
    {
        // Sessions 1-3 occupy positions 0-2, an unbroken run whatever weekday the week starts on.
        var target = Sessions.FromSessionNumbers(1, 2, 3);
        Assert.AreEqual("Su-Tu", target.DaysShort(DayOfWeek.Sunday));
        Assert.AreEqual("Mo-We", target.DaysShort(DayOfWeek.Monday));
    });

    [TestMethod]
    public void NonAdjacentSessionPositionsAreListedNotRanged() => WithCulture("sv-SE", () =>
    {
        // Sessions 1, 2 and 7 sit at positions 0, 1 and 6 — a gap, so Mo, Ti, Sö is listed not ranged.
        var target = Sessions.FromSessionNumbers(1, 2, 7);
        Assert.AreEqual("M, Ti, S", target.DaysShort(DayOfWeek.Monday));
    });

    [TestMethod]
    public void ConsecutiveSessionsAreShownAsRange()
    {
        var target = Sessions.FromSessionNumbers(1, 2, 3, 4, 5);
        Assert.AreEqual("1-5", target.SessionsNumbers);
    }

    [TestMethod]
    public void SeparateRunsAreShownAsCommaSeparatedRanges()
    {
        // Mon-Fri stored as days mirrors the weekday bits into the upper session bits (1-5 and 8-12).
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday);
        Assert.AreEqual("1-5,8-12", target.SessionsNumbers);
    }

    [TestMethod]
    public void CappingIgnoresHigherSessionBitsInText()
    {
        // A Mon-Fri days pattern mirrors into sessions 1-5 and 8-12; capping at 6 drops everything from
        // session 7 up, leaving a clean 1-5.
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday);
        Assert.AreEqual("1-5", target.Capped(6).SessionsNumbers);
    }

    [TestMethod]
    public void CappingLeavesTheStoredValueUnchanged()
    {
        var target = Sessions.FromSessionNumbers(1, 2, 3, 8);
        _ = target.Capped(6);
        Assert.AreEqual("1-3,8", target.SessionsNumbers, "Capping must not mutate the original value.");
    }

    [TestMethod]
    public void CappingAtFourteenOrMoreIsANoOp()
    {
        var target = Sessions.FromSessionNumbers(1, 2, 8, 14);
        Assert.AreEqual(target.SessionsNumbers, target.Capped(14).SessionsNumbers);
    }

    [TestMethod]
    public void CappedForDisplayKeepsMondayToSaturdayWhenWeekStartsMonday() => WithCulture("sv-SE", () =>
    {
        // Every weekday, a six-day week starting Monday: Sunday falls outside the period.
        var everyDay = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday | Days.Saturday | Days.Sunday);
        var capped = everyDay.CappedForDisplay(useDays: true, maxSessions: 6);
        Assert.AreEqual("M-L", capped.DaysShort(DayOfWeek.Monday));
    });

    [TestMethod]
    public void CappedForDisplayKeepsSundayToFridayWhenWeekStartsSunday() => WithCulture("en-US", () =>
    {
        // Every weekday, a six-day week starting Sunday: Saturday falls outside the period.
        var everyDay = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday | Days.Saturday | Days.Sunday);
        var capped = everyDay.CappedForDisplay(useDays: true, maxSessions: 6);
        Assert.AreEqual("Su-Fr", capped.DaysShort(DayOfWeek.Sunday));
    });

    [TestMethod]
    public void CappedForDisplayShowsSessionRangeWhenNotUsingDays()
    {
        // A Mon-Fri days pattern mirrors into 1-5 and 8-12; as sessions capped at 6 it reads 1-5.
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday);
        Assert.AreEqual("1-5", target.CappedForDisplay(useDays: false, maxSessions: 6).SessionsNumbers);
    }

    [TestMethod]
    public void LoneSessionsAreShownIndividuallyBetweenRanges()
    {
        var target = Sessions.FromSessionNumbers(1, 3, 4, 5, 7);
        Assert.AreEqual("1,3-5,7", target.SessionsNumbers);
    }

    [TestMethod]
    public void IsAllSessions()
    {
        var target = Sessions.All;
        var actual = target.Numbers;
        Assert.HasCount(14, actual);
        Assert.AreEqual(1, actual[0]);
        Assert.AreEqual(14, actual[13]);
    }

    [TestMethod]
    public void IsOddSessions()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var actual = target.Numbers;
        Assert.HasCount(7, actual);
        Assert.AreEqual(1, actual[0]);
        Assert.AreEqual(13, actual[^1]);
    }

    [TestMethod]
    public void IsEvenSessions()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var actual = target.Numbers;
        Assert.HasCount(7, actual);
        Assert.AreEqual(2, actual[0]);
        Assert.AreEqual(14, actual[^1]);
    }

    [TestMethod]
    public void TwoSessionsWithDifferentDaysCombinedIsNoDays()
    {
        var odd = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var even = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var target = odd.And(even);
        var actual = target.Numbers;
        Assert.HasCount(0, actual);
    }

    [TestMethod]
    public void TwoSessionsWithComplementaryDaysCombinedIsAllDays()
    {
        var odd = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var even = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var target = odd.Or(even);
        var actual = target.Numbers;
        Assert.HasCount(14, actual);
        Assert.AreEqual(1, actual[0]);
        Assert.AreEqual(14, actual[^1]);
    }

    [TestMethod]
    public void IsOnDemand()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Even | CommonSessionPatterns.OnDemand);
        var actual = target.Numbers;
        Assert.HasCount(7, actual);
        Assert.AreEqual(2, actual[0]);
        Assert.AreEqual(14, actual[^1]);
        Assert.IsTrue(target.IsOnDemand);
    }

    [TestMethod]
    public void Daily()
    {
        var target = Sessions.FromBitPattern(CommonDayPatterns.Daily);
        var actual = target.ActiveDays(DayOfWeek.Monday);
        Assert.HasCount(7, actual);
        Assert.AreEqual(Days.Monday, actual[0], "First day in week is Monday");
        Assert.AreEqual(Days.Sunday, actual[^1], "Last day in week is Sunday");
    }

    [TestMethod]
    public void MondayWednesdayFridaySunday()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var actual = target.ActiveDays(DayOfWeek.Monday);
        Assert.HasCount(4, actual);
        Assert.AreEqual(Days.Monday, actual[0], "First day is Monday");
        Assert.AreEqual(Days.Sunday, actual[^1], "Last day is Sunday");
    }

    [TestMethod]
    public void TuesdayThursdaySaturday()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var actual = target.ActiveDays(DayOfWeek.Monday);
        Assert.HasCount(3, actual);
        Assert.AreEqual(Days.Tuesday, actual[0], "First day is Tuesday");
        Assert.AreEqual(Days.Saturday, actual[^1], "Last day is Saturday");
    }

    [TestMethod]
    public void ConstructorSetsCorrectDays()
    {
        var target = Sessions.FromDays(Days.Monday | Days.Friday);
        var actual = target.Numbers;
        Assert.HasCount(4, actual);
        var expected = new byte[] { 1, 5, 8, 12 };
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ConstructorSetsCorrectSessions()
    {
        var target = Sessions.FromSessionNumbers([1, 5, 8, 12]);
        var actual = target.Numbers;
        Assert.HasCount(4, actual);
        var expected = new byte[] { 1, 5, 8, 12 };
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void DayResourceNameIsConsequtive()
    {
        var target = Sessions.FromDays(Days.Tuesday | Days.Wednesday | Days.Thursday);
        var actual = target.FullDayNamesResourceKey(DayOfWeek.Monday);
        Assert.AreEqual("Tuesday-Thursday", actual);
    }

    [TestMethod]
    public void SortOrderPutsAllSessionsFirstAndNoneLast()
    {
        Assert.IsTrue(Sessions.All.SortOrder < Sessions.FromSessionNumbers(1).SortOrder,
            "Every session should sort before a single session.");
        Assert.AreEqual(int.MaxValue, Sessions.FromSessionNumbers().SortOrder,
            "An empty pattern should sort last.");
    }

    [TestMethod]
    public void SortOrderRanksEarlierStartFirst()
    {
        Assert.IsTrue(Sessions.FromSessionNumbers(1, 2, 3).SortOrder < Sessions.FromSessionNumbers(2, 3).SortOrder,
            "A pattern starting on session 1 should sort before one starting on session 2.");
    }

    [TestMethod]
    public void SortOrderRanksBroaderPatternFirstWithinTheSameStart()
    {
        Assert.IsTrue(Sessions.FromSessionNumbers(1, 2, 3).SortOrder < Sessions.FromSessionNumbers(1).SortOrder,
            "Among patterns starting on session 1, the broader one should sort first.");
    }

    [TestMethod]
    public void SortOrderRanksDayPatternsByFirstDayThenBreadth()
    {
        var monToWed = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday);
        var monToTue = Sessions.FromDays(Days.Monday | Days.Tuesday);
        var tueToWed = Sessions.FromDays(Days.Tuesday | Days.Wednesday);
        Assert.IsTrue(monToWed.SortOrder < monToTue.SortOrder, "The broader Monday pattern should sort first.");
        Assert.IsTrue(monToTue.SortOrder < tueToWed.SortOrder, "A Monday start should sort before a Tuesday start.");
        var everyDay = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday | Days.Saturday | Days.Sunday);
        Assert.IsTrue(everyDay.SortOrder < monToWed.SortOrder, "Every day should sort first.");
    }

    [TestMethod]
    public void DefaultCatalogueHasTheSixStandardPatterns()
    {
        var catalogue = CommonSessionPatterns.DefaultCatalogue;
        Assert.HasCount(6, catalogue);
        Assert.AreEqual(14, catalogue[0].Numbers.Length, "First pattern is All.");
    }

    [TestMethod]
    public void EnsureSessionsSeedsTheCatalogueWhenEmpty()
    {
        var timetable = new Timetable("test", new Tellurian.Trains.Schedules.Model.Layouts.Layout());
        Assert.IsEmpty(timetable.Sessions);

        var added = timetable.EnsureSessions();

        Assert.IsTrue(added);
        Assert.HasCount(6, timetable.Sessions);
    }

    [TestMethod]
    public void EnsureSessionsIsNoOpWhenCatalogueIsNotEmpty()
    {
        var timetable = new Timetable("test", new Tellurian.Trains.Schedules.Model.Layouts.Layout());
        timetable.Sessions.Add(Sessions.FromSessionNumbers(1, 2, 3));

        var added = timetable.EnsureSessions();

        Assert.IsFalse(added);
        Assert.HasCount(1, timetable.Sessions);
    }
}
