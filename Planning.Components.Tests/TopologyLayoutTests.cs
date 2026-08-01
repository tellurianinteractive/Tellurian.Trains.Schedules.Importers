using System.Globalization;
using Tellurian.Trains.Schedules.Planning.Components.Shared;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies how the topology diagram stacks timetable stretches: lines that do not overlap horizontally
/// share a row so the diagram stays as low as possible, lines that do overlap are pushed apart, and every
/// branch is linked to its parent by a 45° connector that continues a short way along the branch's own line.
/// </summary>
[TestClass]
public class TopologyLayoutTests
{
    private static readonly string[] Colors = ["#000000", "#1a3a5c", "#8B0000", "#006400", "#4B0082", "#8B4513"];

    // Builds a layout from one line per spec, each spec a space-separated run of station signatures.
    // Stations of the same signature are shared between lines, which is what makes one line a branch of another.
    // A signature may carry the distance from the station before it as "Sig@2"; it is 10 when left out.
    private static Layout CreateLayout(params string[] specs)
    {
        var layout = new Layout { Name = "Test" };
        var stations = new Dictionary<string, OperationLocation>();
        var stationId = 0;
        var trackId = 0;
        var lineId = 0;

        OperationLocation Station(string signature)
        {
            if (!stations.TryGetValue(signature, out var station))
            {
                station = layout.Add(new Station(++stationId, signature, signature));
                stations.Add(signature, station);
            }
            return station;
        }

        foreach (var spec in specs)
        {
            var tokens = spec.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Split('@'))
                .Select(t => (Signature: t[0], Distance: t.Length > 1 ? double.Parse(t[1], CultureInfo.InvariantCulture) : 10.0))
                .ToList();
            lineId++;
            var line = new TimetableStretch(lineId, lineId.ToString(CultureInfo.InvariantCulture))
            {
                Color = Colors[lineId % Colors.Length],
            };
            for (var i = 1; i < tokens.Count; i++)
                line.AddLast(layout.Add(new TrackStretch(++trackId, Station(tokens[i - 1].Signature), Station(tokens[i].Signature), tokens[i].Distance, 1)));
            layout.Add(line);
        }
        return layout;
    }

    // A deliberately generous estimate of the room a signature takes: the diagram reserves space from a
    // narrower average glyph, so a label that clears this one clears what is actually drawn.
    private static double HalfLabelWidth(TopologyNode node) =>
        node.Hidden ? 0.0 : node.Signature.Length * TopologyDiagram.FontSize * 0.5 / 2.0;

    private static TopologyLine Line(TopologyDiagram diagram, string number) => diagram.Lines.Single(l => l.Number == number);

    private static int RowCount(TopologyDiagram diagram) => diagram.Lines.Select(l => l.Y).Distinct().Count();

    [TestMethod]
    public void BranchesThatDoNotOverlapShareARow()
    {
        // Two short branches leaving a long main line far apart: nothing forces them onto rows of their own.
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "B P", "H Q"));

        Assert.AreEqual(2, RowCount(diagram), "The main line and both branches should need two rows only.");
        Assert.AreEqual(Line(diagram, "2").Y, Line(diagram, "3").Y, "Both branches should be on the same row.");
        Assert.IsTrue(Line(diagram, "2").Y > Line(diagram, "1").Y, "A branch is drawn below the line it leaves.");
    }

    [TestMethod]
    public void BranchesThatOverlapAreDrawnOnRowsOfTheirOwn()
    {
        // The second branch leaves the main line one station further on, under the length of the first.
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "B P Q R S T", "C X"));

        Assert.AreEqual(3, RowCount(diagram));
        Assert.IsTrue(Line(diagram, "2").Y > Line(diagram, "3").Y,
            "The branch leaving further along is drawn first, so the long one behind it is pushed below.");
    }

    [TestMethod]
    public void ABranchNeverFallsThroughALineOnItsWayDown()
    {
        // The long branch off B lies right across the path of the short branch off C: whichever row the
        // short one is put on, its connector reaches that row at the same place. It is only clear of the
        // long branch because the long one is pushed below it.
        AssertNoConnectorCrossesALine(TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "B P Q R S T", "C X")));

        // The diagram of a real layout: six stretches, two of them branching off a branch.
        AssertNoConnectorCrossesALine(TopologyDiagram.Build(CreateLayout(
            "Klb Forgr1@13.3 Rub@15.5 Ful@22.5 Mkd@23.5 Forgr2@19.5 Sbg@13.5",
            "Forgr1 Hvl@6 Mir@3",
            "Rub Kst@13 Cb@7.5 Alk@6",
            "Forgr2 Htås@16.8",
            "Ful Ins@8 Idp@5",
            "Cb Ing@7.5 FulT@4")));
    }

    // A connector drops at 45° from its junction to the line it leads to. Every row it passes on the way
    // must be clear where it crosses, or the diagram shows a branch running straight through another line.
    private static void AssertNoConnectorCrossesALine(TopologyDiagram diagram)
    {
        foreach (var connector in diagram.Connectors)
        {
            foreach (var line in diagram.Lines.Where(l => l.Y > connector.JunctionY && l.Y < connector.LineY))
            {
                var x = connector.JunctionX
                    + ((line.Y - connector.JunctionY) / (connector.LineY - connector.JunctionY) * (connector.LineX - connector.JunctionX));
                Assert.IsFalse(x > line.StartX && x < line.EndX,
                    $"The connector of the line coloured {connector.Color} crosses line {line.Number} at {x:0}.");
            }
        }
    }

    [TestMethod]
    public void PackingLinesOntoSharedRowsLowersTheDiagram()
    {
        var packed = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "B P", "H Q"));
        var spread = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "B P Q R S T", "C X"));

        Assert.IsTrue(packed.Height < spread.Height, "Three lines on two rows must be lower than three lines on three rows.");
    }

    [TestMethod]
    public void LinesSharingARowDoNotOverlap()
    {
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "B P", "E Q", "H R"));

        Assert.AreEqual(2, RowCount(diagram), "Three well-separated branches off one main line fit on a single row below it.");
        foreach (var row in diagram.Lines.GroupBy(l => l.Y))
        {
            var ordered = row.OrderBy(l => l.StartX).ToList();
            for (var i = 1; i < ordered.Count; i++)
                Assert.IsTrue(ordered[i].StartX > ordered[i - 1].EndX,
                    $"Line {ordered[i].Number} starts before line {ordered[i - 1].Number} ends on the same row.");
        }
    }

    [TestMethod]
    public void ConnectorsAreDrawnAtFortyFiveDegrees()
    {
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "F P", "X Y C"));

        Assert.AreEqual(2, diagram.Connectors.Count);
        foreach (var connector in diagram.Connectors)
            Assert.AreEqual(Math.Abs(connector.LineY - connector.JunctionY), Math.Abs(connector.LineX - connector.JunctionX), 0.001,
                "A connector's horizontal run must equal its vertical drop.");
    }

    [TestMethod]
    public void ConnectorContinuesAlongTheBranchItLeadsTo()
    {
        // Line 2 leads out of the main line and runs to the right of its corner; line 3 leads into the main
        // line and runs to the left of it. The stub must follow the branch either way, so that the renderer
        // can draw the corner as a join rather than butt two strokes together.
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "F P", "X Y C"));

        var leadsOut = diagram.Connectors.Single(c => c.Color == Line(diagram, "2").Color);
        Assert.AreEqual(Line(diagram, "2").StartX, leadsOut.LineX, 0.001, "The connector meets the branch where its line begins.");
        Assert.IsTrue(leadsOut.StubX > leadsOut.LineX, "The stub follows a leading-out branch to the right.");

        var leadsInto = diagram.Connectors.Single(c => c.Color == Line(diagram, "3").Color);
        Assert.AreEqual(Line(diagram, "3").EndX, leadsInto.LineX, 0.001, "The connector meets the branch where its line ends.");
        Assert.IsTrue(leadsInto.StubX < leadsInto.LineX, "The stub follows a leading-into branch to the left.");
    }

    [TestMethod]
    public void StubNeverReachesBeyondTheBranchItFollows()
    {
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "F P", "X Y C"));

        foreach (var connector in diagram.Connectors)
        {
            var line = diagram.Lines.Single(l => l.Color == connector.Color);
            Assert.IsTrue(connector.StubX >= line.StartX - 0.001 && connector.StubX <= line.EndX + 0.001,
                $"The stub of line {line.Number} sticks out beyond the line itself.");
        }
    }

    [TestMethod]
    public void StationsCloseTogetherAreSpacedOutSoTheirSignaturesDoNotOverlap()
    {
        // A branch that runs a long way out to a yard and then has three stations almost on top of each
        // other: at true scale the last three signatures would be printed over one another.
        var diagram = TopologyDiagram.Build(CreateLayout("Klb Forgr1 Rub Ful Mkd Forgr2 Sbg", "Rub Kst@30 Cb@0.5 Alk@0.5"));

        foreach (var line in diagram.Lines)
        {
            var nodes = line.Nodes;
            for (var i = 1; i < nodes.Count; i++)
                Assert.IsTrue(nodes[i].X - nodes[i - 1].X >= HalfLabelWidth(nodes[i - 1]) + HalfLabelWidth(nodes[i]),
                    $"The signatures of {nodes[i - 1].Signature} and {nodes[i].Signature} on line {line.Number} overlap.");
        }
    }

    [TestMethod]
    public void SpacingOutCloseStationsLeavesTheRestOfTheLineToScale()
    {
        // The first two stations are far too close for their signatures and must be pushed apart; the two
        // long hauls after them are well clear of the minimum and must keep their true proportions.
        var diagram = TopologyDiagram.Build(CreateLayout("Aa Bb@0.5 Cc@20 Dd@40"));

        var x = Line(diagram, "1").Nodes.Select(n => n.X).ToList();
        Assert.IsTrue(x[1] - x[0] > 0.5 / 40.0 * (x[3] - x[2]), "Stations too close together are pushed apart.");
        Assert.AreEqual(2.0, (x[3] - x[2]) / (x[2] - x[1]), 0.001, "Distances above the minimum keep their true proportion.");
    }

    [TestMethod]
    public void SignaturesAndLineNumbersFitInsideTheDiagram()
    {
        // Line 3 leads into the main line early on, which places it left of where a line normally starts.
        var diagram = TopologyDiagram.Build(CreateLayout("Klb Forgr1 Rub Ful Mkd Forgr2 Sbg", "Rub Kst Cb Alk", "Htås Forgr1"));

        foreach (var line in diagram.Lines)
        {
            Assert.IsTrue(line.StartX - TopologyDiagram.NumberOffset - line.Number.Length * TopologyDiagram.FontSize * 0.5 > 0.0,
                $"The number of line {line.Number} is cut off at the left edge.");
            foreach (var node in line.Nodes)
            {
                Assert.IsTrue(node.X - HalfLabelWidth(node) >= 0.0, $"Signature {node.Signature} is cut off at the left edge.");
                Assert.IsTrue(node.X + HalfLabelWidth(node) <= diagram.Width, $"Signature {node.Signature} is cut off at the right edge.");
            }
        }
    }

    [TestMethod]
    public void ASingleLineIsDrawnOnOneRow()
    {
        var diagram = TopologyDiagram.Build(CreateLayout("A B C"));

        Assert.AreEqual(1, RowCount(diagram));
        Assert.AreEqual(0, diagram.Connectors.Count);
    }
}
