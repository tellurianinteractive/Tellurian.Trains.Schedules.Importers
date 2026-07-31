using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Reproduces the browser-storage round-trip used by Planning.App's ScheduleStateService to verify
/// that Layout.Settings (specifically the graphical timetable settings) survive serialize/deserialize.
/// </summary>
[TestClass]
public class PlanSettingsPersistenceTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    [TestMethod]
    public void GraphicTimetableSettingsSurvivePlanRoundTrip()
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        layout.Settings.GraphicTimetable.KilometerSpacing = 99;
        layout.Settings.GraphicTimetable.StationSpacing = 77;
        var timetable = new Timetable("TT", layout) { Id = 1 };
        var plan = Plan.Create("Plan", timetable);

        var json = JsonSerializer.Serialize(plan, Options);
        var restored = JsonSerializer.Deserialize<Plan>(json, Options);

        Assert.IsNotNull(restored);
        Assert.AreEqual(99, restored.Timetable.Layout.Settings.GraphicTimetable.KilometerSpacing);
        Assert.AreEqual(77, restored.Timetable.Layout.Settings.GraphicTimetable.StationSpacing);
    }

    [TestMethod]
    public void TextCallNoteKeepsItsTextThroughPolymorphicPreserveRoundTrip()
    {
        // The real question: does a note keep its Texts when serialized via its CallNote base
        // (polymorphic, $type) together with ReferenceHandler.Preserve ($id)? If this loses Texts,
        // that is how an in-memory note ends up empty after a restore.
        Notes.CallNote note = new Notes.TextCallNote("Hello", "en");

        var json = JsonSerializer.Serialize(note, Options);
        var restored = JsonSerializer.Deserialize<Notes.CallNote>(json, Options);

        Assert.IsNotNull(restored);
        Assert.AreEqual("Hello", restored.ToText);
    }

    [TestMethod]
    public void TextCallNoteWithNoTextsSerializesInsteadOfThrowing()
    {
        // A TextCallNote whose Texts list is empty must not throw when its Text is read during
        // serialization. Previously Text did Texts[0], throwing IndexOutOfRange, which failed the
        // whole-plan serialize — and the fire-and-forget save swallowed it, so nothing persisted.
        var note = JsonSerializer.Deserialize<Notes.TextCallNote>("{\"Texts\":[]}", Options);

        Assert.IsNotNull(note);
        Assert.AreEqual(string.Empty, note.ToText);
        Assert.IsNotNull(JsonSerializer.Serialize(note, Options));
    }

    // A plan under construction passes through half-finished shapes: a train whose calls are being
    // added or removed, a timetable stretch whose route is not put together yet. None of them may make
    // the plan unserializable, because the app's save is fire-and-forget: one throwing serialize and
    // nothing is written from then on, silently, until the browser is closed and the work is gone.

    [TestMethod]
    public void PlanWithTrainHavingOneCallStillSerializes()
    {
        var plan = PlanWithTrain(out var train);
        var last = train.CallsInRunOrder[^1];
        last.Track.Calls.Remove(last);
        train.Calls.Remove(last);

        Assert.IsNotNull(JsonSerializer.Serialize(plan, PlanJson.CreateOptions()));
    }

    [TestMethod]
    public void PlanWithTrainHavingNoCallsStillSerializes()
    {
        var plan = PlanWithTrain(out var train);
        foreach (var call in train.Calls.ToList()) call.Track.Calls.Remove(call);
        train.Calls.Clear();

        Assert.IsNotNull(JsonSerializer.Serialize(plan, PlanJson.CreateOptions()));
    }

    [TestMethod]
    public void PlanWithEmptyTimetableStretchStillSerializes()
    {
        var plan = PlanWithTrain(out _);
        plan.Layout.TimetableStretches.First().Stretches.Clear();

        Assert.IsNotNull(JsonSerializer.Serialize(plan, PlanJson.CreateOptions()));
    }

    [TestMethod]
    public void DerivedPropertiesAreNotWrittenToThePlanDocument()
    {
        // Derived state is computed from what is stored, so writing it only bloats the document and
        // exposes the save to anything those computations can throw. Deserialization ignores it either
        // way, since none of them has a setter.
        var plan = PlanWithTrain(out _);

        var json = JsonSerializer.Serialize(plan, PlanJson.CreateOptions());

        foreach (var derived in new[] { "\"Passings\"", "\"AsTrainPart\"", "\"DriverStartTime\"", "\"DriverEndTime\"", "\"Stations\"", "\"SortTime\"", "\"OperationLocation\"" })
            Assert.IsFalse(json.Contains(derived, StringComparison.Ordinal), $"{derived} should not be written to a saved plan.");
    }

    [TestMethod]
    public void PlanSavedByAnEarlierVersionStillLoads()
    {
        // Version 0.3.2 wrote the derived properties too, and ReferenceHandler.Preserve numbers objects
        // in the order they are written: an object first written inside a property that is now ignored
        // would take its $id there, and every later $ref to it would dangle on read. This document was
        // written by that version — reading it must still give back the whole plan.
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "Plan.0.3.2.json"));

        var plan = JsonSerializer.Deserialize<Plan>(json, PlanJson.CreateOptions());

        Assert.IsNotNull(plan);
        Assert.AreEqual(3, plan.Layout.OperationLocations.Count, "operation locations");
        Assert.AreEqual(2, plan.Layout.TrackStretches.Count, "track stretches");
        Assert.AreEqual(2, plan.Layout.TimetableStretches.First().Stretches.Count, "timetable stretch route");
        Assert.AreEqual(2, plan.Layout.DispatchStretches.Count, "dispatch stretches");
        var train = plan.Timetable.Trains.Single();
        Assert.AreEqual(3, train.Calls.Count, "calls");
        Assert.AreEqual("Vorsicht", train.CallsInRunOrder[0].ManualNoteText, "call note");
        Assert.IsTrue(plan.Layout.IsConnected(train.CallsInRunOrder[0].OperationLocation, train.CallsInRunOrder[1].OperationLocation));
        // That document wrote each call under its track as well as under its train; the per-track index
        // is now rebuilt on read instead, and must hold every call the trains run.
        Assert.AreEqual(3, plan.Layout.StationTracks().Sum(t => t.Calls.Count), "per-track call index");
    }

    [TestMethod]
    public void StationCallsAreWrittenOnceUnderTheirTrain()
    {
        // A call belongs to its train; StationTrack.Calls is an index into that, rebuilt on read. It was
        // written too, and being written first (the layout comes before the trains) it dragged the whole
        // graph — trains, categories, cargo flows — under the tracks, nested deeper at every step.
        var plan = PlanWithTrain(out _);

        var json = JsonSerializer.Serialize(plan, PlanJson.CreateOptions());
        var restored = JsonSerializer.Deserialize<Plan>(json, PlanJson.CreateOptions());

        Assert.AreEqual(plan.Timetable.Trains.Count, Regex.Matches(json, "\"Calls\"").Count, "only a train writes a Calls collection");
        Assert.IsNotNull(restored);
        Assert.AreEqual(2, restored.Timetable.Trains.Single().Calls.Count, "calls");
        Assert.AreEqual(2, restored.Layout.StationTracks().Sum(t => t.Calls.Count), "per-track call index");
    }

    // A minimal plan of the shape the app creates: two stations joined by a track stretch, one
    // timetable stretch over it and one train calling at both ends.
    private static Plan PlanWithTrain(out Train train)
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var from = AddStation(layout, 1, "Alpha", "A");
        var to = AddStation(layout, 2, "Beta", "B");
        var stretch = layout.Add(new TrackStretch(1, from, to, 10, 1, 100, 5));
        var timetableStretch = new TimetableStretch(1, "1", "Alpha-Beta");
        timetableStretch.AddLast(stretch);
        layout.Add(timetableStretch);

        var timetable = new Timetable("TT", layout) { Id = 1 };
        train = new Train(1, 101);
        timetable.Add(train);
        train.Add(new StationCall(1, from.Tracks.First(), Time.FromHourAndMinute(7, 50), Time.FromHourAndMinute(8, 0)) { IsDeparture = true });
        train.Add(new StationCall(2, to.Tracks.First(), Time.FromHourAndMinute(8, 20), Time.FromHourAndMinute(8, 30)) { IsArrival = true });
        return Plan.Create("Plan", timetable);
    }

    private static Station AddStation(Layout layout, int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10 + 1, "1", isMain: true, isScheduled: true));
        layout.Add(station);
        return station;
    }
}
