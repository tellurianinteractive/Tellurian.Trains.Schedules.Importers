using System.Globalization;
using Tellurian.Trains.Schedules.Planning.Components.Shared;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies how the topology diagram stacks timetable stretches: lines that do not overlap horizontally
/// share a row so the diagram stays as compact as possible, lines that do overlap are pushed apart, and
/// every branch is linked to what it leaves by a 45° connector that continues a short way along the
/// branch's own line. Above all, nothing may cross anything else.
/// </summary>
[TestClass]
public class TopologyLayoutTests
{
    private static readonly string[] Colors = ["#000000", "#1a3a5c", "#8B0000", "#006400", "#4B0082", "#8B4513"];

    // A real layout that cannot be drawn with every branch below the line it leaves. Rub sends a branch out
    // to the right and Ful, only 17 units further on, takes one in from the left: their connectors both fall
    // at 45° and so meet less than a row below the line, whichever rows the two branches end up on. On top
    // of that, two stretches lead into Ful from the same side.
    private static readonly string[] Kolding =
    [
        "Klb Forgr1@10 Rub@12 Ful@17 Mkd@18 Forgr2@15 Sbg@10",
        "Mir Hvn@2 Forgr1@4",
        "Rub Kst@10 Cb@6 Alk@4",
        "Forgr2 Htaas@9",
        "Idp Ins@4 Ful@6",
        "FulT Ins@3 Ful@6",
    ];

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
    public void ABranchNeverCutsAcrossALineOnItsWayOut()
    {
        // The long branch off B lies right across the path of the short branch off C: whichever row the
        // short one is put on, its connector reaches that row at the same place. It is only clear of the
        // long branch because the long one is pushed below it.
        AssertNothingCrosses(TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "B P Q R S T", "C X")));

        // The diagram of a real layout: six stretches, two of them branching off a branch.
        AssertNothingCrosses(TopologyDiagram.Build(CreateLayout(
            "Klb Forgr1@13.3 Rub@15.5 Ful@22.5 Mkd@23.5 Forgr2@19.5 Sbg@13.5",
            "Forgr1 Hvl@6 Mir@3",
            "Rub Kst@13 Cb@7.5 Alk@6",
            "Forgr2 Htås@16.8",
            "Ful Ins@8 Idp@5",
            "Cb Ing@7.5 FulT@4")));

        // The same layout with three of its stretches running the other way round, which turns their
        // branches from leading out of the main line into leading into it.
        AssertNothingCrosses(TopologyDiagram.Build(CreateLayout(Kolding)));
    }

    [TestMethod]
    public void ABranchIsDrawnAboveTheLineItLeavesWhenNothingBelowIsClear()
    {
        var diagram = TopologyDiagram.Build(CreateLayout(Kolding));
        var main = Line(diagram, "1").Y;

        Assert.IsTrue(diagram.Lines.Any(l => l.Y < main), "A branch with no clear path below must be drawn above instead.");
        Assert.IsTrue(diagram.Lines.Any(l => l.Y > main), "Branches that do have a clear path below must stay below.");
        AssertNothingCrosses(diagram);
    }

    [TestMethod]
    public void BranchesMeetingAtOneStationHangOffEachOtherRatherThanOverlapping()
    {
        // Three stretches all leading into station C from the same side. Hung from C itself they would be
        // drawn one on top of another: every connector leaving a junction at 45° reaches a given row at the
        // same point, so each would run straight down the one before it and through its corner.
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "P Q C", "R S C", "T U C"));

        Assert.AreEqual(3, diagram.Connectors.Count);
        Assert.AreEqual(3, diagram.Connectors.Select(c => (c.JunctionX, c.JunctionY)).Distinct().Count(),
            "Each branch must hang off the corner of the one before it, so no two connectors start at the same point.");
        AssertNothingCrosses(diagram);
    }

    // A connector runs at 45° from its junction to the line it leads to, falling to a branch drawn below and
    // climbing to one drawn above. Nothing may lie in its way: not a line on a row it passes — that would
    // show a branch running straight through another line — and not another connector, which would show two
    // branches crossing in mid-air.
    private static void AssertNothingCrosses(TopologyDiagram diagram)
    {
        foreach (var connector in diagram.Connectors)
        {
            var top = Math.Min(connector.JunctionY, connector.LineY);
            var bottom = Math.Max(connector.JunctionY, connector.LineY);
            foreach (var line in diagram.Lines.Where(l => l.Y > top && l.Y < bottom))
            {
                var x = connector.JunctionX
                    + ((line.Y - connector.JunctionY) / (connector.LineY - connector.JunctionY) * (connector.LineX - connector.JunctionX));
                Assert.IsFalse(x >= line.StartX && x <= line.EndX,
                    $"The connector of the line coloured {connector.Color} crosses line {line.Number} at {x:0}.");
            }
        }

        for (var i = 0; i < diagram.Connectors.Count; i++)
            for (var j = i + 1; j < diagram.Connectors.Count; j++)
            {
                var a = diagram.Connectors[i];
                var b = diagram.Connectors[j];
                // Connectors that share an end meet at a station: they are chained, not crossing.
                if (Joined(a, b)) continue;
                Assert.IsFalse(Crosses(a, b),
                    $"The connectors of the lines coloured {a.Color} and {b.Color} cross each other.");
            }
    }

    private static bool Joined(TopologyConnector a, TopologyConnector b) =>
        Near(a.JunctionX, a.JunctionY, b.JunctionX, b.JunctionY)
        || Near(a.JunctionX, a.JunctionY, b.LineX, b.LineY)
        || Near(a.LineX, a.LineY, b.JunctionX, b.JunctionY);

    private static bool Near(double ax, double ay, double bx, double by) =>
        Math.Abs(ax - bx) < 0.5 && Math.Abs(ay - by) < 0.5;

    private static bool Crosses(TopologyConnector a, TopologyConnector b)
    {
        // Each endpoint of one connector falls on one side of the other's line or the other; they cross
        // only when both pairs straddle.
        static double SideOf(TopologyConnector c, double px, double py) =>
            ((c.LineX - c.JunctionX) * (py - c.JunctionY)) - ((c.LineY - c.JunctionY) * (px - c.JunctionX));

        var d1 = SideOf(b, a.JunctionX, a.JunctionY);
        var d2 = SideOf(b, a.LineX, a.LineY);
        var d3 = SideOf(a, b.JunctionX, b.JunctionY);
        var d4 = SideOf(a, b.LineX, b.LineY);
        return ((d1 > 0.0 && d2 < 0.0) || (d1 < 0.0 && d2 > 0.0))
            && ((d3 > 0.0 && d4 < 0.0) || (d3 < 0.0 && d4 > 0.0));
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
        AssertEverythingFitsInside(TopologyDiagram.Build(CreateLayout("Klb Forgr1 Rub Ful Mkd Forgr2 Sbg", "Rub Kst Cb Alk", "Htås Forgr1")));

        // A layout that has to put branches above the main line, which places them above its top row.
        AssertEverythingFitsInside(TopologyDiagram.Build(CreateLayout(Kolding)));
    }

    private static void AssertEverythingFitsInside(TopologyDiagram diagram)
    {
        foreach (var line in diagram.Lines)
        {
            Assert.IsTrue(line.Y - TopologyDiagram.FontSize - 10.0 >= 0.0, $"Line {line.Number} is drawn above the top edge.");
            Assert.IsTrue(line.Y <= diagram.Height, $"Line {line.Number} is drawn below the bottom edge.");
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
