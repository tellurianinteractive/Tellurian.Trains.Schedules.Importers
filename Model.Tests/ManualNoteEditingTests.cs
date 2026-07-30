using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Writing a call's manual note — what the note field in the Trains tab does through the model.
/// </summary>
[TestClass]
public class ManualNoteEditingTests
{
    private const string English = "en";
    private const string Swedish = "sv";

    private static StationCall Call() =>
        new(1, new StationTrack(1, "1") { Station = new Station(1, "Falun", "Fln") }, Time.Zero, Time.Zero);

    [TestInitialize]
    public void UseEnglish() => CultureInfo.CurrentCulture = new CultureInfo(English);

    [TestMethod]
    public void CallWithoutNotesHasNoManualNote()
    {
        var call = Call();
        Assert.IsNull(call.ManualNote);
        Assert.AreEqual(string.Empty, call.ManualNoteText);
    }

    [TestMethod]
    public void FirstTextCreatesTheNote()
    {
        var call = Call();
        call.SetManualNote("Take the **first** track");

        Assert.AreEqual("Take the **first** track", call.ManualNoteText);
        Assert.AreEqual(1, call.Notes.Count);
    }

    [TestMethod]
    public void CreatedNoteIsForEveryAudience()
    {
        // A single field asks for no audience flags, and a note nobody is shown would silently not
        // reach the reports it was typed for.
        var call = Call();
        call.SetManualNote("Wait for the branch train");

        var note = call.ManualNote!;
        Assert.IsTrue(note.IsDriverNote);
        Assert.IsTrue(note.IsStationNote);
        Assert.IsTrue(note.IsShuntingNote);
    }

    [TestMethod]
    public void SecondEditReplacesRatherThanAdds()
    {
        var call = Call();
        call.SetManualNote("First");
        call.SetManualNote("Second");

        Assert.AreEqual("Second", call.ManualNoteText);
        Assert.AreEqual(1, call.Notes.Count);
    }

    [TestMethod]
    public void ClearingTheTextRemovesTheNote()
    {
        // An emptied field must leave nothing behind: a blank note would still claim a row in a
        // printed booklet.
        var call = Call();
        call.SetManualNote("Something");
        call.SetManualNote("   ");

        Assert.IsNull(call.ManualNote);
        Assert.AreEqual(0, call.Notes.Count);
    }

    [TestMethod]
    public void EditingInAnotherLanguageKeepsTheFirstTranslation()
    {
        var call = Call();
        call.SetManualNote("Track one", English);
        call.SetManualNote("Spår ett", Swedish);

        Assert.AreEqual(1, call.Notes.Count);
        Assert.AreEqual("Track one", call.ManualNoteText);

        CultureInfo.CurrentCulture = new CultureInfo(Swedish);
        Assert.AreEqual("Spår ett", call.ManualNoteText);
    }

    [TestMethod]
    public void EditingReplacesTextThatHasNoLanguage()
    {
        // The XPLN import stores a call remark with no language code. That is not a translation, so an
        // edit replaces it instead of leaving it to resurface for a reader in another language.
        var call = new StationCall(1, new StationTrack(1, "1") { Station = new Station(1, "Falun", "Fln") },
            Time.Zero, Time.Zero, "Imported remark");
        Assert.AreEqual("Imported remark", call.ManualNoteText);

        call.SetManualNote("Edited", English);

        Assert.AreEqual(1, call.Notes.Count);
        Assert.AreEqual("Edited", call.ManualNoteText);
        CultureInfo.CurrentCulture = new CultureInfo(Swedish);
        Assert.AreEqual("Edited", call.ManualNoteText);
    }

    [TestMethod]
    public void EditedTextRendersItsEmphasis()
    {
        var call = Call();
        call.SetManualNote("Take the **first** track");

        Assert.AreEqual("""<span class="callnote">Take the <b>first</b> track</span>""", call.ManualNote!.ToHtml.Value);
    }
}
