namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>Joining the parts of a name without a word said twice, as Module Draw titles an end profile.</summary>
[TestClass]
public class WordingTests
{
    [TestMethod]
    public void PartsAreJoinedBySpaces()
    {
        Assert.AreEqual("FREMO H0 B96", Wording.JoinWithoutRepeatedWords("FREMO", "H0", "B96"));
        Assert.AreEqual("H0 B96", Wording.JoinWithoutRepeatedWords(null, "H0", " ", "B96"), "Empty parts are left out.");
        Assert.AreEqual(string.Empty, Wording.JoinWithoutRepeatedWords(null, ""));
    }

    [TestMethod]
    public void WordsAlreadySaidAreLeftOut()
    {
        Assert.AreEqual("FREMO/H0-RE B96L", Wording.JoinWithoutRepeatedWords("FREMO/H0-RE", "H0", "FREMO H0 B96L"));
        Assert.AreEqual("FREMO/H0-RE B96", Wording.JoinWithoutRepeatedWords("FREMO/H0-RE", "H0", "FREMO-H0-B96"), "Also within a hyphenated part.");
        Assert.AreEqual("FREMO/H0-RE B96", Wording.JoinWithoutRepeatedWords("FREMO/H0-RE", "H0", "fremo h0 B96"), "Ignoring case.");
        Assert.AreEqual("FREMO/H0-RE B96-L", Wording.JoinWithoutRepeatedWords("FREMO/H0-RE", "H0", "H0 B96-L"), "Kept words keep their separator.");
        Assert.AreEqual("H0 FREMO B96", Wording.JoinWithoutRepeatedWords(null, "H0", "FREMO H0 B96"), "The first part with text is kept as it is.");
        Assert.AreEqual("FREMO/H0-RE", Wording.JoinWithoutRepeatedWords("FREMO/H0-RE", "H0"), "A part with nothing left is left out.");
    }
}
