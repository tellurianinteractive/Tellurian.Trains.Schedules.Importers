namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Timetable stretches suggested from the order of the locations along the lines (<see cref="Layout.StretchLines"/>),
/// which the planner keeps from the hall plan, without its drawings (<c>ModelSurfaceAnalysis.md</c> 3.3).
/// </summary>
[TestClass]
public sealed class TimetableStretchSuggestionTests
{
    [TestMethod]
    public void TimetableStretchesAreSuggestedPerLineAndNotAdded()
    {
        var timetable = Timetable("Aby", "Bby", "Cby", "Dby");
        Stretch(timetable, "Aby", "Bby");
        Stretch(timetable, "Bby", "Cby");
        Stretch(timetable, "Bby", "Dby");
        Line(timetable, "Main", "Aby", "Bby", "Cby");
        Line(timetable, "Branch", "Bby", "Dby");
        timetable.Add(new TimetableStretch(4, "3"));

        var suggestions = timetable.SuggestedTimetableStretches();

        Assert.HasCount(1, timetable.TimetableStretches, "suggestions are not added");
        Assert.HasCount(2, suggestions);
        Assert.AreEqual(5, suggestions[0].Id);
        Assert.AreEqual("4", suggestions[0].Number);
        Assert.AreEqual("Main", suggestions[0].Description);
        CollectionAssert.AreEqual(new[] { "Aby", "Bby", "Cby" }, Names(suggestions[0]));
        Assert.AreEqual("5", suggestions[1].Number);
        CollectionAssert.AreEqual(new[] { "Bby", "Dby" }, Names(suggestions[1]));
    }

    [TestMethod]
    public void ALoopDividedAtAStationGivesTwoTimetableStretchesEndingThere()
    {
        var timetable = Timetable("Aby", "Bby", "Cby", "Dby");
        Stretch(timetable, "Aby", "Bby");
        Stretch(timetable, "Bby", "Cby");
        Stretch(timetable, "Dby", "Cby"); // turned by the planner, as is the next
        Stretch(timetable, "Aby", "Dby");
        Line(timetable, "Loop", "Aby", "Bby", "Cby", "Dby", "Aby");

        var suggestions = timetable.SuggestedTimetableStretches();

        Assert.HasCount(2, suggestions);
        CollectionAssert.AreEqual(new[] { "Aby", "Bby", "Cby" }, Names(suggestions[0]));
        CollectionAssert.AreEqual(new[] { "Aby", "Dby", "Cby" }, Names(suggestions[1]));
    }

    [TestMethod]
    public void ALineIsDividedWhereATrackStretchIsMissing()
    {
        var timetable = Timetable("Aby", "Bby", "Cby", "Dby");
        Stretch(timetable, "Aby", "Bby");
        Stretch(timetable, "Cby", "Dby");
        Line(timetable, "Main", "Aby", "Bby", "Cby", "Dby");

        var suggestions = timetable.SuggestedTimetableStretches();

        Assert.HasCount(2, suggestions);
        CollectionAssert.AreEqual(new[] { "Aby", "Bby" }, Names(suggestions[0]));
        CollectionAssert.AreEqual(new[] { "Cby", "Dby" }, Names(suggestions[1]));
    }

    [TestMethod]
    public void ALayoutWithoutLinesSuggestsNothing()
    {
        var timetable = Timetable("Aby", "Bby");
        Stretch(timetable, "Aby", "Bby");

        Assert.IsFalse(timetable.CanSuggestTimetableStretches);
        Assert.IsEmpty(timetable.SuggestedTimetableStretches());
    }

    [TestMethod]
    public void AnInsertedLocationSplitsTheLegAndARemovedOneJoinsThem()
    {
        var timetable = Timetable("Aby", "Bby", "Cby");
        Line(timetable, "Main", "Aby", "Cby");
        var layout = timetable.Layout;

        layout.InsertIntoStretchLines(At(timetable, "Cby"), At(timetable, "Aby"), At(timetable, "Bby"));
        CollectionAssert.AreEqual(new[] { "Aby–Bby", "Bby–Cby" }, Legs(timetable));

        layout.RemoveFromStretchLines(At(timetable, "Bby"));
        CollectionAssert.AreEqual(new[] { "Aby–Cby" }, Legs(timetable));
    }

    private static Timetable Timetable(params string[] names)
    {
        var layout = new Layout();
        var id = 0;
        foreach (var name in names) layout.Add(new Station(++id, name, name[..1]));
        return new Timetable("Test", layout);
    }

    private static OperationLocation At(Timetable timetable, string name) => timetable.Layout.OperationLocations.Single(l => l.Name == name);

    private static string NameOf(Timetable timetable, int id) => timetable.Layout.OperationLocations.Single(l => l.Id == id).Name;

    private static void Stretch(Timetable timetable, string start, string end)
    {
        var layout = timetable.Layout;
        layout.TrackStretches.Add(new TrackStretch(layout.TrackStretches.Count + 1, At(timetable, start), At(timetable, end), 10) { Layout = layout });
    }

    private static void Line(Timetable timetable, string name, params string[] locations) =>
        timetable.Layout.StretchLines.Add(new StretchLine
        {
            Id = timetable.Layout.StretchLines.Count + 1,
            Name = name,
            Legs = [.. locations.Zip(locations.Skip(1), (a, b) => new LineLeg(At(timetable, a).Id, At(timetable, b).Id))],
        });

    private static string[] Names(TimetableStretch stretch) => [.. stretch.Stations.Select(s => s.Name)];

    private static string[] Legs(Timetable timetable) =>
        [.. timetable.Layout.StretchLines.Single().Legs.Select(l => $"{NameOf(timetable, l.StartId)}–{NameOf(timetable, l.EndId)}")];
}
