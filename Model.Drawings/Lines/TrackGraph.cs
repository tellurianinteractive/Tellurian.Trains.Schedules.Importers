using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Drawings.Modules;
using Tellurian.Trains.Schedules.Model.Drawings.Tracks;

namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>
/// The drawn tracks of a <see cref="DrawnLayout"/> as one graph (<c>LayoutsLayerSurvey.md</c> 3.2). An edge is one route of
/// one track element of a placed module; a node is a track end, where ends within a module meet. Across a joint, a track
/// at offset <i>x</i> of one exit's end profile meets the track at −<i>x</i> of the other's, within
/// <see cref="FitTolerances.Joint"/>. Everything is in each module's own coordinates: distances follow from the order of
/// the modules, not from where they stand in a hall.
/// </summary>
/// <remarks>
/// A route is searched over directed edges, so it only continues from a track end in the way it came in: it can't run
/// in along a turnout's straight route and out along its diverging one.
/// </remarks>
internal sealed class TrackGraph
{
    /// <summary>Degrees: leaving a track end turned more than this from how it was entered is turning back.</summary>
    private const double TurningBack = 90;

    /// <summary>Millimetres: how close to the location centre's cross-section a track must pass to start a route there.</summary>
    private const double CentreTolerance = 1;

    /// <summary>Steps to find where a track crosses the centre's cross-section; a track along the main path needs one.</summary>
    private const int CentreSteps = 8;

    private readonly DrawnLayout _layout;
    private readonly LineJoins _joins;
    private readonly LineProblemLog _log;
    private readonly List<Node> _nodes = [];
    private readonly List<Edge> _edges = [];
    private readonly Dictionary<int, List<Departure>> _departures = [];
    private readonly Dictionary<int, JointCrossing> _crossings = [];
    private readonly HashSet<int> _modulesWithTracks = [];

    public TrackGraph(DrawnLayout layout, LineJoins joins, LineProblemLog log)
    {
        _layout = layout;
        _joins = joins;
        _log = log;
        foreach (var module in joins.Modules.Values) AddModule(module);
        foreach (var (exit, join) in joins.All) AddCrossings(exit, join);
    }

    /// <summary>
    /// The shortest route from the centre of <paramref name="start"/> in <paramref name="startModule"/> to the centre of
    /// <paramref name="end"/> in <paramref name="endModule"/>, through those modules and <paramref name="between"/> only.
    /// Null, without a problem, when one of the modules has no track elements or a location's centre can't be told; null
    /// with a problem when the tracks are drawn but give no route.
    /// </summary>
    public TrackRoute? Route(OperationLocation start, PlacedModule startModule, OperationLocation end, PlacedModule endModule, IReadOnlyList<int> between)
    {
        HashSet<int> allowed = [startModule.Id, endModule.Id, .. between];
        if (!allowed.All(_modulesWithTracks.Contains)) return null;
        if (CentreCandidates(start, startModule) is not { } from || CentreCandidates(end, endModule) is not { } to) return null;
        if (from.Count == 0 || to.Count == 0)
        {
            _log.Report(new(LineProblemCode.NoTrackAtCentre, from.Count == 0 ? startModule.Id : endModule.Id));
            return null;
        }
        var route = Search(from, to, allowed);
        if (route is null) _log.Report(new(LineProblemCode.NoTrackPath, startModule.Id));
        return route;
    }

    private sealed record Node(int ModuleId, Point Position);

    /// <summary>A route of a track element, from <paramref name="StartNode"/> to <paramref name="EndNode"/>.</summary>
    private sealed record Edge(int ModuleId, TrackNetwork.Line Line, int RouteNumber, int StartNode, int EndNode)
    {
        public double Length { get; } = Line.Path.Length;

        public double? Radius { get; } = Line.Path.Segments.OfType<ArcSegment>().Select(a => (double?)a.Radius).Min();

        public TrackRoutePiece Piece(double length) =>
            new(ModuleId, Line.Element.Id, RouteNumber, Line.TrackId, length, Radius);
    }

    /// <summary>A way to leave a node: along an edge, forward or backward, with the heading it starts with.</summary>
    private readonly record struct Departure(int Edge, bool IsForward, Angle Heading);

    /// <summary>Where a track end at an exit continues across the joint, and the headings out of and into the modules.</summary>
    private sealed record JointCrossing(int Node, Angle Outwards, Angle Inwards);

    /// <summary>Having run along an edge, forward or backward, to its far end.</summary>
    private readonly record struct State(int Edge, bool IsForward);

    /// <summary>A point on an edge, mm from its start, where a route starts or ends.</summary>
    private sealed record Candidate(int Edge, double At);

    private void AddModule(PlacedModule module)
    {
        var drawing = module.Module.Drawing;
        if (drawing.TrackElements.Count == 0) return;
        _modulesWithTracks.Add(module.Id);
        var routeNumbers = new Dictionary<TrackElement, int>(ReferenceEqualityComparer.Instance);
        foreach (var line in new TrackNetwork(drawing).Lines)
        {
            var number = routeNumbers[line.Element] = routeNumbers.GetValueOrDefault(line.Element) + 1;
            var start = NodeAt(module.Id, line.Path.Start);
            var end = NodeAt(module.Id, line.Path.End);
            var index = _edges.Count;
            _edges.Add(new(module.Id, line, number, start, end));
            Depart(start, new(index, true, line.Path.StartPose.Heading));
            Depart(end, new(index, false, line.Path.EndPose.Heading + Angle.Half));
        }
    }

    private void Depart(int node, Departure departure)
    {
        if (!_departures.TryGetValue(node, out var departures)) _departures[node] = departures = [];
        departures.Add(departure);
    }

    private int NodeAt(int moduleId, Point position)
    {
        if (FindNode(moduleId, position) is int existing) return existing;
        _nodes.Add(new(moduleId, position));
        return _nodes.Count - 1;
    }

    private int? FindNode(int moduleId, Point position)
    {
        var index = _nodes.FindIndex(n => n.ModuleId == moduleId && n.Position.IsCloseTo(position, TrackNetwork.JoinTolerance));
        return index < 0 ? null : index;
    }

    /// <summary>
    /// Joins the track ends at <paramref name="exit"/> to those across the joint. Each side reports its own missing
    /// track ends; tracks that don't pair up are reported from the side the line runs from.
    /// </summary>
    private void AddCrossings(ExitReference exit, LineJoin join)
    {
        var other = join.Other;
        if (!_modulesWithTracks.Contains(exit.ModuleId) || !_modulesWithTracks.Contains(other.ModuleId)) return;
        if (TracksAt(exit) is not (var fromExit, var fromTracks) || TracksAt(other) is not (var toExit, var toTracks)) return;
        var tolerance = _layout.EffectiveFitTolerances.Joint;
        foreach (var track in fromTracks)
        {
            if (FindNode(exit.ModuleId, ModuleGeometry.ToPlan(fromExit, track.Offset)) is not int node)
            {
                _log.Report(new(LineProblemCode.TrackMissingAtExit, exit.ModuleId, exit.Place));
                continue;
            }
            var partner = toTracks.FirstOrDefault(t => Math.Abs(track.Offset + t.Offset) <= tolerance);
            if (partner is null)
            {
                if (join.IsForward) _log.Report(new(LineProblemCode.TracksDontMeetAtJoint, exit.ModuleId, exit.Place));
                continue;
            }
            if (FindNode(other.ModuleId, ModuleGeometry.ToPlan(toExit, partner.Offset)) is not int otherNode) continue;
            _crossings[node] = new(otherNode, fromExit.Pose.Heading, toExit.Pose.Heading + Angle.Half);
        }
    }

    /// <summary>The drawn exit and the tracks of its end profile as built there; null when either isn't known.</summary>
    private (ModuleExit Exit, IReadOnlyList<EndProfileTrack> Tracks)? TracksAt(ExitReference reference)
    {
        if (reference.Place.In(_joins.Modules[reference.ModuleId].Module.Drawing) is not { } exit) return null;
        if (_layout.FindEndProfile(exit.EndProfile.Id) is not { } profile) return null;
        return (exit, profile.Oriented(exit.Orientation).Tracks);
    }

    /// <summary>
    /// Where the tracks of <paramref name="module"/> cross the cross-section at the location's centre: its
    /// <see cref="OperationLocation.Centre"/> along the main path, or the middle. Null when the centre can't be told: the
    /// location spans more than one module, so its centre is not in one of them, or the module has no main path.
    /// </summary>
    private List<Candidate>? CentreCandidates(OperationLocation location, PlacedModule module)
    {
        if (_joins.Modules.Values.Count(m => m.Module.OperationLocation?.Id == location.Id) != 1) return null;
        if (module.Module.Drawing.MainPath?.ToPath() is not { } main) return null;
        var centre = Math.Clamp(location.Centre * StationTrackMeasurement.MillimetresPerMetre ?? main.Length / 2, 0, main.Length);
        var point = main.PoseAt(centre).Position;
        var candidates = new List<Candidate>();
        for (var i = 0; i < _edges.Count; i++)
        {
            if (_edges[i].ModuleId != module.Id) continue;
            var path = _edges[i].Line.Path;
            var at = path.Project(point).Distance;
            var miss = double.PositiveInfinity;
            for (var step = 0; step < CentreSteps; step++)
            {
                miss = centre - main.Project(path.PoseAt(at).Position).Distance;
                if (Math.Abs(miss) <= CentreTolerance) break;
                at = Math.Clamp(at + miss, 0, path.Length);
            }
            if (Math.Abs(miss) <= CentreTolerance) candidates.Add(new(i, at));
        }
        return candidates;
    }

    /// <summary>Dijkstra over directed edges, from any start candidate to the nearest end candidate.</summary>
    private TrackRoute? Search(List<Candidate> from, List<Candidate> to, HashSet<int> allowed)
    {
        var targets = to.GroupBy(c => c.Edge).ToDictionary(g => g.Key, g => g.Select(c => c.At).ToList());
        var costs = new Dictionary<State, double>();
        var previous = new Dictionary<State, State?>();
        var startsAt = new Dictionary<State, double>();
        var queue = new PriorityQueue<State, double>();
        var best = double.PositiveInfinity;
        (State Last, State Final, double At)? finish = null;

        foreach (var candidate in from)
        {
            Offer(new(candidate.Edge, true), _edges[candidate.Edge].Length - candidate.At, null, candidate.At);
            Offer(new(candidate.Edge, false), candidate.At, null, candidate.At);
        }
        while (queue.TryDequeue(out var state, out var cost))
        {
            if (cost > costs[state]) continue;
            if (cost >= best) break;
            foreach (var (node, heading) in PositionsAfter(state))
            {
                if (!_departures.TryGetValue(node, out var departures)) continue;
                foreach (var departure in departures)
                {
                    var edge = _edges[departure.Edge];
                    if (!allowed.Contains(edge.ModuleId) || !Continues(heading, departure.Heading)) continue;
                    var next = new State(departure.Edge, departure.IsForward);
                    if (targets.TryGetValue(departure.Edge, out var ats))
                    {
                        foreach (var at in ats)
                        {
                            var total = cost + (departure.IsForward ? at : edge.Length - at);
                            if (total >= best) continue;
                            best = total;
                            finish = (state, next, at);
                        }
                    }
                    Offer(next, cost + edge.Length, state, null);
                }
            }
        }
        return finish is { } f ? Pieces(f.Last, f.Final, f.At) : null;

        void Offer(State state, double cost, State? from, double? startAt)
        {
            if (costs.TryGetValue(state, out var known) && known <= cost) return;
            costs[state] = cost;
            previous[state] = from;
            if (startAt is double at) startsAt[state] = at; else startsAt.Remove(state);
            queue.Enqueue(state, cost);
        }

        TrackRoute Pieces(State last, State final, double at)
        {
            var finalEdge = _edges[final.Edge];
            var pieces = new List<TrackRoutePiece> { finalEdge.Piece(final.IsForward ? at : finalEdge.Length - at) };
            for (State? state = last; state is { } s; state = previous[s])
            {
                var edge = _edges[s.Edge];
                pieces.Add(edge.Piece(startsAt.TryGetValue(s, out var startAt) && previous[s] is null
                    ? (s.IsForward ? edge.Length - startAt : startAt)
                    : edge.Length));
            }
            pieces.Reverse();
            return new(pieces);
        }
    }

    /// <summary>The node reached by <paramref name="state"/> and the heading there, and, at an exit, the node across the joint.</summary>
    private IEnumerable<(int Node, Angle Heading)> PositionsAfter(State state)
    {
        var edge = _edges[state.Edge];
        var path = edge.Line.Path;
        var node = state.IsForward ? edge.EndNode : edge.StartNode;
        var heading = state.IsForward ? path.EndPose.Heading : path.StartPose.Heading + Angle.Half;
        yield return (node, heading);
        if (_crossings.TryGetValue(node, out var crossing) && Continues(heading, crossing.Outwards)) yield return (crossing.Node, crossing.Inwards);
    }

    private static bool Continues(Angle arriving, Angle leaving) => Math.Abs((leaving - arriving).Signed.Degrees) < TurningBack;
}
