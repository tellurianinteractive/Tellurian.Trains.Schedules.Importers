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
            var signatures = spec.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            lineId++;
            var line = new TimetableStretch(lineId, lineId.ToString(CultureInfo.InvariantCulture))
            {
                Color = Colors[lineId % Colors.Length],
            };
            for (var i = 1; i < signatures.Length; i++)
                line.AddLast(layout.Add(new TrackStretch(++trackId, Station(signatures[i - 1]), Station(signatures[i]), 10, 1)));
            layout.Add(line);
        }
        return layout;
    }

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
        Assert.IsTrue(Line(diagram, "3").Y > Line(diagram, "2").Y, "The overlapping branch is pushed below the first.");
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
    public void ASingleLineIsDrawnOnOneRow()
    {
        var diagram = TopologyDiagram.Build(CreateLayout("A B C"));

        Assert.AreEqual(1, RowCount(diagram));
        Assert.AreEqual(0, diagram.Connectors.Count);
    }
}
