using System.Globalization;
using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Trains.Schedules.Planning.App.Translations.Resources;
using Tellurian.Trains.Schedules.Planning.Components.Reporting;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Guards the turnus-card identity used to de-duplicate/merge cards in <c>ToTurnusData</c>. Regression:
/// a vehicle assigned to two schedules (a daily working and a Mo-Fr working) must produce two cards — a
/// Mo-Fr card and a Sat-Sun card. When the turnus has no number the full identity dropped the sessions,
/// so the two combinations merged into one and the Sat-Sun card disappeared.
/// </summary>
[TestClass]
public class TurnusDataKeyTests
{
    private static TurnusData Card(Sessions sessions) =>
        new() { ExternalId = "BR 218 1", Number = 0, Sessions = sessions };

    [TestMethod]
    public void KeyWithSessionSeparatesSessionCombinationsOfANumberlessTurnus()
    {
        var weekdays = Card(Sessions.FromSessionNumbers(1, 2, 3, 4, 5));
        var weekend = Card(Sessions.FromSessionNumbers(6, 7));

        Assert.AreNotEqual(weekdays.KeyWithSession, weekend.KeyWithSession,
            "A numberless vehicle working two session sets must yield two distinct cards, not one merged card.");
    }

    [TestMethod]
    public void KeyIgnoresSessionsSoTheVariantsCountAsOneTurnusThatTurnsOver()
    {
        var weekdays = Card(Sessions.FromSessionNumbers(1, 2, 3, 4, 5));
        var weekend = Card(Sessions.FromSessionNumbers(6, 7));

        Assert.AreEqual(weekdays.Key, weekend.Key,
            "Both variants are the same turnus (same identity), so they are flagged as turning over.");
    }

    /// <summary>
    /// Regression: <c>Split('-', ',', options)</c> bound to <c>Split(char, int count, options)</c>, so a
    /// non-consecutive day set was never split on the comma and printed as raw resource keys.
    /// </summary>
    [TestMethod]
    [DataRow(Days.Monday | Days.Wednesday | Days.Friday, "Mo,We,Fr")]
    [DataRow(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday, "Mo-Fr")]
    [DataRow(Days.Saturday, "Sa")]
    public void OperatingPeriodTranslatesEveryDayName(Days days, string expected)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
        var localizer = new ResxStringLocalizer<Labels>();
        Translator translator = key => localizer[key ?? string.Empty].Value;
        var card = new TurnusData { Sessions = Sessions.FromDays(days), UseDays = true, MaxSessions = 7 };

        Assert.AreEqual(expected, card.OperatingPeriod(translator));
    }
}
