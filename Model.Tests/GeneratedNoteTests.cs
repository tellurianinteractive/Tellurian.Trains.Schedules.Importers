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
        Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Use {0}.", Loco), note.Text);
    }

    [TestMethod]
    public void CoupleNoteWithPositionUsesTheArityTwoKey()
    {
        ICallNote note = new CoupleNote(Loco, 3);
        Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, "Couple {0} to train in position {1}.", Loco, 3), note.Text);
    }
}
