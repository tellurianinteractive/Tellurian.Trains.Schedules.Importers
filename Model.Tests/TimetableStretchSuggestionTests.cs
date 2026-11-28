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
        var layout = Layout("Aby", "Bby", "Cby", "Dby");
        Stretch(layout, "Aby", "Bby");
        Stretch(layout, "Bby", "Cby");
        Stretch(layout, "Bby", "Dby");
        Line(layout, "Main", "Aby", "Bby", "Cby");
        Line(layout, "Branch", "Bby", "Dby");
        layout.TimetableStretches.Add(new TimetableStretch(4, "3"));

        var suggestions = layout.SuggestedTimetableStretches();

        Assert.HasCount(1, layout.TimetableStretches, "suggestions are not added");
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
        var layout = Layout("Aby", "Bby", "Cby", "Dby");
        Stretch(layout, "Aby", "Bby");
        Stretch(layout, "Bby", "Cby");
        Stretch(layout, "Dby", "Cby"); // turned by the planner, as is the next
        Stretch(layout, "Aby", "Dby");
        Line(layout, "Loop", "Aby", "Bby", "Cby", "Dby", "Aby");

        var suggestions = layout.SuggestedTimetableStretches();

        Assert.HasCount(2, suggestions);
        CollectionAssert.AreEqual(new[] { "Aby", "Bby", "Cby" }, Names(suggestions[0]));
        CollectionAssert.AreEqual(new[] { "Aby", "Dby", "Cby" }, Names(suggestions[1]));
    }

    [TestMethod]
    public void ALineIsDividedWhereATrackStretchIsMissing()
    {
        var layout = Layout("Aby", "Bby", "Cby", "Dby");
        Stretch(layout, "Aby", "Bby");
        Stretch(layout, "Cby", "Dby");
        Line(layout, "Main", "Aby", "Bby", "Cby", "Dby");

        var suggestions = layout.SuggestedTimetableStretches();

        Assert.HasCount(2, suggestions);
        CollectionAssert.AreEqual(new[] { "Aby", "Bby" }, Names(suggestions[0]));
        CollectionAssert.AreEqual(new[] { "Cby", "Dby" }, Names(suggestions[1]));
    }

    [TestMethod]
    public void ALayoutWithoutLinesSuggestsNothing()
    {
        var layout = Layout("Aby", "Bby");
        Stretch(layout, "Aby", "Bby");

        Assert.IsFalse(layout.CanSuggestTimetableStretches);
        Assert.IsEmpty(layout.SuggestedTimetableStretches());
    }

    [TestMethod]
    public void AnInsertedLocationSplitsTheLegAndARemovedOneJoinsThem()
    {
        var layout = Layout("Aby", "Bby", "Cby");
        Line(layout, "Main", "Aby", "Cby");

        layout.InsertIntoStretchLines(At(layout, "Cby"), At(layout, "Aby"), At(layout, "Bby"));
        CollectionAssert.AreEqual(new[] { "Aby–Bby", "Bby–Cby" }, Legs(layout));

        layout.RemoveFromStretchLines(At(layout, "Bby"));
        CollectionAssert.AreEqual(new[] { "Aby–Cby" }, Legs(layout));
    }

    private static Layout Layout(params string[] names)
    {
        var layout = new Layout();
        var id = 0;
        foreach (var name in names) layout.Add(new Station(++id, name, name[..1]));
        return layout;
    }

    private static OperationLocation At(Layout layout, string name) => layout.OperationLocations.Single(l => l.Name == name);

    private static void Stretch(Layout layout, string start, string end) =>
        layout.TrackStretches.Add(new TrackStretch(layout.TrackStretches.Count + 1, At(layout, start), At(layout, end), 10) { Layout = layout });

    private static void Line(Layout layout, string name, params string[] locations) =>
        layout.StretchLines.Add(new StretchLine
        {
            Id = layout.StretchLines.Count + 1,
            Name = name,
            Legs = [.. locations.Zip(locations.Skip(1), (a, b) => new LineLeg(At(layout, a).Id, At(layout, b).Id))],
        });

    private static string[] Names(TimetableStretch stretch) => [.. stretch.Stations.Select(s => s.Name)];

    private static string[] Legs(Layout layout) =>
        [.. layout.StretchLines.Single().Legs.Select(l => $"{layout.OperationLocations.Single(o => o.Id == l.StartId).Name}–{layout.OperationLocations.Single(o => o.Id == l.EndId).Name}")];
}
