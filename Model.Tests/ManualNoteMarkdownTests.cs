namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// The Markdown subset a planner may use in a manual note, exercised through the note itself rather
/// than the renderer behind it: what matters is what a report prints, in both forms.
/// </summary>
[TestClass]
public class ManualNoteMarkdownTests
{
    private const string LanguageCode = "en";

    private static ICallNote Note(string text) => new TextCallNote(text, LanguageCode);

    private static string Html(string text) => Note(text).ToHtml.Value;

    private static string Text(string text) => Note(text).ToText;

    [TestMethod]
    public void BoldRendersAsBElement()
    {
        Assert.AreEqual("""<span class="callnote">Take the <b>first</b> track</span>""", Html("Take the **first** track"));
    }

    [TestMethod]
    public void ItalicRendersAsIElement()
    {
        Assert.AreEqual("""<span class="callnote">Take the <i>first</i> track</span>""", Html("Take the *first* track"));
    }

    [TestMethod]
    public void EmphasisNests()
    {
        Assert.AreEqual("""<span class="callnote"><b>Always <i>first</i> track</b></span>""", Html("**Always *first* track**"));
    }

    [TestMethod]
    public void MarkersFlushAgainstEachOtherStayLiteral()
    {
        // A run of three cannot be resolved without a real Markdown parser's delimiter stack, so it is
        // left alone rather than guessed at. Documented in NoteMarkdown: separate the closers.
        Assert.AreEqual("""<span class="callnote">**Always *first***</span>""", Html("**Always *first***"));
    }

    [TestMethod]
    public void PlainTextFormDropsTheMarkers()
    {
        // The printed booklet estimates its page height from the plain form, and a marker occupies no
        // width on paper.
        Assert.AreEqual("Take the first track", Text("Take the **first** track"));
    }

    [TestMethod]
    public void UnmatchedMarkerStaysLiteralInBothForms()
    {
        Assert.AreEqual("Load 2*3 wagons", Text("Load 2*3 wagons"));
        Assert.AreEqual("""<span class="callnote">Load 2*3 wagons</span>""", Html("Load 2*3 wagons"));
    }

    [TestMethod]
    public void EscapedMarkerPrintsAsItself()
    {
        Assert.AreEqual("""<span class="callnote">Track 2* only</span>""", Html(@"Track 2\* only"));
        Assert.AreEqual("Track 2* only", Text(@"Track 2\* only"));
    }

    [TestMethod]
    public void UnderscoresAreNotEmphasis()
    {
        // They turn up inside identifiers often enough that treating them as markup would corrupt more
        // notes than it decorates.
        Assert.AreEqual("""<span class="callnote">Wagon type_a_b</span>""", Html("Wagon type_a_b"));
    }

    [TestMethod]
    public void FreeTextIsEncoded()
    {
        // A manual note is text the planner typed, not markup they authored.
        Assert.AreEqual("""<span class="callnote">Wagons &lt;b&gt; &amp; vans</span>""", Html("Wagons <b> & vans"));
    }

    [TestMethod]
    public void EncodingSurvivesInsideEmphasis()
    {
        Assert.AreEqual("""<span class="callnote"><b>Vans &amp; wagons</b></span>""", Html("**Vans & wagons**"));
    }

    [TestMethod]
    public void EmptyNoteRendersEmpty()
    {
        Assert.AreEqual(string.Empty, Text(string.Empty));
        Assert.AreEqual("""<span class="callnote"></span>""", Html(string.Empty));
    }

    [TestMethod]
    public void StoredTextKeepsItsMarkers()
    {
        // Text is what an editor binds to and what persistence keeps; only the rendered forms resolve
        // the emphasis.
        var note = new TextCallNote("Take the **first** track", LanguageCode);
        Assert.AreEqual("Take the **first** track", note.Text);
    }
}
