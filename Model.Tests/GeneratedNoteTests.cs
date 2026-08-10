using System.Globalization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class GeneratedNoteTests
{
    private static ScheduledObject Loco => new(0, ScheduledObjectType.Locomotive, 42) { Class = "T44" };

    // Pin both cultures to invariant so the asserts resolve the neutral (English) Notes resource (UI culture)
    // and format the arguments deterministically (formatting culture), independent of the host machine's
    // culture and of the localised Notes.<culture>.resx files.
    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [TestMethod]
    public void UseNoteResolvesLocalisedTextFromNotesResource()
    {
        // Confirms the Notes.resx is embedded and resolves (not a silent fall-back to the key):
        // "Use {0}." formatted with the vehicle, rather than the bare key "Use".
        ICallNote note = new UseNote(Loco);
        Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Use {0}.", Loco), note.ToText);
    }

    [TestMethod]
    public void CoupleNoteWithPositionUsesTheArityTwoKey()
    {
        ICallNote note = new CoupleNote(Loco, 3);
        Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Couple {0} to train in position {1}.", Loco, 3), note.ToText);
    }

    [TestMethod]
    public void SubstitutedValueIsEmphasisedInTheMarkupForm()
    {
        // The whole point of the markup form: the vehicle the driver must fetch stands out from the
        // sentence around it, and the sentence itself carries no emphasis.
        ICallNote note = new UseNote(Loco);
        Assert.AreEqual($"""<span class="callnote">Use <b class="value">{Loco}</b>.</span>""", note.ToHtml.Value);
    }

    [TestMethod]
    public void PositionInTrainIsNotEmphasised()
    {
        // A position stands beside the vehicle that already carries the emphasis; bolding both would
        // leave the reader nothing to land on.
        ICallNote note = new CoupleNote(Loco, 3);
        Assert.AreEqual($"""<span class="callnote">Couple <b class="value">{Loco}</b> to train in position 3.</span>""", note.ToHtml.Value);
    }

    [TestMethod]
    public void NoteWithoutValuesGetsNoEmphasis()
    {
        ICallNote note = new NoStopNote();
        Assert.AreEqual("""<span class="callnote">No stop</span>""", note.ToHtml.Value);
    }

    [TestMethod]
    public void SubstitutedValueIsHtmlEncoded()
    {
        // A vehicle class containing an ampersand used to be written into the markup raw, which
        // produced a broken note rather than a wrong one.
        var loco = new ScheduledObject(0, ScheduledObjectType.Locomotive, 42) { Class = "T44 & Co" };
        ICallNote note = new UseNote(loco);
        StringAssert.Contains(note.ToHtml.Value, "T44 &amp; Co");
        Assert.IsFalse(note.ToHtml.Value.Contains("T44 & Co", StringComparison.Ordinal));
    }

    [TestMethod]
    public void PlainTextFormCarriesNoMarkup()
    {
        ICallNote note = new UseNote(Loco);
        Assert.IsFalse(note.ToText.Contains('<', StringComparison.Ordinal));
    }

    [TestMethod]
    public void CirculatingAndTurningEachSayWhichMoveToMake()
    {
        // No vehicle named and no explanation of the manoeuvre: the note is on an arrival, the loco is
        // the one in front of the driver, and circulating is something every loco driver can already do.
        Assert.AreEqual("Circulate locomotive.", new CirculateNote().ToText);
        Assert.AreEqual("Turn locomotive.", new TurnNote().ToText);
        Assert.AreEqual("Turn and circulate locomotive.", new TurnAndCirculateNote().ToText);
    }

    [TestMethod]
    public void OvertakingNotesSayWhichTrainPassesTheOther()
    {
        // The two readings of one event, and the whole reason they are separate note types: a driver
        // cannot act on "meets in the same direction", but can on who is getting past whom.
        var other = OtherTrain;
        var meet = new Meet(other, Time.FromHourAndMinute(12, 02), Time.FromHourAndMinute(12, 05), null);

        Assert.AreEqual($"Overtakes {other} 12:02-12:05", new OvertakesNote([meet], null).ToText);
        Assert.AreEqual($"Is overtaken by {other} 12:02-12:05", new IsOvertakenNote([meet], null).ToText);
    }

    [TestMethod]
    public void AMeetLastingNoTimeIsShownAsASingleTime()
    {
        // A train running through is there for an instant; "12:02-12:02" states an interval that does
        // not exist.
        var other = OtherTrain;
        var meet = new Meet(other, Time.FromHourAndMinute(12, 02), Time.FromHourAndMinute(12, 02), null);

        Assert.AreEqual($"Is overtaken by {other} 12:02", new IsOvertakenNote([meet], null).ToText);
    }

    private static Train OtherTrain =>
        new(1, new TrainCategory { Id = 1, Name = "Freight", Prefix = "GD" }, 42757);
}
