using Tellurian.Trains.Schedules.Planning.Components.Reporting.Instructions;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the language tags an author may write in the programme and the general instructions, and the
/// text each language's booklet is printed with.
/// </summary>
[TestClass]
public class InstructionsLanguagesTests
{
    private static readonly string[] Supported = ["en", "de", "da", "nb", "sv"];
    private static bool IsAvailable(string language) => Supported.Contains(language);

    [TestMethod]
    public void TextWithoutTagsIsPrintedOnceInTheDefaultLanguage()
    {
        CollectionAssert.AreEqual(new[] { "sv" }, InstructionsLanguages.BookletLanguages("sv", IsAvailable, "Plain text").ToArray());
        Assert.AreEqual("Plain text", InstructionsLanguages.Resolve("Plain text", "sv", "sv"));
    }

    [TestMethod]
    public void EveryTaggedLanguageGetsABookletDefaultFirst()
    {
        var languages = InstructionsLanguages.BookletLanguages("sv", IsAvailable, "[nb]Norsk[/nb][sv]Svensk[/sv]", "[DA]Dansk[/DA]");
        CollectionAssert.AreEqual(new[] { "sv", "nb", "da" }, languages.ToArray());
    }

    [TestMethod]
    public void ALanguageTheAppCannotPrintGetsNoBooklet()
    {
        var languages = InstructionsLanguages.BookletLanguages("sv", IsAvailable, "[sv]Svensk[/sv][nl]Nederlands[/nl]");
        CollectionAssert.AreEqual(new[] { "sv" }, languages.ToArray());
    }

    [TestMethod]
    public void EachLanguageGetsItsOwnVersionAndTheCommonText()
    {
        const string text = "Common [sv]Svensk text[/sv][nb]Norsk tekst[/nb] end.";
        Assert.AreEqual("Common Svensk text end.", InstructionsLanguages.Resolve(text, "sv", "sv"));
        Assert.AreEqual("Common Norsk tekst end.", InstructionsLanguages.Resolve(text, "nb", "sv"));
    }

    [TestMethod]
    public void AMissingVersionFallsBackToTheDefaultLanguage()
    {
        const string text = "[sv]Svensk[/sv][nb]Norsk[/nb]";
        Assert.AreEqual("Svensk", InstructionsLanguages.Resolve(text, "da", "sv"));
    }

    [TestMethod]
    public void APassageWithNeitherVersionIsLeftOut()
    {
        const string text = "Before\n\n[nb]Bare for nordmenn[/nb]\n\nAfter";
        Assert.AreEqual("Before\n\n\n\nAfter", InstructionsLanguages.Resolve(text, "da", "sv"));
    }

    [TestMethod]
    public void VersionsSeparatedByTextAreSeparatePassages()
    {
        const string text = "[sv]Ett[/sv][nb]En[/nb] and [sv]Två[/sv][nb]To[/nb]";
        Assert.AreEqual("En and To", InstructionsLanguages.Resolve(text, "nb", "sv"));
        Assert.AreEqual("Ett and Två", InstructionsLanguages.Resolve(text, "de", "sv"));
    }

    [TestMethod]
    public void AVersionMaySpanLinesAndHoldMarkdown()
    {
        const string text = "Intro\n\n[sv]\n## Rubrik\n\nText\n[/sv]\n[nb]\n## Overskrift\n\nTekst\n[/nb]\n\nOutro";
        Assert.AreEqual("Intro\n\n## Overskrift\n\nTekst\n\nOutro", InstructionsLanguages.Resolve(text, "nb", "sv"));
    }

    [TestMethod]
    public void APlaceholderInsideAVersionIsPlacedOnlyInThatLanguage()
    {
        const string text = "[sv]\n<ShuntingYards/>\n[/sv][nb]Ingen tabell[/nb]";
        Assert.IsTrue(InstructionsPlaceholders.Places(InstructionsLanguages.Resolve(text, "sv", "sv"), InstructionsPlaceholder.ShuntingYards));
        Assert.IsFalse(InstructionsPlaceholders.Places(InstructionsLanguages.Resolve(text, "nb", "sv"), InstructionsPlaceholder.ShuntingYards));
    }

    [TestMethod]
    public void AnUnclosedOrMismatchedTagIsLeftAsWritten()
    {
        const string text = "[sv]Svensk[/nb] and [x] box";
        Assert.AreEqual(text, InstructionsLanguages.Resolve(text, "sv", "sv"));
    }

    [TestMethod]
    public void ThePreviewLabelsEveryVersion()
    {
        var preview = InstructionsLanguages.ToPreviewMarkdown("[sv]Svensk[/sv][nb]Norsk[/nb]");
        StringAssert.Contains(preview, "<span class=\"report-language\">SV</span> Svensk");
        StringAssert.Contains(preview, "<span class=\"report-language\">NB</span> Norsk");
    }
}
