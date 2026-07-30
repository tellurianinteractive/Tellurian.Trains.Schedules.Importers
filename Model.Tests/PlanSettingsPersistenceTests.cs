using System.Text.Json;
using System.Text.Json.Serialization;

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
}
