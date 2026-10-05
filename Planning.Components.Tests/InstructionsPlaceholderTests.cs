using Tellurian.Trains.Schedules.Planning.Components.Reporting.Instructions;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the placeholder tags an author may write in the general instructions, and what the booklet's
/// pagination charges for them.
/// </summary>
/// <remarks>
/// The point of a placeholder is that the shunting yards table need not sit on the layout page at the
/// back, where it shares a page with the topology diagram and the cargo key and pushes them past the
/// foot of it on any layout with more than a handful of yards. The page body is <c>overflow: hidden</c>,
/// so what does not fit is dropped without a word — which is why the estimate is tested here as well as
/// the recognition.
/// </remarks>
[TestClass]
public class InstructionsPlaceholderTests
{
    private const string Tag = "<ShuntingYards/>";
    private const string InterchangeTag = "<Interchanges/>";

    // Station names of a realistic length, since it is their joined length that decides how many lines
    // the one paragraph they are set in wraps to.
    private static IReadOnlyList<Station> Interchanges(int count) =>
        [.. Enumerable.Range(1, count).Select(i => new Station(i, $"Interchange station {i}", $"I{i}"))];

    private static IReadOnlyList<ShuntingYard> Yards(int count, int locationsEach = 1)
    {
        var id = 0;
        return
        [
            .. Enumerable.Range(1, count).Select(i => new ShuntingYard(
                new Station(++id, $"Yard {i}", $"Y{i}"),
                // Station names of a realistic length, since it is their joined length that decides how
                // many lines the served-locations column wraps to.
                [.. Enumerable.Range(1, locationsEach)
                    .Select(j => (OperationLocation)new Station(++id, $"Station {i}-{j}", $"S{i}{j}"))],
                []))
        ];
    }

    [TestMethod]
    public void ATagOnALineOfItsOwnIsRecognised()
    {
        Assert.AreEqual(InstructionsPlaceholder.ShuntingYards, InstructionsPlaceholders.PlaceholderOf(Tag));
        Assert.AreEqual(InstructionsPlaceholder.ShuntingYards, InstructionsPlaceholders.PlaceholderOf("  <ShuntingYards />  "));
        // Written as an author types it, not as a compiler would: casing is not the author's problem.
        Assert.AreEqual(InstructionsPlaceholder.ShuntingYards, InstructionsPlaceholders.PlaceholderOf("<shuntingyards/>"));
    }

    [TestMethod]
    public void OrdinaryTextIsNotATag()
    {
        foreach (var line in new[] { "", "Shunting yards", "<ShuntingYards/> and then some", "<Topology/>", "<ShuntingYards>" })
            Assert.IsNull(InstructionsPlaceholders.PlaceholderOf(line), $"'{line}' was taken for a tag.");
    }

    [TestMethod]
    public void TheTagIsWrittenTheWayItIsRecognised()
    {
        // What the settings page shows the author. The two must not drift apart.
        Assert.AreEqual(
            InstructionsPlaceholder.ShuntingYards,
            InstructionsPlaceholders.PlaceholderOf(InstructionsPlaceholders.TagOf(InstructionsPlaceholder.ShuntingYards)));
    }

    [TestMethod]
    public void TextWithNoTagIsOneSegment()
    {
        var segments = InstructionsPlaceholders.Split("# Signalling\n\nAlways stop at a red signal.");

        Assert.HasCount(1, segments);
        Assert.IsNull(segments[0].Placeholder);
        Assert.Contains("Always stop", segments[0].Markdown);
    }

    [TestMethod]
    public void TheTextIsSplitAroundTheTagInTheOrderItWasWritten()
    {
        var segments = InstructionsPlaceholders.Split($"Before the table.\n\n{Tag}\n\nAfter the table.");

        Assert.HasCount(3, segments);
        Assert.AreEqual("Before the table.", segments[0].Markdown);
        Assert.AreEqual(InstructionsPlaceholder.ShuntingYards, segments[1].Placeholder);
        Assert.AreEqual("After the table.", segments[2].Markdown);
    }

    [TestMethod]
    public void EachStretchEitherSideIsMarkdownInItsOwnRight()
    {
        // Which is the whole reason the source is split rather than the rendered HTML: a cut made after
        // conversion could fall inside a paragraph the author started on the line above.
        var segments = InstructionsPlaceholders.Split($"# Where the wagons go\n\n{Tag}\n\nAnd so on.");

        var rendered = segments.Where(s => s.Placeholder is null)
            .Select(s => Markdig.Markdown.ToHtml(s.Markdown)).ToList();

        Assert.Contains("<h1>Where the wagons go</h1>", rendered[0]);
        Assert.Contains("<p>And so on.</p>", rendered[1]);
        Assert.IsFalse(rendered.Any(html => html.Contains("ShuntingYards", StringComparison.OrdinalIgnoreCase)),
            "The tag must never reach the rendered markdown, where it would show as an element the browser drops.");
    }

    [TestMethod]
    public void ATagAloneIsTheWholeContent()
    {
        var segments = InstructionsPlaceholders.Split(Tag);

        Assert.HasCount(1, segments);
        Assert.AreEqual(InstructionsPlaceholder.ShuntingYards, segments[0].Placeholder);
    }

    [TestMethod]
    public void TheLayoutPageAsksWhetherTheAuthorPlacedTheTableThemselves()
    {
        Assert.IsTrue(InstructionsPlaceholders.Places($"Text.\n\n{Tag}", InstructionsPlaceholder.ShuntingYards));
        Assert.IsFalse(InstructionsPlaceholders.Places("Text only.", InstructionsPlaceholder.ShuntingYards));
        Assert.IsFalse(InstructionsPlaceholders.Places(null, InstructionsPlaceholder.ShuntingYards));
    }

    [TestMethod]
    public void ThePreviewShowsANamedBoxWhereTheTagStands()
    {
        // Rendered as it stands, the tag is an unknown element the browser drops, so the author would
        // see it vanish and could not tell a recognised tag from a misspelt one.
        var preview = InstructionsPlaceholders.ToPreviewMarkdown($"Before.\n{Tag}\nAfter.", _ => "Shunting yards");

        Assert.Contains("<p class=\"report-placeholder\">Shunting yards</p>", preview);
        Assert.Contains("Before.", preview);
        Assert.Contains("After.", preview);
    }

    [TestMethod]
    public void APlaceholderIsChargedTheHeightOfTheTableAndNotOfItsLine()
    {
        var yards = Yards(6);

        Assert.IsTrue(InstructionsPagination.ShuntingYardsHeight(yards) > 8,
            "A table of six yards costs far more than the one line its tag occupies.");
    }

    [TestMethod]
    public void ATableTooTallForWhatIsLeftOfThePageMovesToTheNext()
    {
        // Text filling most of a page, then the table. Charged as one line the table would follow the
        // text onto the same page and print past the foot of it, losing its last rows unannounced.
        var text = string.Join("\n\n", Enumerable.Range(1, 20).Select(i => $"Paragraph {i}."));
        var yards = Yards(10);

        var pages = InstructionsPagination.BuildPages($"{text}\n\n{Tag}", includeOverview: false, shuntingYards: yards);
        var content = pages.Where(p => p.Kind == InstructionsPageKind.Content).ToList();

        Assert.HasCount(2, content);
        Assert.Contains(Tag, content[1].Markdown);
    }

    [TestMethod]
    public void AYardWorkingManyLocationsIsChargedTheLinesItsListWrapsTo()
    {
        // The served locations are a list of names in one column, so the same number of yards can cost
        // twice as much on a layout where each yard works half of it.
        var one = InstructionsPagination.ShuntingYardsHeight(Yards(4, locationsEach: 1));
        var many = InstructionsPagination.ShuntingYardsHeight(Yards(4, locationsEach: 6));

        Assert.IsTrue(many > one, $"A wrapping list must cost more than a short one ({many} against {one}).");
    }

    [TestMethod]
    public void ALayoutWithNoShuntingYardsPrintsNoTableAndIsChargedNothing()
    {
        Assert.AreEqual(0, InstructionsPagination.ShuntingYardsHeight([]));
        Assert.AreEqual(0, InstructionsPagination.ShuntingYardsHeight(null));
    }

    [TestMethod]
    public void TheInterchangesTagIsRecognisedAndWrittenTheWayItIsRead()
    {
        Assert.AreEqual(InstructionsPlaceholder.Interchanges, InstructionsPlaceholders.PlaceholderOf(InterchangeTag));
        Assert.AreEqual(InstructionsPlaceholder.Interchanges, InstructionsPlaceholders.PlaceholderOf("  <interchanges />  "));
        Assert.AreEqual(
            InstructionsPlaceholder.Interchanges,
            InstructionsPlaceholders.PlaceholderOf(InstructionsPlaceholders.TagOf(InstructionsPlaceholder.Interchanges)));
    }

    [TestMethod]
    public void TheTwoTagsAreToldApart()
    {
        // One text may place both, and each must render the part it was written for.
        var segments = InstructionsPlaceholders.Split($"{Tag}\n\n{InterchangeTag}");

        Assert.HasCount(2, segments);
        Assert.AreEqual(InstructionsPlaceholder.ShuntingYards, segments[0].Placeholder);
        Assert.AreEqual(InstructionsPlaceholder.Interchanges, segments[1].Placeholder);
        Assert.IsTrue(InstructionsPlaceholders.Places(InterchangeTag, InstructionsPlaceholder.Interchanges));
        Assert.IsFalse(InstructionsPlaceholders.Places(Tag, InstructionsPlaceholder.Interchanges));
    }

    [TestMethod]
    public void AnInterchangesPlaceholderIsChargedTheHeightOfTheListAndNotOfItsLine()
    {
        // Heading, the paragraph of names, and the gap below it: more than the one line the tag occupies.
        Assert.IsTrue(InstructionsPagination.InterchangesHeight(Interchanges(3)) > 2);
    }

    [TestMethod]
    public void ALongerListOfNamesWrapsAndCostsMore()
    {
        var few = InstructionsPagination.InterchangesHeight(Interchanges(2));
        var many = InstructionsPagination.InterchangesHeight(Interchanges(12));

        Assert.IsTrue(many > few, $"A wrapping paragraph must cost more than a short one ({many} against {few}).");
    }

    [TestMethod]
    public void ALayoutWithNoInterchangesPrintsNoListAndIsChargedNothing()
    {
        // Which is also what a meeting not worked with passenger tickets has, so the tag costs it nothing.
        Assert.AreEqual(0, InstructionsPagination.InterchangesHeight([]));
        Assert.AreEqual(0, InstructionsPagination.InterchangesHeight(null));
    }

    [TestMethod]
    public void AListTooTallForWhatIsLeftOfThePageMovesToTheNext()
    {
        // Text filling most of a page, then the list. Charged as one line it would follow the text onto
        // the same page and print past the foot of it, losing its last names unannounced.
        var text = string.Join("\n\n", Enumerable.Range(1, 31).Select(i => $"Paragraph {i}."));
        var interchanges = Interchanges(12);

        var pages = InstructionsPagination.BuildPages(
            $"{text}\n\n{InterchangeTag}", includeOverview: false, passengerInterchanges: interchanges);
        var content = pages.Where(p => p.Kind == InstructionsPageKind.Content).ToList();

        Assert.HasCount(2, content);
        Assert.Contains(InterchangeTag, content[1].Markdown);
    }
}
