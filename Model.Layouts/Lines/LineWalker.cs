using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Walks the lines of a <see cref="Layout"/> from every location, through plain-line modules, to the next location
/// (<c>LayoutsLayerSurvey.md</c> 3.3, step 1). It needs no geometry: which exit joins which follows from the line order,
/// <see cref="LineEntry.IsTurned"/>, <see cref="LineEntry.ContinuesFrom"/>, <see cref="Line.StartAt"/> and
/// <see cref="Line.EndAt"/>.
/// </summary>
internal sealed class LineWalker
{
    private const double MillimetresPerMetre = 1000;

    private readonly Layout _layout;
    private readonly Dictionary<int, LayoutModule> _modules;
    private readonly Dictionary<ExitReference, Join> _joins = [];
    private readonly List<LineProblem> _problems = [];
    private readonly HashSet<LineProblem> _reported = [];

    public LineWalker(Layout layout)
    {
        _layout = layout;
        _modules = layout.Modules.GroupBy(m => m.Id).ToDictionary(g => g.Key, g => g.First());
        foreach (var line in layout.Lines) AddJoins(line);
    }

    public LineStretchesResult Walk()
    {
        var stretches = new List<LineStretch>();
        var keys = new HashSet<string>();
        var pairs = new HashSet<(int, int)>();
        foreach (var entry in _layout.Lines.SelectMany(l => l.Entries))
        {
            if (!_modules.TryGetValue(entry.ModuleId, out var module) || LocationOf(module) is not { } location) continue;
            foreach (var place in OrderedExits(module, entry))
            {
                if (WalkFrom(location, ExitReference.To(module.Id, place)) is not { } stretch || !keys.Add(Key(stretch))) continue;
                if (!pairs.Add((Math.Min(stretch.Start.Id, stretch.End.Id), Math.Max(stretch.Start.Id, stretch.End.Id))))
                {
                    Report(new(LineProblemCode.StretchesInParallel, stretch.StartExit.ModuleId, stretch.StartExit.Place));
                    continue;
                }
                stretches.Add(stretch);
            }
        }
        return new(stretches, _problems);
    }

    /// <summary>A joint as seen from one of its exits: the other exit, the line it is in, and where.</summary>
    /// <param name="Other">The exit at the other side of the joint.</param>
    /// <param name="LineId">The line the joint is part of.</param>
    /// <param name="Position">The number of the joint in its line: 0 for <see cref="Line.StartAt"/>, then one per entry.</param>
    /// <param name="IsForward">The line runs from this exit to <paramref name="Other"/>.</param>
    private sealed record Join(ExitReference Other, int LineId, int Position, bool IsForward);

    private void AddJoins(Line line)
    {
        var entries = line.Entries;
        if (entries.Count == 0) return;
        if (line.StartAt is { } start) AddJoin(start, ExitReference.To(entries[0].ModuleId, entries[0].Entry), line.Id, 0);
        for (var i = 1; i < entries.Count; i++)
            AddJoin(ExitReference.To(entries[i - 1].ModuleId, entries[i - 1].Exit), ExitReference.To(entries[i].ModuleId, entries[i].Entry), line.Id, i);
        if (line.EndAt is { } end) AddJoin(ExitReference.To(entries[^1].ModuleId, entries[^1].Exit), end, line.Id, entries.Count);
    }

    private void AddJoin(ExitReference from, ExitReference to, int lineId, int position)
    {
        if (!IsValid(from) | !IsValid(to)) return;
        if (_joins.ContainsKey(from) || _joins.ContainsKey(to))
        {
            var twice = _joins.ContainsKey(from) ? from : to;
            Report(new(LineProblemCode.ExitJoinedTwice, twice.ModuleId, twice.Place));
            return;
        }
        _joins[from] = new(to, lineId, position, true);
        _joins[to] = new(from, lineId, position, false);
    }

    private bool IsValid(ExitReference exit)
    {
        if (!_modules.TryGetValue(exit.ModuleId, out var module))
        {
            Report(new(LineProblemCode.ModuleMissing, exit.ModuleId));
            return false;
        }
        if (ExitsOf(module).Contains(exit.Place)) return true;
        Report(new(LineProblemCode.ExitMissing, exit.ModuleId, exit.Place));
        return false;
    }

    /// <summary>The exits of the module's drawing; a module drawn without exits is taken to have one at each end.</summary>
    private static IReadOnlyList<ExitPlace> ExitsOf(LayoutModule module)
    {
        var drawing = module.Module.Drawing;
        return drawing.Exits.Count == 0
            ? [ExitPlace.Left, ExitPlace.Right]
            : [.. drawing.Exits.Select(e => ExitPlace.Of(drawing, e)).OfType<ExitPlace>()];
    }

    /// <summary>The exit the line continues from first and the one it comes in by last, so a stretch is found from its start.</summary>
    private static IEnumerable<ExitPlace> OrderedExits(LayoutModule module, LineEntry entry) =>
        ExitsOf(module).OrderBy(p => p == entry.Exit ? 0 : p == entry.Entry ? 2 : 1);

    private static OperationLocation? LocationOf(LayoutModule module) => module.Module.OperationLocation;

    private LineStretch? WalkFrom(OperationLocation start, ExitReference from)
    {
        if (!_joins.TryGetValue(from, out var first)) return null;
        var modules = new List<int>();
        var tracks = new List<int>();
        var current = from;
        var join = first;
        while (true)
        {
            CountTracks(current, tracks);
            CountTracks(join.Other, tracks);
            var next = _modules[join.Other.ModuleId];
            if (LocationOf(next) is { } end)
            {
                if (end.Id == start.Id && modules.Count == 0) return null; // a joint inside a location of several modules
                return Stretch(start, from, first, end, join.Other, join, modules, tracks);
            }
            if (modules.Contains(next.Id))
            {
                Report(new(LineProblemCode.LoopWithoutLocation, next.Id));
                return null;
            }
            modules.Add(next.Id);
            var onwards = ExitsOf(next)
                .Where(p => p != join.Other.Place)
                .Select(p => ExitReference.To(next.Id, p))
                .Where(_joins.ContainsKey)
                .ToList();
            if (onwards.Count != 1)
            {
                Report(new(onwards.Count == 0 ? LineProblemCode.LineEndsWithoutLocation : LineProblemCode.JunctionWithoutLocation, next.Id));
                return null;
            }
            current = onwards[0];
            join = _joins[current];
        }
    }

    /// <summary>The stretch walked, turned to run in the direction of its line.</summary>
    private LineStretch Stretch(OperationLocation start, ExitReference startExit, Join first, OperationLocation end, ExitReference endExit, Join last, List<int> modules, List<int> tracks)
    {
        var tracksCount = tracks.Count == 0 ? (int?)null : tracks.Min();
        var parts = Parts(start, startExit, end, endExit, modules);
        var distance = Distance(parts);
        var speed = Speed(parts);
        if (first.IsForward)
            return new()
            {
                Start = start, StartExit = startExit, End = end, EndExit = endExit, ModuleIds = modules,
                TracksCount = tracksCount, MainPathDistance = distance, Speed = speed, LineId = first.LineId, Position = first.Position,
            };
        modules.Reverse();
        return new()
        {
            Start = end, StartExit = endExit, End = start, EndExit = startExit, ModuleIds = modules,
            TracksCount = tracksCount, MainPathDistance = distance, Speed = speed, LineId = last.LineId, Position = last.Position,
        };
    }

    private static string Key(LineStretch stretch) =>
        $"{stretch.StartExit}>{stretch.EndExit}:{string.Join(',', stretch.ModuleIds.Select(id => id.ToString(CultureInfo.InvariantCulture)))}";

    private void CountTracks(ExitReference exit, List<int> tracks)
    {
        if (exit.Place.In(_modules[exit.ModuleId].Module.Drawing) is not { } drawn) return;
        if (_layout.FindEndProfile(drawn.EndProfile.Id) is not { } profile)
        {
            Report(new(LineProblemCode.EndProfileMissing, exit.ModuleId, exit.Place));
            return;
        }
        if (profile.Tracks.Count > 0) tracks.Add(profile.Tracks.Count);
    }

    /// <summary>A module's part of a stretch: the module, and its length along the main path in millimetres, if known.</summary>
    private sealed record Part(LayoutModule Module, double? Length);

    /// <summary>
    /// The parts of the stretch in order: the start location's module from its centre to the exit, each plain-line
    /// module whole, and the end location's module from the exit to its centre.
    /// </summary>
    private List<Part> Parts(OperationLocation start, ExitReference startExit, OperationLocation end, ExitReference endExit, List<int> modules) =>
    [
        new(_modules[startExit.ModuleId], FromCentre(start, startExit)),
        .. modules.Select(id => new Part(_modules[id], MainPathLength(_modules[id]))),
        new(_modules[endExit.ModuleId], FromCentre(end, endExit)),
    ];

    /// <summary>The length of the stretch in metres, when every part's length is known.</summary>
    private static double? Distance(List<Part> parts) =>
        parts.All(p => p.Length.HasValue) ? parts.Sum(p => p.Length!.Value) / MillimetresPerMetre : null;

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

    /// <summary>Millimetres along the main path from the location's centre to <paramref name="exit"/>, if they can be told.</summary>
    private double? FromCentre(OperationLocation location, ExitReference exit)
    {
        if (_modules.Values.Count(m => LocationOf(m)?.Id == location.Id) != 1) return null;
        if (MainPathLength(_modules[exit.ModuleId]) is not double length) return null;
        var centre = location.Centre * MillimetresPerMetre ?? length / 2;
        return exit.Side switch
        {
            ExitSide.Left => centre,
            ExitSide.Right => length - centre,
            _ => null,
        };
    }

    private static double? MainPathLength(LayoutModule module) => module.Module.Drawing.MainPath?.ToPath().Length;

    private void Report(LineProblem problem)
    {
        if (_reported.Add(problem)) _problems.Add(problem);
    }
}
