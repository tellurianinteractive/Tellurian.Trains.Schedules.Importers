using System.Globalization;
using Tellurian.Trains.Schedules.Model.Layouts.Tracks;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Walks the lines of a <see cref="DrawnLayout"/> from every location, through plain-line modules, to the next location
/// (<c>LayoutsLayerSurvey.md</c> 3.3, step 1), and finds the route along the drawn tracks of each stretch (step 2).
/// </summary>
internal sealed class LineWalker
{
    private readonly DrawnLayout _layout;
    private readonly LineProblemLog _log = new();
    private readonly LineJoins _joins;
    private readonly TrackGraph _tracks;

    public LineWalker(DrawnLayout layout)
    {
        _layout = layout;
        _joins = new(layout, _log);
        _tracks = new(layout, _joins, _log);
    }

    public LineStretchesResult Walk()
    {
        var stretches = new List<LineStretch>();
        var keys = new HashSet<string>();
        var pairs = new HashSet<(int, int)>();
        foreach (var entry in _layout.Lines.SelectMany(l => l.Entries))
        {
            if (!_joins.Modules.TryGetValue(entry.ModuleId, out var module) || LocationOf(module) is not { } location) continue;
            foreach (var place in OrderedExits(module, entry))
            {
                if (WalkFrom(location, ExitReference.To(module.Id, place)) is not { } walked || !keys.Add(Key(walked))) continue;
                if (!pairs.Add((Math.Min(walked.Start.Id, walked.End.Id), Math.Max(walked.Start.Id, walked.End.Id))))
                {
                    _log.Report(new(LineProblemCode.StretchesInParallel, walked.StartExit.ModuleId, walked.StartExit.Place));
                    continue;
                }
                stretches.Add(WithTracks(walked));
            }
        }
        return new(stretches, _log.Problems);
    }

    /// <summary>The exit the line continues from first and the one it comes in by last, so a stretch is found from its start.</summary>
    private static IEnumerable<ExitPlace> OrderedExits(PlacedModule module, LineEntry entry) =>
        LineJoins.ExitsOf(module).OrderBy(p => p == entry.Exit ? 0 : p == entry.Entry ? 2 : 1);

    private static OperationLocation? LocationOf(PlacedModule module) => module.Module.OperationLocation;

    private LineStretch? WalkFrom(OperationLocation start, ExitReference from)
    {
        if (!_joins.TryGet(from, out var first)) return null;
        var modules = new List<int>();
        var tracks = new List<int>();
        var current = from;
        var join = first;
        while (true)
        {
            CountTracks(current, tracks);
            CountTracks(join.Other, tracks);
            var next = _joins.Modules[join.Other.ModuleId];
            if (LocationOf(next) is { } end)
            {
                if (end.Id == start.Id && modules.Count == 0) return null; // a joint inside a location of several modules
                return Walked(start, from, first, end, join.Other, join, modules, tracks);
            }
            if (modules.Contains(next.Id))
            {
                _log.Report(new(LineProblemCode.LoopWithoutLocation, next.Id));
                return null;
            }
            modules.Add(next.Id);
            var onwards = LineJoins.ExitsOf(next)
                .Where(p => p != join.Other.Place)
                .Select(p => ExitReference.To(next.Id, p))
                .Where(_joins.IsJoined)
                .ToList();
            if (onwards.Count != 1)
            {
                _log.Report(new(onwards.Count == 0 ? LineProblemCode.LineEndsWithoutLocation : LineProblemCode.JunctionWithoutLocation, next.Id));
                return null;
            }
            current = onwards[0];
            _joins.TryGet(current, out join!);
        }
    }

    /// <summary>The stretch walked, turned to run in the direction of its line.</summary>
    private static LineStretch Walked(OperationLocation start, ExitReference startExit, LineJoin first, OperationLocation end, ExitReference endExit, LineJoin last, List<int> modules, List<int> tracks)
    {
        var tracksCount = tracks.Count == 0 ? (int?)null : tracks.Min();
        if (first.IsForward)
            return new()
            {
                Start = start, StartExit = startExit, End = end, EndExit = endExit, ModuleIds = modules,
                TracksCount = tracksCount, LineId = first.LineId, Position = first.Position,
            };
        modules.Reverse();
        return new()
        {
            Start = end, StartExit = endExit, End = start, EndExit = startExit, ModuleIds = modules,
            TracksCount = tracksCount, LineId = last.LineId, Position = last.Position,
        };
    }

    /// <summary>The stretch with its route along the tracks, its distance along the main paths, and its speed.</summary>
    private LineStretch WithTracks(LineStretch stretch)
    {
        var startModule = _joins.Modules[stretch.StartExit.ModuleId];
        var endModule = _joins.Modules[stretch.EndExit.ModuleId];
        var route = _tracks.Route(stretch.Start, startModule, stretch.End, endModule, stretch.ModuleIds);
        var parts = MainPathParts(stretch, startModule, endModule);
        var mainPathDistance = parts.All(p => p.Length.HasValue) ? parts.Sum(p => p.Length!.Value) / StationTrackMeasurement.MillimetresPerMetre : (double?)null;
        var weights = parts.All(p => p.Length.HasValue) || route is null
            ? parts
            : [.. route.LengthPerModule.Select(m => new Part(_joins.Modules[m.ModuleId], m.Length))];
        return stretch with { Route = route, MainPathDistance = mainPathDistance, Speed = Speed(weights) };
    }

    private static string Key(LineStretch stretch) =>
        $"{stretch.StartExit}>{stretch.EndExit}:{string.Join(',', stretch.ModuleIds.Select(id => id.ToString(CultureInfo.InvariantCulture)))}";

    private void CountTracks(ExitReference exit, List<int> tracks)
    {
        if (exit.Place.In(_joins.Modules[exit.ModuleId].Module.Drawing) is not { } drawn) return;
        if (_layout.FindEndProfile(drawn.EndProfile.Id) is not { } profile)
        {
            _log.Report(new(LineProblemCode.EndProfileMissing, exit.ModuleId, exit.Place));
            return;
        }
        if (profile.Tracks.Count > 0) tracks.Add(profile.Tracks.Count);
    }

    /// <summary>A module's part of a stretch: the module, and its length along the stretch in millimetres, if known.</summary>
    private sealed record Part(PlacedModule Module, double? Length);

    /// <summary>
    /// The parts of the stretch along the main paths, in order: the start location's module from its centre to the exit,
    /// each plain-line module whole, and the end location's module from the exit to its centre.
    /// </summary>
    private List<Part> MainPathParts(LineStretch stretch, PlacedModule startModule, PlacedModule endModule) =>
    [
        new(startModule, FromCentre(stretch.Start, startModule, stretch.StartSide)),
        .. stretch.ModuleIds.Select(id => new Part(_joins.Modules[id], MainPathLength(_joins.Modules[id]))),
        new(endModule, FromCentre(stretch.End, endModule, stretch.EndSide)),
    ];

    /// <summary>
    /// The speed of the stretch, km/h: the speed limits of its modules, each weighted by the module's part of the length
    /// of the modules that have a limit. A module without a limit doesn't count. Where a length isn't known, the
    /// limits count equally. Null when no module has a limit.
    /// </summary>
    private static int? Speed(List<Part> parts)
    {
        var limited = parts.Where(p => p.Module.Module.SpeedLimit is > 0).ToList();
        if (limited.Count == 0) return null;
        var totalLength = limited.Sum(p => p.Length ?? 0);
        var speed = limited.All(p => p.Length.HasValue) && totalLength > 0
            ? limited.Sum(p => p.Module.Module.SpeedLimit!.Value * p.Length!.Value) / totalLength
            : limited.Average(p => p.Module.Module.SpeedLimit!.Value);
        return (int)Math.Round(speed, MidpointRounding.AwayFromZero);
    }

    /// <summary>Millimetres along the main path from the location's centre to the exit on <paramref name="side"/>, if they can be told.</summary>
    private double? FromCentre(OperationLocation location, PlacedModule module, ExitSide side)
    {
        if (_joins.Modules.Values.Count(m => LocationOf(m)?.Id == location.Id) != 1) return null;
        if (MainPathLength(module) is not double length) return null;
        var centre = location.Centre * StationTrackMeasurement.MillimetresPerMetre ?? length / 2;
        return side switch
        {
            ExitSide.Left => centre,
            ExitSide.Right => length - centre,
            _ => null,
        };
    }

    private static double? MainPathLength(PlacedModule module) => module.Module.Drawing.MainPath?.ToPath().Length;
}
