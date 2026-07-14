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

    [TestMethod]
    public void KeyFallsBackToVehicleIdWhenThereIsNeitherNumberNorExternalId()
    {
        var sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5);
        var first = new TurnusData { Id = 1, Number = 0, ExternalId = string.Empty, Sessions = sessions };
        var second = new TurnusData { Id = 2, Number = 0, ExternalId = string.Empty, Sessions = sessions };

        Assert.AreNotEqual(first.KeyWithSession, second.KeyWithSession,
            "Distinct vehicles with no number and no external id must not merge onto one card.");
    }
}
