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
    public void OrderedDaysMondayFirstStartsOnMonday() => WithCulture("sv-SE", () =>
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var expected = new[] { Days.Monday, Days.Wednesday, Days.Friday, Days.Sunday };
        CollectionAssert.AreEqual(expected, target.OrderedDays);
    });

    [TestMethod]
    public void OrderedDaysSundayFirstStartsOnSunday() => WithCulture("en-US", () =>
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var expected = new[] { Days.Sunday, Days.Monday, Days.Wednesday, Days.Friday };
        CollectionAssert.AreEqual(expected, target.OrderedDays);
    });

    [TestMethod]
    public void DaysShortSundayFirstUsesEnglishShortNames() => WithCulture("en-US", () =>
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        Assert.AreEqual("Su, Mo, We, Fr", target.DaysShort);
    });

    [TestMethod]
    public void DaysFullMondayFirstUsesSwedishNamesInWeekOrder() => WithCulture("sv-SE", () =>
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        Assert.AreEqual("Tisdag, Torsdag, Lördag", target.DaysFull);
    });

    [TestMethod]
    public void DaysFullForAllSessionsIsTranslatedDaily() => WithCulture("sv-SE", () =>
    {
        Assert.AreEqual("Dagligen", Sessions.All.DaysFull);
    });

    [TestMethod]
    public void DaysFullForNoDaysIsTranslatedNone() => WithCulture("sv-SE", () =>
    {
        var target = Sessions.FromSessionNumbers();
        Assert.AreEqual("Inga kördagar", target.DaysFull);
    });

    [TestMethod]
    public void ConsecutiveDaysAreShownAsRange() => WithCulture("en-US", () =>
    {
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday);
        Assert.AreEqual("Mo-Fr", target.DaysShort);
        Assert.AreEqual("Monday to Friday", target.DaysFull);
    });

    [TestMethod]
    public void LongRangeUsesLocalisedConnector() => WithCulture("sv-SE", () =>
    {
        var target = Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday);
        Assert.AreEqual("Måndag till Fredag", target.DaysFull);
    });

    [TestMethod]
    public void DaysConsecutiveOnlyWhenWeekStartsOnSunday() => WithCulture("en-US", () =>
    {
        // Sunday-first: Su, Mo, Tu is an unbroken run.
        var target = Sessions.FromDays(Days.Sunday | Days.Monday | Days.Tuesday);
        Assert.AreEqual("Su-Tu", target.DaysShort);
    });

    [TestMethod]
    public void SameDaysAreNotConsecutiveWhenWeekStartsOnMonday() => WithCulture("sv-SE", () =>
    {
        // Monday-first: the same set is Mo, Tu, ... Su — broken, so listed not ranged.
        var target = Sessions.FromDays(Days.Sunday | Days.Monday | Days.Tuesday);
        Assert.AreEqual("M, Ti, S", target.DaysShort);
    });

    [TestMethod]
    public void ConsecutiveSessionsAreShownAsRange()
    {
        var target = Sessions.FromSessionNumbers(1, 2, 3, 4, 5);
        Assert.AreEqual("1-5", target.SessionsNumbers);
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
        var actual = target.Days;
        Assert.HasCount(7, actual);
        Assert.AreEqual(Days.Monday, actual[0], "First day in week is Monday");
        Assert.AreEqual(Days.Sunday, actual[^1], "Last day in week is Sunday");
    }

    [TestMethod]
    public void MondayWednesdayFridaySunday()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var actual = target.Days;
        Assert.HasCount(4, actual);
        Assert.AreEqual(Days.Monday, actual[0], "First day is Monday");
        Assert.AreEqual(Days.Sunday, actual[^1], "Last day is Sunday");
    }

    [TestMethod]
    public void TuesdayThursdaySaturday()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var actual = target.Days;
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
        var actual = target.FullDayNamesResourceKey;
        Assert.AreEqual("Tuesday-Thursday", actual);
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
