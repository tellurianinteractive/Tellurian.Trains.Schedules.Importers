using System.Globalization;
using Tellurian.Trains.Schedules.Planning.Components.Shared;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies how the topology diagram places a layout's track. Above all else, every operation location is
/// drawn exactly once and every piece of track exactly once, however many timetable stretches run over
/// them and whatever cycles the layout contains; the automatic placement then spaces locations by
/// distance without printing their signatures over one another, and a location the planner has moved is
/// drawn where they put it with its track following.
/// </summary>
[TestClass]
public class TopologyLayoutTests
{
    private static readonly string[] Colors = ["#000000", "#1a3a5c", "#8B0000", "#006400", "#4B0082", "#8B4513"];

    // A real layout that cannot be drawn with every branch below the line it leaves.
    private static readonly string[] Kolding =
    [
        "Klb Forgr1@10:1 Rub@12:2 Ful@17:2 Mkd@18:2 Forgr2@15:2 Sbg@10:2",
        "Mir Hvn@2 Forgr1@4",
        "Rub Kst@10 Cb@6 Alk@4",
        "Forgr2 Htaas@9",
        "Idp Ins@4 Ful@6",
        "FulT Ins@3 Ful@6",
    ];

    // A real club layout with three cycles in it: the Vbö–Vbv–Vbr triangle at Västerborg, the Vb–Gsr–Gka–
    // Gkv–Öbg–Thn–Vb loop, and the Lgö–Sdm–Vkb–Vb–Thn–Öbg–Vpn–Lgö loop the two lines between Lgö and Vb
    // make between them. Vb, Öbg and Vbö are each reached by two different timetable stretches, and each
    // must still be drawn once.
    private static readonly string[] Ralsbiten =
    [
        "Vbc Vbö@4 Vbv@4 Tkn@2 Lek@2 Lsg@4:2 Lgö@3:2 Vpn@5:2 Öbg@3:2 Thn@3:2 Vb@15:2",
        "Mag Tkn@2 Lek@2 Lsg@4:2 Lgö@3:2 Sdm@3 Vkb@4 Vb@4",
        "Vb Gsr@4 Gka@2 Gkv@2 Öbg@2 Thn@3:2",
        "Vbv Vbr@2 Vbö@1",
    ];

    // Builds a layout from one line per spec, each spec a space-separated run of station signatures.
    // Stations of the same signature are shared between lines, which is what makes one line a branch of
    // another — and what closes a cycle where two lines meet at both ends. A signature may carry the
    // distance from the station before it as "Sig@2"; it is 10 when left out. It may also carry the number
    // of tracks on the section reaching it as "Sig@2:2"; it is single track when left out.
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
                .Select(t => (Signature: t[0], Measure: t.Length > 1 ? t[1].Split(':') : []))
                .Select(t => (
                    t.Signature,
                    Distance: t.Measure.Length > 0 ? double.Parse(t.Measure[0], CultureInfo.InvariantCulture) : 10.0,
                    Tracks: t.Measure.Length > 1 ? int.Parse(t.Measure[1], CultureInfo.InvariantCulture) : 1))
                .ToList();
            lineId++;
            var line = new TimetableStretch(lineId, lineId.ToString(CultureInfo.InvariantCulture))
            {
                Color = Colors[lineId % Colors.Length],
            };
            for (var i = 1; i < tokens.Count; i++)
            {
                var from = Station(tokens[i - 1].Signature);
                var to = Station(tokens[i].Signature);
                // Track a line before it already laid is the same piece of the layout, not a second one.
                line.AddLast(layout.StretchBetween(from, to)
                    ?? layout.Add(new TrackStretch(++trackId, from, to, tokens[i].Distance, tokens[i].Tracks)));
            }
            layout.Add(line);
        }
        return layout;
    }

    // A deliberately generous estimate of the room a signature takes: the diagram reserves space from a
    // narrower average glyph, so a label that clears this one clears what is actually drawn.
    private static double HalfLabelWidth(TopologyNode node) =>
        node.Signature.Length * TopologyDiagram.FontSize * 0.5 / 2.0;

    private static TopologyNode Node(TopologyDiagram diagram, string signature) =>
        diagram.Nodes.Single(n => n.Signature == signature);

    private static OperationLocation Location(Layout layout, string signature) =>
        layout.OperationLocations.Single(l => l.Signature == signature);

    // The section joining two locations, whichever way round the diagram happens to have drawn it.
    private static TopologySection Section(TopologyDiagram diagram, string from, string to)
    {
        var a = Node(diagram, from);
        var b = Node(diagram, to);
        return diagram.Sections.Single(s =>
            (Near(s.FromX, a.X) && Near(s.FromY, a.Y) && Near(s.ToX, b.X) && Near(s.ToY, b.Y))
            || (Near(s.FromX, b.X) && Near(s.FromY, b.Y) && Near(s.ToX, a.X) && Near(s.ToY, a.Y)));
    }

    private static bool Near(double a, double b) => Math.Abs(a - b) < 0.001;

    [TestMethod]
    public void EveryOperationLocationIsDrawnExactlyOnce()
    {
        foreach (var specs in new[] { Kolding, Ralsbiten })
        {
            var layout = CreateLayout(specs);
            var diagram = TopologyDiagram.Build(layout);

            var repeated = diagram.Nodes.GroupBy(n => n.Signature).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.AreEqual(0, repeated.Count, $"Drawn more than once: {string.Join(", ", repeated)}.");
            CollectionAssert.AreEquivalent(
                layout.OperationLocations.Select(l => l.Signature).ToArray(),
                diagram.Nodes.Select(n => n.Signature).ToArray(),
                "Every location the track reaches must be drawn, and nothing else.");
        }
    }

    [TestMethod]
    public void ALocationTwoTimetableStretchesShareIsStillDrawnOnce()
    {
        // The three the old drawing repeated. Vb is reached by all three of Rälsbiten's main stretches,
        // Öbg by two of them, and Vbö by the main line and by the Västerborg triangle.
        var diagram = TopologyDiagram.Build(CreateLayout(Ralsbiten));

        foreach (var signature in new[] { "Vb", "Öbg", "Vbö", "Thn", "Tkn", "Lgö" })
            Assert.AreEqual(1, diagram.Nodes.Count(n => n.Signature == signature), $"{signature} must be drawn once.");
    }

    [TestMethod]
    public void EveryPieceOfTrackIsDrawnExactlyOnce()
    {
        var layout = CreateLayout(Ralsbiten);
        var diagram = TopologyDiagram.Build(layout);

        Assert.AreEqual(20, layout.TrackStretches.Count, "The layout has twenty pieces of track.");
        Assert.AreEqual(layout.TrackStretches.Count, diagram.Sections.Count,
            "Track two timetable stretches run over is one piece of the layout, and is drawn once.");
    }

    [TestMethod]
    public void ACycleClosesInsteadOfRepeatingItsLocations()
    {
        // The Västerborg triangle: three locations joined by three pieces of track, each drawn once. A
        // drawing that could only make trees would have to repeat a location to close it.
        var diagram = TopologyDiagram.Build(CreateLayout(Ralsbiten));

        foreach (var (from, to) in new[] { ("Vbö", "Vbv"), ("Vbv", "Vbr"), ("Vbr", "Vbö") })
            Assert.IsNotNull(Section(diagram, from, to), $"{from}–{to} must join the two locations themselves.");
    }

    [TestMethod]
    public void TrackTwoTimetableStretchesRunOverCarriesBothColours()
    {
        // Lines 1 and 2 both run from Lek through Lsg to Lgö. That track belongs to the layout, not to
        // either of them, so it is drawn once and carries both colours for the renderer to lay over.
        var layout = CreateLayout(Ralsbiten);
        var diagram = TopologyDiagram.Build(layout);

        var shared = Section(diagram, "Lek", "Lsg");
        CollectionAssert.AreEqual(
            new[] { Colors[1], Colors[2] }, shared.Colors.ToArray(),
            "Both stretches running over the track must be carried, in stretch order.");

        var alone = Section(diagram, "Sdm", "Vkb");
        CollectionAssert.AreEqual(new[] { Colors[2] }, alone.Colors.ToArray(), "Only line 2 runs over Sdm–Vkb.");
    }

    [TestMethod]
    public void TrackNoTimetableStretchRunsOverIsStillDrawn()
    {
        // A piece of the layout no timetable stretch covers is drawn without a colour, so the gap can be
        // seen rather than being invisible for want of one.
        var layout = CreateLayout("A B C D");
        var stray = layout.Add(new TrackStretch(99, Location(layout, "D"), layout.Add(new Station(99, "E", "E")), 5.0));
        Assert.IsNotNull(stray);

        var diagram = TopologyDiagram.Build(layout);

        Assert.AreEqual(1, diagram.Nodes.Count(n => n.Signature == "E"), "The location it reaches must be drawn.");
        Assert.IsTrue(Section(diagram, "D", "E").IsUncovered, "No timetable stretch runs over D–E.");
        Assert.IsFalse(Section(diagram, "C", "D").IsUncovered, "Line 1 runs over C–D.");
    }

    [TestMethod]
    public void DoubleTrackIsCarriedThroughToEveryPieceThatHasIt()
    {
        var layout = CreateLayout(Ralsbiten);
        var diagram = TopologyDiagram.Build(layout);

        Assert.AreEqual(2, Section(diagram, "Lek", "Lsg").Tracks, "Lek–Lsg is double track.");
        Assert.AreEqual(2, Section(diagram, "Thn", "Vb").Tracks, "Thn–Vb is double track.");
        Assert.AreEqual(1, Section(diagram, "Tkn", "Lek").Tracks, "Tkn–Lek is single track.");
        Assert.AreEqual(1, Section(diagram, "Vbr", "Vbö").Tracks, "The triangle is single track.");
    }

    [TestMethod]
    public void EveryPieceOfTrackRunsBetweenTheTwoLocationsItJoins()
    {
        var layout = CreateLayout(Ralsbiten);
        var diagram = TopologyDiagram.Build(layout);
        var at = diagram.Nodes.ToDictionary(n => (Math.Round(n.X, 3), Math.Round(n.Y, 3)));

        foreach (var section in diagram.Sections)
        {
            Assert.IsTrue(at.ContainsKey((Math.Round(section.FromX, 3), Math.Round(section.FromY, 3))),
                "A piece of track must start at a location that is drawn.");
            Assert.IsTrue(at.ContainsKey((Math.Round(section.ToX, 3), Math.Round(section.ToY, 3))),
                "A piece of track must end at a location that is drawn.");
        }
    }

    [TestMethod]
    public void LocationsOnOneRowAreSpacedSoTheirSignaturesDoNotOverlap()
    {
        foreach (var specs in new[] { Kolding, Ralsbiten })
        {
            var diagram = TopologyDiagram.Build(CreateLayout(specs));
            foreach (var row in diagram.Nodes.GroupBy(n => Math.Round(n.Y, 3)))
            {
                var ordered = row.OrderBy(n => n.X).ToList();
                for (var i = 1; i < ordered.Count; i++)
                    Assert.IsTrue(
                        ordered[i].X - ordered[i - 1].X >= HalfLabelWidth(ordered[i - 1]) + HalfLabelWidth(ordered[i]),
                        $"The signatures of {ordered[i - 1].Signature} and {ordered[i].Signature} overlap.");
            }
        }
    }

    [TestMethod]
    public void SpacingOutCloseLocationsLeavesTheRestOfTheLineToScale()
    {
        // The first two locations are far too close for their signatures and must be pushed apart; the two
        // long hauls after them are well clear of the minimum and must keep their true proportions.
        var diagram = TopologyDiagram.Build(CreateLayout("Aa Bb@0.5 Cc@20 Dd@40"));

        var x = new[] { "Aa", "Bb", "Cc", "Dd" }.Select(s => Node(diagram, s).X).ToList();
        Assert.IsTrue(x[1] - x[0] > 0.5 / 40.0 * (x[3] - x[2]), "Locations too close together are pushed apart.");
        Assert.AreEqual(2.0, (x[3] - x[2]) / (x[2] - x[1]), 0.001, "Distances above the minimum keep their true proportion.");
    }

    [TestMethod]
    public void NoTrackRunsThroughASignature()
    {
        foreach (var specs in new[] { Kolding, Ralsbiten })
        {
            var diagram = TopologyDiagram.Build(CreateLayout(specs));
            foreach (var node in diagram.Nodes)
                foreach (var section in diagram.Sections)
                    Assert.IsFalse(Strikes(section, SignatureArea(node)),
                        $"Track runs through the signature of {node.Signature}, printed {node.LabelSide}.");
        }
    }

    [TestMethod]
    public void ASignatureGoesBesideItsCircleWhereTrackRunsBothUpAndDownFromIt()
    {
        // A location in the middle of a run drawn up and down the page has track over its circle and
        // track under it. Neither side is clear, and printing the signature on one of them anyway — which
        // is all a rule choosing only between over and under can do — strikes it through with the track.
        var layout = CreateLayout("A B C");
        layout.SetTopologyPosition(Location(layout, "A"), 200.0, TopologyDiagram.TopRow);
        layout.SetTopologyPosition(Location(layout, "B"), 200.0, TopologyDiagram.TopRow + TopologyDiagram.SnapY);
        layout.SetTopologyPosition(Location(layout, "C"), 200.0, TopologyDiagram.TopRow + (2 * TopologyDiagram.SnapY));

        var diagram = TopologyDiagram.Build(layout);
        var middle = Node(diagram, "B");

        Assert.IsTrue(middle.LabelSide is TopologyLabelSide.Left or TopologyLabelSide.Right,
            $"The signature must go beside the circle, not {middle.LabelSide}.");
        Assert.AreEqual(middle.LabelSide == TopologyLabelSide.Right ? "start" : "end", middle.LabelAnchor);
        foreach (var section in diagram.Sections)
            Assert.IsFalse(Strikes(section, SignatureArea(middle)), "And nothing may run through it there.");
    }

    [TestMethod]
    public void ASignatureStaysOverItsCircleWhereNothingIsInTheWay()
    {
        // The house style, and it must not be given up for a rule that moves signatures about needlessly.
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E"));

        Assert.IsTrue(diagram.Nodes.All(n => n.LabelSide == TopologyLabelSide.Above),
            "A line drawn straight across the page leaves every signature over its circle.");
        Assert.IsTrue(diagram.Nodes.All(n => n.LabelAnchor == "middle"));
    }

    [TestMethod]
    public void SignaturesDoNotOverlapEachOther()
    {
        // Moving a signature off the track must not put it over a neighbour's instead.
        foreach (var specs in new[] { Kolding, Ralsbiten })
        {
            var areas = TopologyDiagram.Build(CreateLayout(specs)).Nodes
                .Select(n => (n.Signature, Area: SignatureArea(n))).ToList();
            for (var i = 0; i < areas.Count; i++)
                for (var j = i + 1; j < areas.Count; j++)
                    Assert.IsFalse(Overlaps(areas[i].Area, areas[j].Area),
                        $"The signatures of {areas[i].Signature} and {areas[j].Signature} overlap.");
        }
    }

    // The space a signature really takes, estimated from a narrower glyph than the diagram reserves for
    // it, so anything clearing this one clears what is actually drawn.
    private static (double Left, double Right, double Top, double Bottom) SignatureArea(TopologyNode node)
    {
        var width = node.Signature.Length * TopologyDiagram.FontSize * 0.5;
        var (left, right) = node.LabelAnchor switch
        {
            "start" => (node.LabelX, node.LabelX + width),
            "end" => (node.LabelX - width, node.LabelX),
            _ => (node.LabelX - (width / 2), node.LabelX + (width / 2)),
        };
        return (left, right, node.LabelY - (TopologyDiagram.FontSize * 0.75), node.LabelY + (TopologyDiagram.FontSize * 0.15));
    }

    private static bool Overlaps(
        (double Left, double Right, double Top, double Bottom) a,
        (double Left, double Right, double Top, double Bottom) b) =>
        a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;

    // Whether a piece of track's centreline enters the space a signature takes.
    private static bool Strikes(TopologySection section, (double Left, double Right, double Top, double Bottom) area)
    {
        if (Within(section.FromX, section.FromY, area) || Within(section.ToX, section.ToY, area)) return true;
        return Crosses(section, area.Left, area.Top, area.Right, area.Top)
            || Crosses(section, area.Left, area.Bottom, area.Right, area.Bottom)
            || Crosses(section, area.Left, area.Top, area.Left, area.Bottom)
            || Crosses(section, area.Right, area.Top, area.Right, area.Bottom);
    }

    private static bool Within(double x, double y, (double Left, double Right, double Top, double Bottom) area) =>
        x >= area.Left && x <= area.Right && y >= area.Top && y <= area.Bottom;

    private static bool Crosses(TopologySection section, double cx, double cy, double dx, double dy)
    {
        static double SideOf(double ax, double ay, double bx, double by, double px, double py) =>
            ((bx - ax) * (py - ay)) - ((by - ay) * (px - ax));

        var d1 = SideOf(cx, cy, dx, dy, section.FromX, section.FromY);
        var d2 = SideOf(cx, cy, dx, dy, section.ToX, section.ToY);
        var d3 = SideOf(section.FromX, section.FromY, section.ToX, section.ToY, cx, cy);
        var d4 = SideOf(section.FromX, section.FromY, section.ToX, section.ToY, dx, dy);
        return ((d1 > 0.0 && d2 < 0.0) || (d1 < 0.0 && d2 > 0.0))
            && ((d3 > 0.0 && d4 < 0.0) || (d3 < 0.0 && d4 > 0.0));
    }

    [TestMethod]
    public void SignaturesFitInsideTheDiagram()
    {
        foreach (var specs in new[] { Kolding, Ralsbiten })
        {
            var diagram = TopologyDiagram.Build(CreateLayout(specs));
            foreach (var node in diagram.Nodes)
            {
                var area = SignatureArea(node);
                Assert.IsTrue(area.Left >= diagram.MinX, $"Signature {node.Signature} is cut off at the left edge.");
                Assert.IsTrue(area.Right <= diagram.MinX + diagram.Width, $"Signature {node.Signature} is cut off at the right edge.");
                Assert.IsTrue(area.Top >= diagram.MinY, $"Signature {node.Signature} is cut off at the top edge.");
                Assert.IsTrue(area.Bottom <= diagram.MinY + diagram.Height, $"Signature {node.Signature} is cut off at the bottom edge.");
            }
        }
    }

    [TestMethod]
    public void ADiagramThatCanBeArrangedKeepsAnEmptyRowAboveAndBelowIt()
    {
        // Framed against its own content there is nowhere above the topmost line to move a location to,
        // and since the frame is held still for the length of a drag, one dragged up there is carried
        // outside it and clipped away — it lands where it was dropped but cannot be seen going there.
        var layout = CreateLayout(Ralsbiten);
        var arrangeable = TopologyDiagram.Build(layout, withRoomToArrange: true);
        var printed = TopologyDiagram.Build(layout);

        var topmost = arrangeable.Nodes.Min(n => n.Y);
        var bottommost = arrangeable.Nodes.Max(n => n.Y);
        Assert.IsTrue(topmost - arrangeable.MinY >= TopologyDiagram.SnapY,
            "There must be a whole row of clear space above the topmost location to move one into.");
        Assert.IsTrue(arrangeable.MinY + arrangeable.Height - bottommost >= TopologyDiagram.SnapY,
            "And a whole row below it.");

        Assert.IsTrue(printed.Height < arrangeable.Height,
            "The printed booklet is looked at rather than arranged, and must not waste the paper.");
        CollectionAssert.AreEqual(
            printed.Nodes.Select(n => n.Y).ToArray(), arrangeable.Nodes.Select(n => n.Y).ToArray(),
            "Reserving room must frame the drawing differently, not draw it differently.");
    }

    [TestMethod]
    public void ALocationMovedAboveTheTopmostLineIsInsideTheFrame()
    {
        var layout = CreateLayout(Ralsbiten);
        var raised = Location(layout, "Vpn");
        var (x, y) = TopologyDiagram.Snap(300.0, TopologyDiagram.TopRow - TopologyDiagram.SnapY);
        layout.SetTopologyPosition(raised, x, y);

        var diagram = TopologyDiagram.Build(layout, withRoomToArrange: true);
        var node = Node(diagram, "Vpn");

        Assert.AreEqual(TopologyDiagram.TopRow - TopologyDiagram.SnapY, node.Y, 0.001,
            "A location must be placeable a row above the top line.");
        Assert.IsTrue(node.Y - TopologyDiagram.FontSize - 10.0 >= diagram.MinY,
            "Its signature must be inside the frame once it is there.");
        Assert.IsTrue(diagram.Nodes.Min(n => n.Y) - diagram.MinY >= TopologyDiagram.SnapY,
            "And there must still be a clear row above it, to move it up again.");
    }

    [TestMethod]
    public void AMovedLocationIsDrawnWhereItWasPutAndItsTrackFollows()
    {
        var layout = CreateLayout(Ralsbiten);
        var moved = Location(layout, "Vbr");
        layout.SetTopologyPosition(moved, 320.0, 256.0);

        var diagram = TopologyDiagram.Build(layout);
        var node = Node(diagram, "Vbr");

        Assert.AreEqual(320.0, node.X, 0.001);
        Assert.AreEqual(256.0, node.Y, 0.001);

        foreach (var (other, section) in new[] { ("Vbv", Section(diagram, "Vbv", "Vbr")), ("Vbö", Section(diagram, "Vbr", "Vbö")) })
        {
            var end = Near(section.FromX, node.X) && Near(section.FromY, node.Y) ? (section.ToX, section.ToY) : (section.FromX, section.FromY);
            Assert.AreEqual(Node(diagram, other).X, end.Item1, 0.001, $"The track to {other} must follow the location that moved.");
            Assert.AreEqual(Node(diagram, other).Y, end.Item2, 0.001);
        }
    }

    [TestMethod]
    public void MovingOneLocationLeavesTheRestWhereTheyWere()
    {
        var layout = CreateLayout(Ralsbiten);
        var before = TopologyDiagram.Build(layout).Nodes.ToDictionary(n => n.Signature, n => (n.X, n.Y));

        layout.SetTopologyPosition(Location(layout, "Mag"), 0.0, 400.0);
        var after = TopologyDiagram.Build(layout).Nodes.ToDictionary(n => n.Signature, n => (n.X, n.Y));

        foreach (var (signature, at) in before.Where(b => b.Key != "Mag"))
        {
            Assert.AreEqual(at.X, after[signature].X, 0.001, $"{signature} must not move because Mag did.");
            Assert.AreEqual(at.Y, after[signature].Y, 0.001);
        }
    }

    [TestMethod]
    public void ArrangingAutomaticallyForgetsEveryMove()
    {
        var layout = CreateLayout(Ralsbiten);
        var automatic = TopologyDiagram.Build(layout).Nodes.ToDictionary(n => n.Signature, n => (n.X, n.Y));

        layout.SetTopologyPosition(Location(layout, "Mag"), 0.0, 400.0);
        Assert.IsTrue(layout.HasTopologyPositions);
        Assert.IsTrue(layout.ClearTopologyPositions());
        Assert.IsFalse(layout.HasTopologyPositions);

        foreach (var node in TopologyDiagram.Build(layout).Nodes)
        {
            Assert.AreEqual(automatic[node.Signature].X, node.X, 0.001, $"{node.Signature} must go back where it was.");
            Assert.AreEqual(automatic[node.Signature].Y, node.Y, 0.001);
        }
    }

    [TestMethod]
    public void ADroppedLocationSnapsToTheGrid()
    {
        var (x, y) = TopologyDiagram.Snap(103.0, TopologyDiagram.TopRow + (TopologyDiagram.SnapY * 1.4));

        Assert.AreEqual(104.0, x, 0.001, "The horizontal grid is a whole number of steps from zero.");
        Assert.AreEqual(TopologyDiagram.TopRow + TopologyDiagram.SnapY, y, 0.001,
            "The vertical grid is the row spacing, so a moved location stays level with the rows it was not moved past.");

        var (already, level) = TopologyDiagram.Snap(x, y);
        Assert.AreEqual(x, already, 0.001, "Snapping what is already on the grid must not move it.");
        Assert.AreEqual(y, level, 0.001);
    }

    [TestMethod]
    public void ALayoutWithNoTrackDrawsNothing()
    {
        var diagram = TopologyDiagram.Build(new Layout { Name = "Empty" });

        Assert.AreEqual(0, diagram.Nodes.Count);
        Assert.AreEqual(0, diagram.Sections.Count);
    }

    [TestMethod]
    public void ASingleLineIsDrawnOnOneRow()
    {
        var diagram = TopologyDiagram.Build(CreateLayout("A B C"));

        Assert.AreEqual(1, diagram.Nodes.Select(n => n.Y).Distinct().Count());
        Assert.AreEqual(2, diagram.Sections.Count);
    }

    [TestMethod]
    public void BranchesThatDoNotOverlapShareARow()
    {
        // Two short branches leaving a long main line far apart: nothing forces them onto rows of their own.
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "B P", "H Q"));

        Assert.AreEqual(2, diagram.Nodes.Select(n => Math.Round(n.Y, 3)).Distinct().Count(),
            "The main line and both branches should need two rows only.");
        Assert.AreEqual(Node(diagram, "P").Y, Node(diagram, "Q").Y, 0.001, "Both branches should be on the same row.");
        Assert.IsTrue(Node(diagram, "P").Y > Node(diagram, "A").Y, "A branch is drawn below the line it leaves.");
    }

    [TestMethod]
    public void BranchesThatOverlapAreDrawnOnRowsOfTheirOwn()
    {
        var diagram = TopologyDiagram.Build(CreateLayout("A B C D E F G H I J", "B P Q R S T", "C X"));

        Assert.AreEqual(3, diagram.Nodes.Select(n => Math.Round(n.Y, 3)).Distinct().Count());
        Assert.IsTrue(Node(diagram, "X").Y > Node(diagram, "A").Y, "Both branches are drawn below the main line.");
        Assert.AreNotEqual(Math.Round(Node(diagram, "X").Y, 3), Math.Round(Node(diagram, "P").Y, 3),
            "Branches that overlap horizontally cannot share a row.");
    }
}
